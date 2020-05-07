using Blish_HUD.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Net;

namespace Blish_HUD.Pathing.Content {
    public class PathableResourceManager : IDisposable {

        private static readonly Logger Logger = Logger.GetLogger<PathableResourceManager>();

        private readonly Dictionary<string, Texture2D> _textureCache;
        private readonly HashSet<string> _pendingTextureUse;
        private readonly HashSet<string> _pendingTextureRemoval;

        public IDataReader DataReader { get; }

        public PathableResourceManager(IDataReader dataReader) {
            this.DataReader = dataReader;

            _textureCache          = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
            _pendingTextureUse     = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _pendingTextureRemoval = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public void RunTextureDisposal() {
            if (_pendingTextureUse.Count == 0 && _pendingTextureRemoval.Count == 0) return; // Early exit if no changes (mostly to skip logging)

            Logger.Debug("Running texture swap for pathables. {addCount} will be added and {removeCount} will be removed.", _pendingTextureUse.Count, _pendingTextureRemoval.Count);

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

            _pendingTextureRemoval.Add(texturePath);
        }

        public Texture2D LoadTexture(string texturePath) {
            return LoadTexture(texturePath, ContentService.Textures.Error);
        }

        public Texture2D LoadTexture(string texturePath, Texture2D fallbackTexture) {
            _pendingTextureUse.Add(texturePath);

            if (!_textureCache.ContainsKey(texturePath)) {
                using (var textureStream = this.DataReader.GetFileStream(texturePath)) {
                    if (textureStream == null) {
                        Logger.Warn("Failed to load texture {dataReaderPath}.", this.DataReader.GetPathRepresentation(texturePath));

                        return fallbackTexture;
                    };

                    _textureCache.Add(texturePath, TextureUtil.FromStreamPremultiplied(GameService.Graphics.GraphicsDevice, textureStream));

                    Logger.Debug("Successfully loaded texture {dataReaderPath}.", this.DataReader.GetPathRepresentation(texturePath));
                }
            }

            return _textureCache[texturePath];
        }

        /// <inheritdoc />
        public void Dispose() {
            this.DataReader?.Dispose();

            foreach (KeyValuePair<string, Texture2D> texture in _textureCache) {
                texture.Value?.Dispose();
            }

            _textureCache.Clear();
            _pendingTextureUse.Clear();
            _pendingTextureRemoval.Clear();
        }

    }

}
