using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.GameServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Flurl.Http;

namespace Blish_HUD.Content {

    public class DatAssetCache : ServiceModule<ContentService> {

        private static readonly Logger Logger = Logger.GetLogger<DatAssetCache>();

        private const string ASSETSERV_HOST = "https://assets.gw2dat.com";

        private const string ASSETCACHE_PATH = "assets/";
        private const string METADATA_FILE   = "metadata.gz";

        private const double RETRY_COUNT  = 5;
        private const int    RETRY_DELAY  = 2000;
        private const double RETRY_RELOAD = 5000d;

        private Point[]                           _textureSizes;
        private Dictionary<int, TextureReference> _textureReferences;
        private Texture2D[]                       _transparentTextures;

        private double _retryTokens = RETRY_COUNT;

        private class TextureReference {

            public int SizeReference { get; }

            public WeakReference<AsyncTexture2D> Texture { get; set; }

            public TextureReference(int sizeReference) {
                this.SizeReference = sizeReference;
            }

        }

        private readonly string _assetCachePath;

        public DatAssetCache(ContentService service) : base(service) {
            _assetCachePath = DirectoryUtil.RegisterDirectory(DirectoryUtil.CachePath, ASSETCACHE_PATH);

            // We must load ASAP
            EarlyLoad();
        }

        private static Stream LoadFallbackMetadataStream() {
            var datReader = new ZipArchiveReader(ApplicationSettings.Instance.RefPath);

            if (datReader.FileExists(METADATA_FILE)) {
                try {
                    return datReader.GetFileStream(METADATA_FILE);
                } catch (Exception ex) {
                    Logger.Warn(ex, "Failed to load {metadataFile} from the ref.dat.", METADATA_FILE);
                }
            }

            return Stream.Null;
        }

        private Stream LoadMetadataStream() {
            string metadataCache = Path.Combine(_assetCachePath, METADATA_FILE);

            var metadataStream = Stream.Null;

            try {
                // We block for this on purpose
                byte[] rawMetadata = $"{ASSETSERV_HOST}/{METADATA_FILE}".GetBytesAsync().GetAwaiter().GetResult();

                File.WriteAllBytes(metadataCache, rawMetadata);
                metadataStream = new MemoryStream(rawMetadata);
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to load asset metadata.  Will attempt to load from local cache instead");
                metadataStream = File.Exists(metadataCache)
                                     // Use the last one successfully downloaded
                                     ? File.Open(metadataCache, FileMode.Open, FileAccess.Read, FileShare.Read)
                                     // Yikes, use the one we ship with
                                     : LoadFallbackMetadataStream(); 
            }

            return metadataStream;
        }

        private void EarlyLoad() {
            using var metadataStream = LoadMetadataStream();

            if (metadataStream.Length == 0) {
                Logger.Warn("Failed to load asset metadata.  Textures won't be loaded.");

                _textureReferences   = new Dictionary<int, TextureReference>(0);
                _textureSizes        = Array.Empty<Point>();
                _transparentTextures = Array.Empty<Texture2D>();

                return;
            }

            using var gzipStream     = new GZipStream(metadataStream, CompressionMode.Decompress);
            using var parser         = new BinaryReader(gzipStream);

            // BinaryReader to keep things fairly readable

            _textureReferences = new Dictionary<int, TextureReference>(parser.ReadInt32());

            int sizeCount = parser.ReadInt32();

            _textureSizes        = new Point[sizeCount];
            _transparentTextures = new Texture2D[sizeCount];

            for (int sizeIndex = 0; sizeIndex < sizeCount; sizeIndex++) {
                int width  = parser.ReadInt32();
                int height = parser.ReadInt32();

                _textureSizes[sizeIndex]        = new Point(width, height);
                _transparentTextures[sizeIndex] = new Texture2D(BlishHud.Instance.GraphicsDevice /* This is safe since we're loading early on the main thread */, width, height);
                _transparentTextures[sizeIndex].SetData(Enumerable.Repeat(Color.Transparent, width * height).ToArray());

                int assetCount = parser.ReadInt32();

                for (int assetIndex = 0; assetIndex < assetCount; assetIndex++) {
                    _textureReferences.Add(parser.ReadInt32(), new TextureReference(sizeIndex));
                }
            }
        }

        public override void Load() {
            GameService.Debug.OverlayTexts.Add("LoadedAssetTextures", ReportDebug);
        }

        private double _lastDebugReport = 0;
        private string _lastDebugString = string.Empty;
        private string ReportDebug(GameTime gameTime) {
            // This reporting isn't particularly fast, so we only do it every 2 seconds (only gets called in debug, anyways)
            if (gameTime.TotalGameTime.TotalMilliseconds - _lastDebugReport > 2000) {
                _lastDebugString = null;
                _lastDebugReport = gameTime.TotalGameTime.TotalMilliseconds;
            }

            return _lastDebugString ??= "Loaded Asset Textures: " + _textureReferences.Values.Count(tf => 
                                                                                                          tf.Texture != null 
                                                                                                       && tf.Texture.TryGetTarget(out var texture) 
                                                                                                       && !texture.IsDisposed);
        }

        public override void Update(GameTime gameTime) {
            _retryTokens = Math.Min(_retryTokens + gameTime.ElapsedGameTime.TotalMilliseconds / RETRY_RELOAD, RETRY_COUNT);
        }

        private static async Task<Texture2D> LoadTextureFromFileSystem(string path) {
            if (Program.IsMainThread) {
                await Task.Yield();
            }

            using var sourceStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            return TextureUtil.FromStreamPremultiplied(sourceStream);
        }

        private static async Task<Texture2D> LoadTextureFromServ(string path, int assetId) {
            byte[] rawAsset = await $"{ASSETSERV_HOST}/{assetId}.png".GetBytesAsync();

            // Save to local cache for future requests
            using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write, 4096, FileOptions.Asynchronous);
            await fileStream.WriteAsync(rawAsset, 0, rawAsset.Length);

            return TextureUtil.FromStreamPremultiplied(new MemoryStream(rawAsset));
        }

        private AsyncTexture2D LoadTexture(int assetId, TextureReference textureReference) {
            string textureName  = $"{assetId}.png";
            string textureDir   = Path.Combine(_assetCachePath, $"{textureName[0]}");
            string localTexture = Path.Combine(textureDir,      textureName);

            Directory.CreateDirectory(textureDir);

            var texture = new AsyncTexture2D(_transparentTextures[textureReference.SizeReference]);

            async void HandleResponse(Task<Texture2D> textureResponse) {
                var loadedTexture = ContentService.Textures.Error;

                if (textureResponse.Exception == null) {
                    loadedTexture = textureResponse.Result;
                } else {
                    Logger.Warn(textureResponse.Exception, "Attempt to read cached texture {localTexture} failed.", localTexture);

                    try {
                        // Clear potentially corrupt textures
                        if (File.Exists(localTexture)) {
                            File.Delete(localTexture);
                        }
                    } catch (Exception ex) {
                        Logger.Warn(ex, "Failed to delete cached texture.");
                    }

                    if (_retryTokens > 1) {
                        _retryTokens--;
                        await Task.Delay(RETRY_DELAY);

                        LoadTextureFromServ(localTexture, assetId).ContinueWith(HandleResponse);
                        return;
                    }
                }

                texture.SwapTexture(loadedTexture);
            }

            if (File.Exists(localTexture)) {
                LoadTextureFromFileSystem(localTexture).ContinueWith(HandleResponse);
            } else {
                LoadTextureFromServ(localTexture, assetId).ContinueWith(HandleResponse);
            }

            return texture;
        }

        /// <summary>
        /// Returns an <see cref="AsyncTexture2D"/> which will swap to the specified asset texture
        /// or <c>null</c> if no such asset texture exists.
        /// </summary>
        public AsyncTexture2D GetTextureFromAssetId(int assetId) {
            if (_textureReferences.TryGetValue(assetId, out var textureReference)) {
                lock (textureReference) {
                    AsyncTexture2D texture = null;

                    if (textureReference.Texture == null) {
                        textureReference.Texture = new WeakReference<AsyncTexture2D>(texture = LoadTexture(assetId, textureReference));
                    } else if (!textureReference.Texture.TryGetTarget(out texture) || texture.Texture.IsDisposed) {
                        textureReference.Texture.SetTarget(texture = LoadTexture(assetId, textureReference));
                    }

                    return texture;
                }
            }

            Logger.Info("Failed to get assetId: " + assetId);

            return null;
        }

        /// <summary>
        /// Returns <c>true</c> and assigns an <see cref="AsyncTexture2D"/> to
        /// <paramref name="texture"/> which will swap to the specified asset texture
        /// or <c>false</c> if no such asset texture exists.
        /// </summary>
        public bool TryGetTextureFromAssetId(int assetId, out AsyncTexture2D texture) {
            texture = GetTextureFromAssetId(assetId);

            return texture != null;
        }

    }
}
