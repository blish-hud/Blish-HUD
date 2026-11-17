using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Content {
    /// <summary>
    /// A thread-safe wrapper on Texture2D that allows you to swap the <see cref="Texture2D"/> backing for a texture already assigned to a property.
    /// </summary>
    public sealed class AsyncTexture2D : IDisposable {

        private Texture2D _stagedTexture2D;
        private Texture2D _activeTexture2D;

        /// <summary>
        /// <c>true</c>, if the <see cref="AsyncTexture2D"/>'s <see cref="Texture"/> is set.
        /// </summary>
        public bool HasTexture => _activeTexture2D != null;

        /// <summary>
        /// <c>true</c>, if the <see cref="AsyncTexture2D"/> has had a texture assigned to it (other than a provided default texture).
        /// </summary>
        public bool HasSwapped { get; private set; }

        /// <summary>
        /// The active <see cref="Texture2D"/> of the <see cref="AsyncTexture2D"/>.
        /// </summary>
        public Texture2D Texture {
            get {
                if (!this.HasTexture) {
                    throw new InvalidOperationException($"{nameof(AsyncTexture2D)} object must have a Texture.");
                }

                return _activeTexture2D;
            }
        }

        /// <inheritdoc cref="Texture2D.Width"/>
        public int Width => this.HasTexture 
                                ? _activeTexture2D.Width 
                                : throw new InvalidOperationException($"{nameof(AsyncTexture2D)} object must have a Texture.");

        /// <inheritdoc cref="Texture2D.Height"/>
        public int Height => this.HasTexture
                                 ? _activeTexture2D.Height
                                 : throw new InvalidOperationException($"{nameof(AsyncTexture2D)} object must have a Texture.");

        /// <inheritdoc cref="Texture2D.Bounds"/>
        public Rectangle Bounds => this.HasTexture
                                       ? _activeTexture2D.Bounds
                                       : throw new InvalidOperationException($"{nameof(AsyncTexture2D)} object must have a Texture.");

        /// <inheritdoc cref="Texture2D.IsDisposed"/>
        public bool IsDisposed => this.HasTexture
                                       ? _activeTexture2D.IsDisposed
                                       : throw new InvalidOperationException($"{nameof(AsyncTexture2D)} object must have a Texture.");

        /// <summary>
        /// Occurs when the <see cref="Texture"/> of the <see cref="AsyncTexture2D"/> is replaced.
        /// </summary>
        public event EventHandler<ValueChangedEventArgs<Texture2D>> TextureSwapped;

        /// <summary>
        /// Create an <see cref="AsyncTexture2D"/> where the current <see cref="Texture"/> is a single transparent pixel.
        /// </summary>
        public AsyncTexture2D() {
            _activeTexture2D = ContentService.Textures.TransparentPixel;
        }

        /// <summary>
        /// Create an <see cref="AsyncTexture2D"/> where the current <see cref="Texture"/> is the <see cref="Texture2D"/> passed as <param name="defaultTexture"/>.
        /// </summary>
        public AsyncTexture2D(Texture2D defaultTexture) {
            _activeTexture2D = defaultTexture;
        }

        /// <summary>
        /// Replaces the <see cref="Texture"/> of the <see cref="AsyncTexture2D"/> with the texture provided in <param name="newTexture"/> on the main thread.
        /// </summary>
        /// <param name="newTexture">The new texture to assign.</param>
        public void SwapTexture(Texture2D newTexture) {
            _stagedTexture2D = newTexture;

            if (Program.IsMainThread) {
                ApplyTextureSwap(null);
            } else {
                GameService.Overlay.QueueMainThreadUpdate(this.ApplyTextureSwap);
            }
        }

        private void ApplyTextureSwap(GameTime gameTime) {
            var previousTexture2D = _activeTexture2D;
            _activeTexture2D = _stagedTexture2D;
            this.HasSwapped  = true;
            _stagedTexture2D = null;
            this.TextureSwapped?.Invoke(this, new ValueChangedEventArgs<Texture2D>(previousTexture2D, _activeTexture2D));
        }

        /// <inheritdoc cref="DatAssetCache.GetTextureFromAssetId" />
        public static AsyncTexture2D FromAssetId(int assetId) {
            return GameService.Content.DatAssetCache.GetTextureFromAssetId(assetId);
        }

        /// <inheritdoc cref="DatAssetCache.TryGetTextureFromAssetId" />
        public static bool TryFromAssetId(int assetId, out AsyncTexture2D texture) {
            return GameService.Content.DatAssetCache.TryGetTextureFromAssetId(assetId, out texture);
        }

        public override bool Equals(object obj) {
            if (!HasTexture) return obj == null;
            if (obj == null) return false;

            if (obj is Texture2D tobj) {
                return _activeTexture2D.Equals(tobj);
            }

            return this == obj;
        }

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

        public void Dispose() {
            _stagedTexture2D?.Dispose();
            _activeTexture2D?.Dispose();
        }

    }
}
