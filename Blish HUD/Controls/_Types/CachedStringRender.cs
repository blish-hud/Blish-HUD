using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using Blish_HUD;
using Blish_HUD.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD.Controls {
    public class CachedStringRender : IDisposable {

        private static readonly ConcurrentDictionary<int, CachedStringRender> _cachedStringRenders = new ConcurrentDictionary<int, CachedStringRender>();
        private static readonly NullControl _proxyControl = new NullControl();

        private readonly AsyncTexture2D _cachedRender;

        public AsyncTexture2D CachedRender => _cachedRender;

        public string Text { get; }

        public BitmapFont Font { get; }

        public Rectangle DestinationRectangle { get; }

        public Color Color { get; }

        public bool Wrap { get; }

        public bool Stroke { get; }

        public int StrokeDistance { get; }

        public HorizontalAlignment HorizontalAlignment { get; }

        public VerticalAlignment VerticalAlignment { get; }

        public CachedStringRender(string              text,
                                  BitmapFont          font,
                                  Rectangle           destinationRectangle,
                                  Color               color,
                                  bool                wrap,
                                  bool                stroke,
                                  int                 strokeDistance      = 1,
                                  HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left,
                                  VerticalAlignment   verticalAlignment   = VerticalAlignment.Middle) {

            this.Text                 = text;
            this.Font                 = font;
            this.DestinationRectangle = new Rectangle(Point.Zero, destinationRectangle.Size);
            this.Color                = color;
            this.Wrap                 = wrap;
            this.Stroke               = stroke;
            this.StrokeDistance       = strokeDistance;
            this.HorizontalAlignment  = horizontalAlignment;
            this.VerticalAlignment    = verticalAlignment;

            _cachedRender = new AsyncTexture2D(ContentService.Textures.TransparentPixel.Duplicate());
        }

        private void InitRender(GraphicsDevice graphicsDevice) {
            var cachedRenderTarget = new RenderTarget2D(BlishHud.Instance.ActiveGraphicsDeviceManager.GraphicsDevice,
                                                        this.DestinationRectangle.Width,
                                                        this.DestinationRectangle.Height,
                                                        false,
                                                        SurfaceFormat.Color,
                                                        DepthFormat.None,
                                                        BlishHud.Instance.ActiveGraphicsDeviceManager.GraphicsDevice.PresentationParameters.MultiSampleCount,
                                                        RenderTargetUsage.PreserveContents);

            _proxyControl.Size = this.DestinationRectangle.Size;

            graphicsDevice.SetRenderTarget(cachedRenderTarget);

            using (var spriteBatch = new SpriteBatch(graphicsDevice)) {
                spriteBatch.Begin();

                spriteBatch.DrawStringOnCtrl(_proxyControl,
                                             this.Text,
                                             this.Font,
                                             this.DestinationRectangle,
                                             this.Color,
                                             this.Wrap,
                                             this.Stroke,
                                             this.StrokeDistance,
                                             this.HorizontalAlignment,
                                             this.VerticalAlignment);

                spriteBatch.End();
            }

            graphicsDevice.SetRenderTarget(null);

            _cachedRender.SwapTexture(cachedRenderTarget);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = (this.Text != null ? this.Text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.Font != null ? this.Font.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.DestinationRectangle.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Color.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Wrap.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Stroke.GetHashCode();
                hashCode = (hashCode * 397) ^ this.StrokeDistance;
                hashCode = (hashCode * 397) ^ (int) this.HorizontalAlignment;
                hashCode = (hashCode * 397) ^ (int) this.VerticalAlignment;
                return hashCode;
            }
        }

        protected bool Equals(CachedStringRender other) {
            return string.Equals(this.Text, other.Text) && Equals(this.Font, other.Font) && this.DestinationRectangle.Equals(other.DestinationRectangle) && this.Color.Equals(other.Color) && this.Wrap == other.Wrap && this.Stroke == other.Stroke && this.StrokeDistance == other.StrokeDistance && this.HorizontalAlignment == other.HorizontalAlignment && this.VerticalAlignment == other.VerticalAlignment;
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;

            return Equals((CachedStringRender) obj);
        }

        public static CachedStringRender GetCachedStringRender(string              text,
                                                               BitmapFont          font,
                                                               Rectangle           destinationRectangle,
                                                               Color               color,
                                                               bool                wrap,
                                                               bool                stroke,
                                                               int                 strokeDistance      = 1,
                                                               HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left,
                                                               VerticalAlignment   verticalAlignment   = VerticalAlignment.Middle) {

            var checkCsr = new CachedStringRender(text, font, destinationRectangle, color, wrap, stroke, strokeDistance, horizontalAlignment, verticalAlignment);

            int csrHash = checkCsr.GetHashCode();

            bool containsCachedCsr = _cachedStringRenders.ContainsKey(csrHash);

            if (containsCachedCsr && _cachedStringRenders.TryGetValue(csrHash, out var existingCsr)) {
                checkCsr.Dispose();
                return existingCsr;
            }

            if (!containsCachedCsr) {
                _cachedStringRenders.TryAdd(csrHash, checkCsr);
            }

            GameService.Graphics.QueueMainThreadRender(checkCsr.InitRender);

            return checkCsr;
        }

        public void Dispose() {
            _cachedRender?.Dispose();
        }

    }
}
