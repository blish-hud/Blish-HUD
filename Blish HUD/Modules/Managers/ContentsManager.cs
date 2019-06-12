using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD.Modules.Managers {
    public class ContentsManager {

        private readonly IDataReader _reader;

        private readonly Dictionary<string, Texture2D> _loadedTextures;
        private readonly Dictionary<string, SoundEffect> _loadedSoundEffects;
        private readonly Dictionary<string, Effect> _loadedEffects;
        private readonly Dictionary<string, BitmapFont> _loadedBitmapFonts;
        private readonly Dictionary<string, Model> _loadedModels;

        public ContentsManager(IDataReader reader) {
            _reader = reader;

            _loadedTextures = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
            _loadedSoundEffects = new Dictionary<string, SoundEffect>(StringComparer.OrdinalIgnoreCase);
            _loadedEffects = new Dictionary<string, Effect>(StringComparer.OrdinalIgnoreCase);
            _loadedBitmapFonts = new Dictionary<string, BitmapFont>(StringComparer.OrdinalIgnoreCase);
            _loadedModels = new Dictionary<string, Model>(StringComparer.OrdinalIgnoreCase);
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
                return new Effect(GameService.Graphics.GraphicsDevice, effectData, 0, (int)effectDataLength);
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

}
