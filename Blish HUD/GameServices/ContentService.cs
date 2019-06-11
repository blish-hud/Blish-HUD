using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Blish_HUD.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Framework.Content;

namespace Blish_HUD {

    public class ContentsManager {

        private readonly IDataReader _reader;

        private readonly Dictionary<string, Texture2D>   _loadedTextures;
        private readonly Dictionary<string, SoundEffect> _loadedSoundEffects;
        private readonly Dictionary<string, Effect>      _loadedEffects;
        private readonly Dictionary<string, BitmapFont>  _loadedBitmapFonts;
        private readonly Dictionary<string, Model>       _loadedModels;

        public ContentsManager(IDataReader reader) {
            _reader         = reader;

            _loadedTextures     = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
            _loadedSoundEffects = new Dictionary<string, SoundEffect>(StringComparer.OrdinalIgnoreCase);
            _loadedEffects      = new Dictionary<string, Effect>(StringComparer.OrdinalIgnoreCase);
            _loadedBitmapFonts  = new Dictionary<string, BitmapFont>(StringComparer.OrdinalIgnoreCase);
            _loadedModels       = new Dictionary<string, Model>(StringComparer.OrdinalIgnoreCase);
        }

        public Texture2D GetTexture(string texturePath) {
            return GetTexture(texturePath, ContentService.Textures.Error);
        }

        public Texture2D GetTexture(string texturePath, Texture2D fallbackTexture) {
            using (var textureStream = _reader.GetFileStream(texturePath)) {
                if (textureStream != null) {
                    return Texture2D.FromStream(GameService.Graphics.GraphicsDevice, textureStream);
                } else {
                    return fallbackTexture;
                }
            }
        }

        public Effect GetEffect<TEffect>(string effectPath) where TEffect : Effect {
            if (GetEffect(effectPath) is TEffect effect) {
                return effect;
            }

            return null;
        }

        public Effect GetEffect(string effectPath) {
            long effectDataLength = _reader.GetFileBytes(effectPath, out byte[] effectData);

            if (effectDataLength > 0) {
                return new Effect(GameService.Graphics.GraphicsDevice, effectData, 0, (int) effectDataLength);
            }

            return null;
        }

        public SoundEffect GetSound(string soundPath) {
            using (var soundStream = _reader.GetFileStream(soundPath)) {
                if (soundStream != null)
                    return SoundEffect.FromStream(soundStream);
            }

            return null;
        }

        public BitmapFont GetBitmapFont(string fontPath) {
            throw new NotImplementedException();
        }

        public Model GetModel(string modelPath) {
            throw new NotImplementedException();
        }

    }

    public class ContentService : GameService {

        private const string REF_NAME = "ref";
        private const string REF_FILE = "ref.dat";

        #region Load Static

        private static readonly Dictionary<string, SoundEffect> _loadedSoundEffects;
        private static readonly Dictionary<string, BitmapFont>  _loadedBitmapFonts;
        private static readonly Dictionary<string, Texture2D>   _loadedTextures;
        private static readonly Dictionary<string, Stream>      _loadedFiles;

        static ContentService() {
            _loadedSoundEffects = new Dictionary<string, SoundEffect>();
            _loadedBitmapFonts  = new Dictionary<string, BitmapFont>();
            _loadedTextures     = new Dictionary<string, Texture2D>();
            _loadedFiles        = new Dictionary<string, Stream>();
        }

        #endregion

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

        public Microsoft.Xna.Framework.Content.ContentManager ContentManager => Overlay.ActiveContentManager;

        protected override void Initialize() {
            
        }

        protected override void Load() {
            Textures.Load();
        }

        public ContentsManager RegisterContents(IDataReader dataReader) {
            return new ContentsManager(dataReader.GetSubPath(REF_NAME));
        }

        public void PlaySoundEffectByName(string soundName) {
            if (!_loadedSoundEffects.ContainsKey(soundName))
                _loadedSoundEffects.Add(soundName, Overlay.ActiveContentManager.Load<SoundEffect>($"{soundName}"));

            // TODO: Volume was 0.25f - changing to 0.125 until a setting can be exposed in the UI
            _loadedSoundEffects[soundName].Play(0.125f, 0, 0);
        }

        // Used while debugging since it's easier
        private static Texture2D TextureFromFile(string filepath) {
            if (File.Exists(filepath)) {
                using (var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    return Texture2D.FromStream(Overlay.ActiveGraphicsDeviceManager.GraphicsDevice, fileStream);
                }
            } else return null;
        }

        private static Texture2D TextureFromFileSystem(string filepath) {
            if (!File.Exists(REF_FILE)) {
                Console.WriteLine($"{REF_FILE} is missing!  Lots of assets will be missing!");
                return null;
            }
            
            using (var refFs = new FileStream(REF_FILE, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using (var refArchive = new ZipArchive(refFs, ZipArchiveMode.Read)) {
                    var refEntry = refArchive.GetEntry(filepath);

                    if (refEntry != null) {
                        using (var textureStream = refEntry.Open()) {
                            var textureCanSeek = new MemoryStream();
                            textureStream.CopyTo(textureCanSeek);

                            return Texture2D.FromStream(Overlay.ActiveGraphicsDeviceManager.GraphicsDevice, textureCanSeek);
                        }
                    }

                    #if DEBUG
                    System.IO.Directory.CreateDirectory(@"ref\to-include");

                    // Makes it easy to know what's in use so that it can be added to the ref archive later
                    if (File.Exists($@"ref\{filepath}")) File.Copy($@"ref\{filepath}", $@"ref\to-include\{filepath}", true);

                    return TextureFromFile($@"ref\{filepath}");
                    #endif

                    return null;
                }
            }
        }


        public MonoGame.Extended.TextureAtlases.TextureAtlas GetTextureAtlas(string textureAtlasName) {
            return Overlay.ActiveContentManager.Load<MonoGame.Extended.TextureAtlases.TextureAtlas>(textureAtlasName);
        }

        public void PurgeTextureCache(string textureName) {
            _loadedTextures.Remove(textureName);
        }

        public Texture2D GetTexture(string textureName) {
            return GetTexture(textureName, Textures.Error);
        }

        public Texture2D GetTexture(string textureName, Texture2D defaultTexture) {
            if (textureName == null) return defaultTexture;

            if (_loadedTextures.TryGetValue(textureName, out var cachedTexture))
                return cachedTexture;

            if (File.Exists(textureName)) {
                return TextureFromFile(textureName);
            }

            cachedTexture = TextureFromFileSystem($"{textureName}.png");

            if (cachedTexture == null) {
                try {
                    cachedTexture = Overlay.ActiveContentManager.Load<Texture2D>(textureName);
                } catch (ContentLoadException e) {
                    Debug.WriteWarningLine($"Could not find '{textureName}' precompiled or in include directory. Full error message: {e.ToString()}");
                }
            }

            cachedTexture = cachedTexture ?? defaultTexture;

            _loadedTextures.Add(textureName, cachedTexture);

            return cachedTexture;
        }

        public BitmapFont GetFont(FontFace font, FontSize size, FontStyle style) {
            string fullFontName = $"{font.ToString().ToLower()}-{((int)size).ToString()}-{style.ToString().ToLower()}";

            if (!_loadedBitmapFonts.ContainsKey(fullFontName)) {
                var loadedFont = Overlay.ActiveContentManager.Load<BitmapFont>($"fonts\\{font.ToString().ToLower()}\\{fullFontName}");
                loadedFont.LetterSpacing = -1;
                _loadedBitmapFonts.Add(fullFontName, loadedFont);

                return loadedFont;
            }

            return _loadedBitmapFonts[fullFontName];
        }

        private static Stream FileFromSystem(string filepath) {
            if (!File.Exists(REF_FILE)) {
                Debug.WriteWarningLine($"{REF_FILE} is missing! Lots of assets will likely be missing!");
                return null;
            }

            using (var refFs = new FileStream(REF_FILE, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using (var refArchive = new ZipArchive(refFs, ZipArchiveMode.Read)) {
                    var refEntry = refArchive.GetEntry(filepath);

                    if (refEntry != null) {
                        using (var fileStream = refEntry.Open()) {
                            var fileCanSeek = new MemoryStream();
                            fileStream.CopyTo(fileCanSeek);

                            return fileCanSeek;
                        }
                    } else {
#if DEBUG
                        System.IO.Directory.CreateDirectory(@"ref\to-include");

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
