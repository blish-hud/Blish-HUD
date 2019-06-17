using Blish_HUD.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Pathing.Content {
    public class PathableResourceManager : IDisposable {

        private readonly Dictionary<string, Texture2D> _textureCache;
        private readonly HashSet<string> _pendingTextureUse;
        private readonly HashSet<string> _pendingTextureRemoval;

        private readonly IDataReader _dataReader;

        public IDataReader DataReader => _dataReader;

        public PathableResourceManager(IDataReader dataReader) {
            _dataReader = dataReader;

            _textureCache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
            _pendingTextureUse = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _pendingTextureRemoval = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public void RunTextureDisposal() {
            // Prevent us from removing textures that are still needed
            _pendingTextureRemoval.RemoveWhere(t => _pendingTextureUse.Contains(t));

            foreach (string textureKey in _pendingTextureRemoval) {
                if (_textureCache.TryGetValue(textureKey, out var texture)) {
                    texture?.Dispose();
                }

                _textureCache.Remove(textureKey);
            }

            _pendingTextureUse.Clear();
            _pendingTextureRemoval.Clear();
        }

        public void MarkTextureForDisposal(string texturePath) {
            if (texturePath == null) return;

            if (_textureCache.ContainsKey(texturePath)) {
                _pendingTextureRemoval.Add(texturePath);
            }
        }

        public Texture2D LoadTexture(string texturePath) {
            return LoadTexture(texturePath, ContentService.Textures.Error);
        }

        public Texture2D LoadTexture(string texturePath, Texture2D fallbackTexture) {
            _pendingTextureUse.Add(texturePath);

            if (!_textureCache.ContainsKey(texturePath)) {
                using (var textureStream = _dataReader.GetFileStream(texturePath)) {
                    if (textureStream == null) return fallbackTexture;

                    _textureCache.Add(texturePath,
                                      Texture2D.FromStream(GameService.Graphics.GraphicsDevice,
                                                           textureStream));
                }
            }

            return _textureCache[texturePath];
        }

        /// <inheritdoc />
        public void Dispose() {
            _dataReader?.Dispose();

            foreach (KeyValuePair<string, Texture2D> texture in _textureCache) {
                texture.Value?.Dispose();
            }

            _textureCache.Clear();
            _pendingTextureUse.Clear();
            _pendingTextureRemoval.Clear();
        }

    }

}
