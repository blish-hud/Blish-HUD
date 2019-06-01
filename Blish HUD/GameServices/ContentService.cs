using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Framework.Content;

namespace Blish_HUD {
    public class ContentService:GameService {

        private const string REF_PATH = "ref.dat";

        public static class Colors {
            public static readonly Color ColonialWhite = Color.FromNonPremultiplied(255, 238, 187, 255);
            public static readonly Color Chardonnay = Color.FromNonPremultiplied(255, 204, 119, 255);
            public static readonly Color OldLace = Color.FromNonPremultiplied(253, 246, 225, 255);

            public static readonly Color DullColor = Color.FromNonPremultiplied(150, 150, 150, 255);

            public static Color Darkened(float amt) {
                return Color.FromNonPremultiplied((int)(amt * 255), (int)(amt * 255), (int)(amt * 255), 255);
            }

        }

        public static class Textures {
            public static Texture2D Error { get; private set; }
            public static Texture2D Pixel { get; private set; }

            public static void Load() {
                Error = Content.GetTexture(@"common\error");

                Pixel = new Texture2D(Graphics.GraphicsDevice, 1, 1);
                Pixel.SetData(new[] { Color.White });
            }
        }

        private BitmapFont _defaultFont12;
        public BitmapFont DefaultFont12 => _defaultFont12 ?? (_defaultFont12 = GetFont(FontFace.Menomonia, FontSize.Size12, FontStyle.Regular));

        private BitmapFont _defaultFont14;
        public BitmapFont DefaultFont14 => _defaultFont14 ?? (_defaultFont14 = GetFont(FontFace.Menomonia, FontSize.Size14, FontStyle.Regular));

        private BitmapFont _defaultFont16;
        public BitmapFont DefaultFont16 => _defaultFont16 ?? (_defaultFont16 = GetFont(FontFace.Menomonia, FontSize.Size16, FontStyle.Regular));

        private BitmapFont _defaultFont32;
        public BitmapFont DefaultFont32 => _defaultFont32 ?? (_defaultFont32 = GetFont(FontFace.Menomonia, FontSize.Size32, FontStyle.Regular));

        public enum FontFace {
            Menomonia
        }

        public enum FontSize {
            Size8 = 8,
            Size11 = 11,
            Size12 = 12,
            Size14 = 14,
            Size16 = 16,
            Size18 = 18,
            Size20 = 20,
            Size22 = 22,
            Size24 = 24,
            Size32 = 32,
            Size34 = 34,
            Size36 = 36,
        }

        public enum FontStyle {
            Regular,
            Italic
        }

        private Dictionary<string, SoundEffect> _loadedSoundEffects;
        private Dictionary<string, BitmapFont> _loadedBitmapFonts;
        private Dictionary<string, Texture2D> _loadedTextures;
        private Dictionary<string, Stream> _loadedFiles;

        private Dictionary<string, Texture2D> _mapLoadedTextures;

        //private IAppCache contentCache = new CachingService();

        private ContentManager _contentManager;

        protected override void Initialize() {
            _contentManager = Overlay.Content;

            _loadedSoundEffects = new Dictionary<string, SoundEffect>();
            _loadedBitmapFonts = new Dictionary<string, BitmapFont>();
            _loadedTextures = new Dictionary<string, Texture2D>();
            _loadedFiles = new Dictionary<string, Stream>();
        }

        protected override void Load() {
            Textures.Load();
        }

        public void PlaySoundEffectByName(string soundName) {
            if (!_loadedSoundEffects.ContainsKey(soundName))
                _loadedSoundEffects.Add(soundName, _contentManager.Load<SoundEffect>($"{soundName}"));

            // TODO: Volume was 0.25f - changing to 0.125 until a setting can be exposed in the UI
            _loadedSoundEffects[soundName].Play(0.125f, 0, 0);
        }
        // Used while debugging since it's easier
        private static Texture2D TextureFromFile(GraphicsDevice gc, string filepath) {
            if (File.Exists(filepath)) {
                using (var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    return Texture2D.FromStream(gc, fileStream);
                }
            } else return null;
        }
        private static Texture2D TextureFromFileSystem(GraphicsDevice gc, string filepath) {
            if (!File.Exists(REF_PATH)) {
                Console.WriteLine($"{REF_PATH} is missing!  Lots of assets will be missing!");
                return null;
            }
            
            using (var refFs = new FileStream(REF_PATH, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using (var refArchive = new ZipArchive(refFs, ZipArchiveMode.Read)) {
                    var refEntry = refArchive.GetEntry(filepath);

                    if (refEntry != null) {
                        using (var textureStream = refEntry.Open()) {
                            var textureCanSeek = new MemoryStream();
                            textureStream.CopyTo(textureCanSeek);

                            return Texture2D.FromStream(gc, textureCanSeek);
                        }
                    } else {
                        #if DEBUG
                        Directory.CreateDirectory(@"ref\to-include");

                        // Makes it easy to know what's in use so that it can be added to the ref archive later
                        if (File.Exists($@"ref\{filepath}")) File.Copy($@"ref\{filepath}", $@"ref\to-include\{filepath}", true);

                        return TextureFromFile(gc, $@"ref\{filepath}");
                        #endif
                    }

                    return null;
                }
            }
        }


        public MonoGame.Extended.TextureAtlases.TextureAtlas GetTextureAtlas(string textureAtlasName) {
            return _contentManager.Load<MonoGame.Extended.TextureAtlases.TextureAtlas>(textureAtlasName);
        }

        public void PurgeTextureCache(string textureName) {
            _loadedTextures.Remove(textureName);
        }

        public Texture2D GetTexture(string textureName) {
            return GetTexture(textureName, Textures.Error);
        }

        public void CacheTexture(string texturePath) {
            string cacheFolder = Path.Combine(FileSrv.BasePath, "cache");
            string spritesFolder = Path.Combine(cacheFolder, "sprites");
            string tempFolder = Path.Combine(cacheFolder, "_temp");
            Directory.CreateDirectory(spritesFolder);
            Directory.CreateDirectory(tempFolder);

            var pm = new MonoGame.Framework.Content.Pipeline.Builder.PipelineManager(spritesFolder, tempFolder, @"Path\_temp");
            pm.Profile = GraphicsProfile.HiDef;
            pm.CompressContent = false;
            pm.Platform = TargetPlatform.Windows;
            var builtContent = pm.BuildContent(texturePath);
            var processedContent = pm.ProcessContent(builtContent);

            //Console.WriteLine($"{texturePath} => {processedContent.GetType().Name}");
        }

        public Texture2D GetTexture(string textureName, Texture2D defaultTexture) {
            if (textureName == null) return defaultTexture;

            if (_loadedTextures.TryGetValue(textureName, out var cachedTexture))
                return cachedTexture;

            if (File.Exists(textureName)) {
                CacheTexture(textureName);
                return TextureFromFile(GameService.Graphics.GraphicsDevice, textureName);
            }

            cachedTexture = TextureFromFileSystem(GameService.Graphics.GraphicsDevice, $"{textureName}.png");

            if (cachedTexture == null) {
                try {
                    cachedTexture = _contentManager.Load<Texture2D>(textureName);
                } catch (ContentLoadException e) {
                    GameService.Debug.WriteInfoLine($"Could not find '{textureName}' precompiled or in include directory. Full error message: {e.ToString()}");
                }
            }

            cachedTexture = cachedTexture ?? defaultTexture;

            _loadedTextures.Add(textureName, cachedTexture);

            return cachedTexture;
        }

        public BitmapFont GetFont(FontFace font, FontSize size, FontStyle style) {
            string fullFontName = $"{font.ToString().ToLower()}-{((int)size).ToString()}-{style.ToString().ToLower()}";

            if (!_loadedBitmapFonts.ContainsKey(fullFontName)) {
                var loadedFont = _contentManager.Load<BitmapFont>($"fonts\\{font.ToString().ToLower()}\\{fullFontName}");
                loadedFont.LetterSpacing = -1;
                _loadedBitmapFonts.Add(fullFontName, loadedFont);

                return loadedFont;
            }

            return _loadedBitmapFonts[fullFontName];
        }
        private static Stream FileFromSystem(string filepath)
        {
            if (!File.Exists(REF_PATH))
            {
                Console.WriteLine($"{REF_PATH} is missing!  Lots of assets will be missing!");
                return null;
            }

            using (var refFs = new FileStream(REF_PATH, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var refArchive = new ZipArchive(refFs, ZipArchiveMode.Read))
                {
                    var refEntry = refArchive.GetEntry(filepath);

                    if (refEntry != null)
                    {
                        using (var fileStream = refEntry.Open())
                        {
                            var fileCanSeek = new MemoryStream();
                            fileStream.CopyTo(fileCanSeek);

                            return fileCanSeek;
                        }
                    }
                    else
                    {
#if DEBUG
                        Directory.CreateDirectory(@"ref\to-include");

                        // Makes it easy to know what's in use so that it can be added to the ref archive later
                        if (File.Exists($@"ref\{filepath}")) File.Copy($@"ref\{filepath}", $@"ref\to-include\{filepath}", true);
#endif
                    }
                    return null;
                }
            }
        }

        public Stream GetFile(string fileName) {
            if (_loadedFiles.TryGetValue(fileName, out var cachedFile))
                return cachedFile;

            cachedFile = FileFromSystem($"{fileName}");

            _loadedFiles.Add(fileName, cachedFile);

            return cachedFile;
        }

        protected override void Unload() {
            _loadedTextures.Clear();
            _loadedBitmapFonts.Clear();
            _loadedSoundEffects.Clear();
            _loadedFiles.Clear();
        }

        protected override void Update(GameTime gameTime) {
            // Unload content from memory since they aren't playing gw2 right now

            // We can't do this until all content is actually handled by the content GameService
            // If we do, some sprites won't be visible (like some font sprites that are loaded in MainLoop)
            
            //if (!GameService.GameIntegration.Gw2IsRunning) {
            //    LoadedTextures.Clear();
            //    LoadedBitmapFonts.Clear();
            //    LoadedSoundEffects.Clear();
            //    ContentManager.Unload();
            //}
        }
    }
}
