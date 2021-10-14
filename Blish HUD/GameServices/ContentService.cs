﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Blish_HUD.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD {

    public class ContentService : GameService {

        private static readonly Logger Logger = Logger.GetLogger<ContentService>();

        private const string REF_FILE = "ref.dat";

        #region Load Static

        private static readonly ConcurrentDictionary<string, SoundEffect> _loadedSoundEffects;
        private static readonly ConcurrentDictionary<string, BitmapFont>  _loadedBitmapFonts;
        private static readonly ConcurrentDictionary<string, Texture2D>   _loadedTextures;
        private static readonly ConcurrentDictionary<string, Stream>      _loadedFiles;

        static ContentService() {
            _loadedSoundEffects = new ConcurrentDictionary<string, SoundEffect>();
            _loadedBitmapFonts  = new ConcurrentDictionary<string, BitmapFont>();
            _loadedTextures     = new ConcurrentDictionary<string, Texture2D>();
            _loadedFiles        = new ConcurrentDictionary<string, Stream>();
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

            public static Texture2D TransparentPixel { get; private set; }

            public static void Load() {
                Error = Content.GetTexture(@"common\error");

                Pixel = new Texture2D(Graphics.GraphicsDevice, 1, 1);
                Pixel.SetData(new[] { Color.White });

                TransparentPixel = new Texture2D(Graphics.GraphicsDevice, 1, 1);
                TransparentPixel.SetData(new[] { Color.Transparent });
            }
        }

        private IDataReader _audioDataReader;

        private BitmapFont  _defaultFont12;
        public  BitmapFont  DefaultFont12 => _defaultFont12 ??= GetFont(FontFace.Menomonia, FontSize.Size12, FontStyle.Regular);

        private BitmapFont _defaultFont14;
        public  BitmapFont DefaultFont14 => _defaultFont14 ??= GetFont(FontFace.Menomonia, FontSize.Size14, FontStyle.Regular);

        private BitmapFont _defaultFont16;
        public  BitmapFont DefaultFont16 => _defaultFont16 ??= GetFont(FontFace.Menomonia, FontSize.Size16, FontStyle.Regular);

        private BitmapFont _defaultFont18;
        public  BitmapFont DefaultFont18 => _defaultFont18 ??= GetFont(FontFace.Menomonia, FontSize.Size18, FontStyle.Regular);

        private BitmapFont _defaultFont32;
        public  BitmapFont DefaultFont32 => _defaultFont32 ??= GetFont(FontFace.Menomonia, FontSize.Size32, FontStyle.Regular);

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

        public Microsoft.Xna.Framework.Content.ContentManager ContentManager => BlishHud.Instance.ActiveContentManager;

        protected override void Initialize() { /* NOOP */ }

        protected override void Load() {
            Textures.Load();
            var zipArchiveReader = new ZipArchiveReader(RefPath);
            _audioDataReader = zipArchiveReader.GetSubPath("audio");
        }

        // Temporary failsafe until all audio bugs are resolved
        private int _playRemainingAttempts = 3;

        public void PlaySoundEffectByName(string soundName) {
            if (_playRemainingAttempts <= 0) {
                // We keep failing to play sound effects - don't even bother
                return;
            }

            if (GameService.GameIntegration.Audio.AudioDevice == null) {
                // No device is set yet or there isn't one to use
                return;
            }

            try {
                const string SOUND_EFFECT_FILE_EXTENSION = ".wav";
                var          filePath                    = soundName + SOUND_EFFECT_FILE_EXTENSION;
            
                if (_audioDataReader.FileExists(filePath)) {
                    SoundEffect.FromStream(_audioDataReader.GetFileStream(filePath)).Play(GameService.GameIntegration.Audio.Volume, 0, 0);
                }

                _playRemainingAttempts = 3;
            } catch (Exception ex) {
                _playRemainingAttempts--;
                Logger.Warn(ex, "Failed to play sound effect.");
            }
}

        private static string RefPath => ApplicationSettings.Instance.RefPath ?? REF_FILE;

        // Used while debugging since it's easier
        private static Texture2D TextureFromFile(string filepath) {
            if (File.Exists(filepath)) {
                using (var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    return TextureUtil.FromStreamPremultiplied(BlishHud.Instance.GraphicsDevice, fileStream);
                }
            } else return null;
        }

        private static Texture2D TextureFromFileSystem(string filepath) {
            var refPath = RefPath;
            if (!File.Exists(refPath)) {
                Logger.Warn("{refFileName} is missing!  Lots of assets will be missing!", refPath);
                return null;
            }

            using (var refFs = new FileStream(refPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using (var refArchive = new ZipArchive(refFs, ZipArchiveMode.Read)) {
                    var refEntry = refArchive.GetEntry(filepath);

                    if (refEntry != null) {
                        using (var textureStream = refEntry.Open()) {
                            var textureCanSeek = new MemoryStream();
                            textureStream.CopyTo(textureCanSeek);

                            return TextureUtil.FromStreamPremultiplied(BlishHud.Instance.GraphicsDevice, textureCanSeek);
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
            return GameService.Content.ContentManager.Load<MonoGame.Extended.TextureAtlases.TextureAtlas>(textureAtlasName);
        }

        public void PurgeTextureCache(string textureName) {
            _loadedTextures.TryRemove(textureName, out var _);
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
                    cachedTexture = GameService.Content.ContentManager.Load<Texture2D>(textureName);
                } catch (ContentLoadException) {
                    Logger.Warn("Could not find {textureName} precompiled or in the ref archive.", textureName);
                }
            }

            cachedTexture = cachedTexture ?? defaultTexture;

            _loadedTextures.TryAdd(textureName, cachedTexture);

            return cachedTexture;
        }

        #region Render Service

        private const string RENDERSERVICE_REQUESTURL = "https://render.guildwars2.com/file/";

        /// <summary>
        /// Retreives a texture from the Guild Wars 2 Render Service.
        /// </summary>
        /// <param name="signature">The SHA1 signature of the requested texture.</param>
        /// <param name="fileId">The file id of the requested texture.</param>
        /// <param name="size">Specifies the size of the texture requested - only some render service hosts will utilize this setting.</param>
        /// <returns>A transparent texture that is later overwritten by the texture downloaded from the Render Service.</returns>
        /// <seealso cref="https://wiki.guildwars2.com/wiki/API:Render_service"/>
        public AsyncTexture2D GetRenderServiceTexture(string signature, string fileId) {
            AsyncTexture2D returnedTexture = new AsyncTexture2D(Textures.TransparentPixel.Duplicate());

            string requestUrl = $"{RENDERSERVICE_REQUESTURL}{signature}/{fileId}.png";

            Gw2WebApi.AnonymousConnection.Client.Render.DownloadToByteArrayAsync(requestUrl)
                     .ContinueWith((textureDataResponse) => {
                                       if (textureDataResponse.Exception != null) {
                                           Logger.Warn(textureDataResponse.Exception, "Request to render service for {textureUrl} failed.", requestUrl);
                                           return;
                                       }

                                       try {
                                           var textureData = textureDataResponse.Result;

                                           using (var textureStream = new MemoryStream(textureData)) {
                                               var loadedTexture = TextureUtil.FromStreamPremultiplied(Graphics.GraphicsDevice, textureStream);

                                               returnedTexture.SwapTexture(loadedTexture);
                                           }
                                       } catch (Exception ex) {
                                           Logger.Warn(ex, $"Render service texture {requestUrl} failed to load.");

                                           returnedTexture.SwapTexture(Textures.Error);
                                       }
                                   });

            return returnedTexture;
        }

        private static readonly Regex _regexRenderServiceSignatureFileIdPair = new Regex(@"(.{40})\/(\d+)(?>\..*)?$", RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Retreives a texture from the Guild Wars 2 Render Service.
        /// </summary>
        /// <param name="uriOrSignatureFileIdPair">Either the full Render Service URL or the signature and file id URI (e.g. "7554DCAF5A1EA1BDF5297352A203AF2357BE2B5B/498983").</param>
        /// <returns>A transparent texture that is later overwritten by the texture downloaded from the Render Service.</returns>
        /// <seealso cref="https://wiki.guildwars2.com/wiki/API:Render_service"/>
        public AsyncTexture2D GetRenderServiceTexture(string uriOrSignatureFileIdPair) {
            var splitUri = _regexRenderServiceSignatureFileIdPair.Match(uriOrSignatureFileIdPair);

            if (!splitUri.Success) {
                throw new ArgumentException($"Could not find signature / file id pair in provided '{uriOrSignatureFileIdPair}'.", nameof(uriOrSignatureFileIdPair));
            }

            string signature = splitUri.Groups[1].Value;
            string fileId    = splitUri.Groups[2].Value;

            return GetRenderServiceTexture(signature, fileId);
        }

        #endregion

        public BitmapFont GetFont(FontFace font, FontSize size, FontStyle style) {
            string fullFontName = $"{font.ToString().ToLowerInvariant()}-{((int)size).ToString()}-{style.ToString().ToLowerInvariant()}";

            if (!_loadedBitmapFonts.ContainsKey(fullFontName)) {
                var loadedFont = this.ContentManager.Load<BitmapFont>($"fonts\\{font.ToString().ToLowerInvariant()}\\{fullFontName}");
                loadedFont.LetterSpacing = -1;
                _loadedBitmapFonts.TryAdd(fullFontName, loadedFont);

                return loadedFont;
            }

            return _loadedBitmapFonts[fullFontName];
        }

        protected override void Unload() {
            _loadedTextures.Clear();
            _loadedBitmapFonts.Clear();
            _loadedSoundEffects.Clear();
            _loadedFiles.Clear();
        }

        protected override void Update(GameTime gameTime) { /* NOOP */ }

    }
}
