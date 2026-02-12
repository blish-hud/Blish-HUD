using Blish_HUD.GameServices;
using Blish_HUD.Graphics;
using Flurl.Http;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

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

        internal DatAssetCache(ContentService service) : base(service) {
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

        private async Task<Stream> DownloadMetadata() {
            string metadataCache = Path.Combine(_assetCachePath, METADATA_FILE);

            try {
                byte[] rawMetadata = await $"{ASSETSERV_HOST}/{METADATA_FILE}".GetBytesAsync();

                File.WriteAllBytes(metadataCache, rawMetadata);
                Logger.Debug("Metadata update successful");

                return new MemoryStream(rawMetadata);
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to download asset metadata from server.");

                return Stream.Null;
            }
        }

        private void EarlyLoad() {
            string metadataCache = Path.Combine(_assetCachePath, METADATA_FILE);

            // Use either existing asset metadata cache or fallback to ref.dat
            if (File.Exists(metadataCache)) {
                using Stream localMetadataStream = File.Open(metadataCache, FileMode.Open, FileAccess.Read, FileShare.Read);
                ProcessMetadataStream(localMetadataStream);
                Logger.Debug("Local asset metadata loaded.");
            } else {
                using Stream metadataStream = LoadFallbackMetadataStream();
                ProcessMetadataStream(metadataStream);
                Logger.Debug("Fallback asset metadata loaded from ref.dat.");
            }

            // Trigger background update of asset metadata cache
            _ = Task.Run(async () => {
                try {
                    using var webMetadata = await DownloadMetadata().ConfigureAwait(false);
                    ProcessMetadataStream(webMetadata);
                } catch (Exception ex) {
                    Logger.Warn(ex, "Background asset metadata refresh from server failed.");
                }
            });
        }

        private void ProcessMetadataStream(Stream metadataStream) {
            if (metadataStream.Length == 0) {
                if (_textureReferences == null) {
                    Logger.Warn("Failed to load asset metadata. Textures won't be loaded.");

                    _textureReferences   = new Dictionary<int, TextureReference>(0);
                    _textureSizes        = Array.Empty<Point>();
                    _transparentTextures = Array.Empty<Texture2D>();
                }

                return;
            }

            byte[] data;

            using (var gzipStream = new GZipStream(metadataStream, CompressionMode.Decompress))
            using (var ms = new MemoryStream()) {
                gzipStream.CopyTo(ms);
                data = ms.GetBuffer();
            }

            int offset = 0;

            int totalTextureCount = BitConverter.ToInt32(data, offset);
            offset += 4;

            int sizeCount = BitConverter.ToInt32(data, offset);
            offset += 4;

            bool isUpdate = _textureReferences != null && _textureReferences.Count > 0;

            if (!isUpdate) {
                InitializeMetadata(data, offset, totalTextureCount, sizeCount);
            } else {
                MergeMetadata(data, offset, totalTextureCount, sizeCount);
            }
        }

        private void InitializeMetadata(byte[] data, int offset, int totalTextureCount, int sizeCount) {
            // Initial load — build everything from scratch
            _textureReferences = new Dictionary<int, TextureReference>(totalTextureCount);
            _textureSizes = new Point[sizeCount];
            _transparentTextures = new Texture2D[sizeCount];

            for (int sizeIndex = 0; sizeIndex < sizeCount; sizeIndex++) {
                int width = BitConverter.ToInt32(data, offset);
                offset += 4;
                int height = BitConverter.ToInt32(data, offset);
                offset += 4;

                _textureSizes[sizeIndex] = new Point(width, height);
                _transparentTextures[sizeIndex] = new Texture2D(BlishHud.Instance.GraphicsDevice /* This is safe since we're loading early on the main thread */, width, height);
                _transparentTextures[sizeIndex].SetData(new Color[width * height]);

                int assetCount = BitConverter.ToInt32(data, offset);
                offset += 4;

                for (int assetIndex = 0; assetIndex < assetCount; assetIndex++) {
                    _textureReferences.Add(BitConverter.ToInt32(data, offset), new TextureReference(sizeIndex));
                    offset += 4;
                }
            }
        }

        private void MergeMetadata(byte[] data, int offset, int totalTextureCount, int sizeCount) {
            // Merge update — only add new sizes and texture references
            var existingSizes = new Dictionary<Point, int>(_textureSizes.Length);
            for (int i = 0; i < _textureSizes.Length; i++) {
                existingSizes[_textureSizes[i]] = i;
            }

            var textureSizesList = new List<Point>(_textureSizes);
            var transparentTexturesList = new List<Texture2D>(_transparentTextures);

            // Copy existing references into a new dictionary so we can atomically swap later
            var mergedReferences = new Dictionary<int, TextureReference>(Math.Max(totalTextureCount, _textureReferences.Count));
            foreach (var kvp in _textureReferences) {
                mergedReferences[kvp.Key] = kvp.Value;
            }

            int addedCount = 0;

            // I'm 99% sure that GameService.Graphics will be defined by this point
            // but just in case, we'll check and fall back to direct device access if it's not
            GraphicsDeviceContext graphicsLease = default;
            GraphicsDevice graphicsDevice;

            if (GameService.Graphics != null) {
                graphicsLease = GameService.Graphics.LendGraphicsDeviceContext(true);
                graphicsDevice = graphicsLease.GraphicsDevice;
            } else {
                graphicsDevice = BlishHud.Instance.GraphicsDevice;
            }

            try {
                for (int sizeIndex = 0; sizeIndex < sizeCount; sizeIndex++) {
                    int width = BitConverter.ToInt32(data, offset);
                    offset += 4;
                    int height = BitConverter.ToInt32(data, offset);
                    offset += 4;

                    var size = new Point(width, height);

                    if (!existingSizes.TryGetValue(size, out int resolvedSizeIndex)) {
                        resolvedSizeIndex = textureSizesList.Count;
                        existingSizes[size] = resolvedSizeIndex;
                        textureSizesList.Add(size);

                        var transparentTexture = new Texture2D(graphicsDevice, width, height);
                        transparentTexture.SetData(new Color[width * height]);
                        transparentTexturesList.Add(transparentTexture);
                    }

                    int assetCount = BitConverter.ToInt32(data, offset);
                    offset += 4;

                    for (int assetIndex = 0; assetIndex < assetCount; assetIndex++) {
                        int assetId = BitConverter.ToInt32(data, offset);
                        offset += 4;

                        if (!mergedReferences.ContainsKey(assetId)) {
                            mergedReferences[assetId] = new TextureReference(resolvedSizeIndex);
                            addedCount++;
                        }
                    }
                }
            } finally {
                graphicsLease.Dispose();
            }

            // Update arrays before the dictionary so that any new size indices
            // are valid before the TextureReferences pointing to them become visible
            _textureSizes = textureSizesList.ToArray();
            _transparentTextures = transparentTexturesList.ToArray();
            _textureReferences = mergedReferences;

            Logger.Debug($"Asset metadata merge complete. Added {addedCount} new texture references ({mergedReferences.Count} total).");
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

        /// <summary>
        /// Returns the path of a cached asset texture based on its <paramref name="assetId"/>.
        /// The path is returned regardless of if a cached copy of the texture actually exists.
        /// Do not modify the texture at this path.  If you wish to read this file, ensure you
        /// specify <see cref="FileShare.ReadWrite"/> to avoid conflicting with the caching mechanism.
        /// </summary>
        public string GetLocalTexturePath(int assetId) {
            string textureName  = $"{assetId}.png";
            string textureDir   = Path.Combine(_assetCachePath, $"{textureName[0]}");
            string localTexture = Path.Combine(textureDir,      textureName);

            try {
                Directory.CreateDirectory(textureDir);
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to create directory for texture with path {texturePath}.", localTexture);
            }

            return localTexture;
        }

        /// <summary>
        /// Returns <c>true</c> and assigns the local texture path to
        /// <paramref name="texturePath"/> if the texture has been cached
        /// locally or returns <c>false</c> if no texture is found locally.
        /// </summary>
        public bool TryGetLocalTexturePath(int assetId, out string texturePath) {
            texturePath = GetLocalTexturePath(assetId);

            return File.Exists(texturePath);
        }

        private AsyncTexture2D LoadTexture(int assetId, TextureReference textureReference) {
            var texture = new AsyncTexture2D(_transparentTextures[textureReference.SizeReference]);

            bool locallyCached = TryGetLocalTexturePath(assetId, out string localTexture);

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

            if (locallyCached) {
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
