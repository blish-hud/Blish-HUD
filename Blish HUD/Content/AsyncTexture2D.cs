﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Content {
    public sealed class AsyncTexture2D : IDisposable {

        private Texture2D _stagedTexture2D;
        private Texture2D _activeTexture2D;

        public AsyncTexture2D() {
            _activeTexture2D = ContentService.Textures.TransparentPixel;
        }

        public AsyncTexture2D(Texture2D defaultTexture) {
            _activeTexture2D = defaultTexture;
        }

        public Texture2D GetTexture() {
            return _activeTexture2D;
        }

        public void SwapTexture(Texture2D newTexture) {
            _stagedTexture2D = newTexture;

            GameService.Overlay.QueueMainThreadUpdate(this.ApplyTextureSwap);
        }

        private void ApplyTextureSwap(GameTime gameTime) {
            _activeTexture2D = _stagedTexture2D;
            _stagedTexture2D = null;
        }

        ///// <inheritdoc />
        //public override bool Equals(object obj) {
        //    return Equals(_activeTexture2D, obj);
        //}

        /// <inheritdoc />
        public override int GetHashCode() {
            return _activeTexture2D?.GetHashCode() ?? 0;
        }

        public static implicit operator Texture2D(AsyncTexture2D asyncTexture2D) {
            return asyncTexture2D._activeTexture2D;
        }

        public static implicit operator AsyncTexture2D(Texture2D texture2D) {
            if (texture2D == null) return null;

            return new AsyncTexture2D(texture2D);
        }

        /// <inheritdoc />
        public void Dispose() {
            _stagedTexture2D?.Dispose();
            _activeTexture2D?.Dispose();
        }

    }
}
