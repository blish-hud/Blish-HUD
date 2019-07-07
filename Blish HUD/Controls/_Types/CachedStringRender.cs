using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD.Controls {
    public class CachedStringRender : IDisposable {

        private static readonly ConcurrentDictionary<int, CachedStringRender> _cachedStringRenders = new ConcurrentDictionary<int, CachedStringRender>();
        private static readonly NullControl _proxyControl = new NullControl();

        private RenderTarget2D _cachedRender;
        public RenderTarget2D CachedRender => _cachedRender;

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
        }

        private void InitRender(GraphicsDevice graphicsDevice) {
            if (_cachedRender != null)
                throw new ActionNotSupportedException($"{nameof(InitRender)} was already called on this!  It can only be called once.");

            _cachedRender = new RenderTarget2D(BlishHud.ActiveGraphicsDeviceManager.GraphicsDevice,
                                               this.DestinationRectangle.Width,
                                               this.DestinationRectangle.Height,
                                               false,
                                               SurfaceFormat.Color,
                                               DepthFormat.None,
                                               BlishHud.ActiveGraphicsDeviceManager.GraphicsDevice.PresentationParameters.MultiSampleCount,
                                               RenderTargetUsage.PreserveContents);

            _proxyControl.Size = this.DestinationRectangle.Size;

            graphicsDevice.SetRenderTarget(_cachedRender);

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
