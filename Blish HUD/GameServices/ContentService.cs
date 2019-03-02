using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Caching;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

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

            public static class GW2Colors {
                public static readonly Color White = Color.FromNonPremultiplied(189, 186, 185, 255);
                public static readonly Color Red = Color.FromNonPremultiplied(135, 0, 10, 255);
                public static readonly Color Blueberry = Color.FromNonPremultiplied(36, 65, 122, 255);
                public static readonly Color AncientSilver = Color.FromNonPremultiplied(157, 142, 108, 255);
                public static readonly Color Abyss = Color.FromNonPremultiplied(26, 24, 27, 255);
                public static readonly Color Green = Color.FromNonPremultiplied(28, 90, 45, 255);
                public static readonly Color Gray = Color.FromNonPremultiplied(72, 69, 70, 255);
                public static readonly Color Fuchsia = Color.FromNonPremultiplied(117, 25, 67, 255);
                public static readonly Color Oxblood = Color.FromNonPremultiplied(55, 4, 0, 255);
                public static readonly Color Orange = Color.FromNonPremultiplied(152, 63, 23, 255);
            }
        }

        public static class Textures {
            public static Texture2D Error { get; private set; }
            public static Texture2D Pixel { get; private set; }

            public static void Load() {
                Error = GameService.Content.GetTexture(@"common\error");

                Pixel = new Texture2D(GameService.Graphics.GraphicsDevice, 1, 1);
                Pixel.SetData(new[] { Color.White });
            }
        }

        public BitmapFont DefaultFont12 => GetFont(FontFace.Menomonia, FontSize.Size12, FontStyle.Regular);
        public BitmapFont DefaultFont14 => GetFont(FontFace.Menomonia, FontSize.Size14, FontStyle.Regular);

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
            Bold,
            Italic
        }

        private Dictionary<string, SoundEffect> _loadedSoundEffects;
        private Dictionary<string, BitmapFont> _loadedBitmapFonts;
        private Dictionary<string, Texture2D> _loadedTextures;

        //private IAppCache contentCache = new CachingService();

        private ContentManager _contentManager;

        protected override void Initialize() {
            _contentManager = Overlay.Content;

            _loadedSoundEffects = new Dictionary<string, SoundEffect>();
            _loadedBitmapFonts = new Dictionary<string, BitmapFont>();
            _loadedTextures = new Dictionary<string, Texture2D>();
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

        public MonoGame.Extended.TextureAtlases.TextureAtlas GetTextureAtlas2(string textureAtlasName) {
            return _contentManager.Load<MonoGame.Extended.TextureAtlases.TextureAtlas>(textureAtlasName);
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
                return TextureFromFile(GameService.Graphics.GraphicsDevice, textureName);
            }

            cachedTexture = TextureFromFileSystem(GameService.Graphics.GraphicsDevice, $"{textureName}.png");

            if (cachedTexture == null) {
                try {
                    cachedTexture = _contentManager.Load<Texture2D>(textureName);
                } catch (ContentLoadException e) {
                    GameService.Debug.WriteInfoLine($"Could not find '{textureName}' precompiled or in include directory. Full: {e.Message}");
                }
            }

            cachedTexture = cachedTexture ?? defaultTexture;

            _loadedTextures.Add(textureName, cachedTexture);

            return cachedTexture;
        }

        public BitmapFont GetFont(FontFace font, FontSize size, FontStyle style) {
            string fullFontName = $"{font.ToString().ToLower()}-{((int)size).ToString()}-{style.ToString().ToLower()}";

            if (!_loadedBitmapFonts.ContainsKey(fullFontName))
                _loadedBitmapFonts.Add(fullFontName, _contentManager.Load<BitmapFont>($"fonts\\{font.ToString().ToLower()}\\{fullFontName}"));

            return _loadedBitmapFonts[fullFontName];
        }

        protected override void Unload() {
            _loadedTextures.Clear();
            _loadedBitmapFonts.Clear();
            _loadedSoundEffects.Clear();
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
