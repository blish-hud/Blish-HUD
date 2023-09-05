using Blish_HUD.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using SpriteFontPlus;
using System;
using System.IO;

namespace Blish_HUD.Modules.Managers {
    public class ContentsManager : IDisposable {

        protected static readonly Logger Logger = Logger.GetLogger<ContentsManager>();

        private const string REF_NAME = "ref";

        private readonly IDataReader _reader;

        private ContentsManager(IDataReader reader) {
            _reader = reader;

            Logger.Debug("New {contentsManagerName} instance utilizing a {dataReaderType} data reader.", nameof(ContentsManager), _reader.GetType().FullName);
        }

        internal static ContentsManager GetModuleInstance(ModuleManager module) {
            return new ContentsManager(module.DataReader.GetSubPath(REF_NAME));
        }

        /// <summary>
        /// Loads a <see cref="Texture2D"/> from a file such as a PNG.
        /// </summary>
        /// <param name="texturePath">The path to the texture.</param>
        public Texture2D GetTexture(string texturePath) {
            return GetTexture(texturePath, ContentService.Textures.Error);
        }

        /// <summary>
        /// Loads a <see cref="Texture2D"/> from a file such as a PNG. If the requested texture is inaccessible, the <see cref="fallbackTexture"/> will be returned.
        /// </summary>
        /// <param name="texturePath">The path to the texture.</param>
        /// <param name="fallbackTexture">An alternative <see cref="Texture2D"/> to return if the requested texture is not found or is invalid.</param>
        public Texture2D GetTexture(string texturePath, Texture2D fallbackTexture) {
            using (var textureStream = _reader.GetFileStream(texturePath)) {
                if (textureStream != null) {
                    Logger.Debug("Successfully loaded texture {dataReaderFilePath}.", _reader.GetPathRepresentation(texturePath));
                    return TextureUtil.FromStreamPremultiplied(textureStream);
                }
            }

            Logger.Warn("Unable to load texture {dataReaderFilePath}.", _reader.GetPathRepresentation(texturePath));
            return fallbackTexture;
        }

        /// <summary>
        /// Loads a compiled shader in from a file as a <see cref="TEffect"/> that inherits from <see cref="Effect"/>.
        /// </summary>
        /// <typeparam name="TEffect">A custom effect wrapper (similar to the function of <see cref="BasicEffect"/>).</typeparam>
        /// <param name="effectPath">The path to the compiled shader.</param>
        public Effect GetEffect<TEffect>(string effectPath) where TEffect : Effect {
            if (GetEffect(effectPath) is TEffect effect) {
                return effect;
            }

            return null;
        }

        /// <summary>
        /// Loads a compiled shader in from a file as an <see cref="Effect"/>.
        /// </summary>
        /// <param name="effectPath">The path to the compiled shader.</param>
        public Effect GetEffect(string effectPath) {
            long effectDataLength = _reader.GetFileBytes(effectPath, out byte[] effectData);

            if (effectDataLength > 0) {
                using var ctx    = GameService.Graphics.LendGraphicsDeviceContext();
                var       effect = new Effect(ctx.GraphicsDevice, effectData, 0, (int)effectDataLength);

                return effect;
            }

            return null;
        }

        /// <summary>
        /// Loads a <see cref="SoundEffect"/> from a file.
        /// </summary>
        /// <param name="soundPath">The path to the sound file.</param>
        public SoundEffect GetSound(string soundPath) {
            using (var soundStream = _reader.GetFileStream(soundPath)) {
                if (soundStream != null)
                    return SoundEffect.FromStream(soundStream);
            }

            return null;
        }

        /// <summary>
        /// Loads a <see cref="SpriteFont"/> from a TrueTypeFont (*.ttf) file.
        /// </summary>
        /// <param name="fontPath">The path to the TTF font file.</param>
        /// <param name="fontSize">Size of the font.</param>
        public SpriteFont GetSpriteFont(string fontPath, int fontSize) {
            if (fontSize <= 0) {
                throw new ArgumentException("Font size must be greater than 0.", nameof(fontSize));
            }

            long fontDataLength = _reader.GetFileBytes(fontPath, out byte[] fontData);

            if (fontDataLength > 0) {
                using var ctx = GameService.Graphics.LendGraphicsDeviceContext();
                var bakeResult = TtfFontBaker.Bake(fontData, fontSize, 1024, 1024, new[] {
                    CharacterRange.BasicLatin,
                    CharacterRange.Latin1Supplement,
                    CharacterRange.LatinExtendedA
                });
                return bakeResult.CreateSpriteFont(ctx.GraphicsDevice);
            }

            return null;
        }

        /// <summary>
        /// Loads a <see cref="BitmapFont"/> from a TrueTypeFont (*.ttf) file.
        /// </summary>
        /// <param name="fontPath">The path to the TTF font file.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="lineHeight">Sets the line height. By default, <see cref="SpriteFont.LineSpacing"/> will be used.</param>
        public BitmapFont GetBitmapFont(string fontPath, int fontSize, int lineHeight = 0) {
            if (lineHeight < 0) {
                throw new ArgumentException("Line height cannot be negative.", nameof(lineHeight));
            }
            return GetSpriteFont(fontPath, fontSize)?.ToBitmapFont(lineHeight);
        }

        /// <summary>
        /// [NOT IMPLEMENTED] Loads a <see cref="Model"/> from a file.
        /// </summary>
        /// <param name="modelPath">The path to the model.</param>
        public Model GetModel(string modelPath) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves the stream of a file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        public Stream GetFileStream(string filePath) {
            return _reader.GetFileStream(filePath);
        }

        public void Dispose() {
            _reader?.Dispose();
        }

    }

}
