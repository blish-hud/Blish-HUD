using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;

namespace Blish_HUD.Modules.MouseUsability {
    public class MouseHighlight:Control {

        public enum Orientation {
            Horizontal,
            Vertical,
        }

        private Color _highlightColor = Color.Red;
        public Color HighlightColor {
            get {
                return _highlightColor;
            }
            set {
                if (_highlightColor != value) {
                    _highlightColor = value;
                    Invalidate();
                }
            }
        }

        private float _highlightThickness = 2;
        public float HighlightThickness {
            get {
                return _highlightThickness;
            }
            set {
                if (_highlightThickness != value) {
                    _highlightThickness = value;
                    Invalidate();
                }
            }
        }

        private Color _outlineColor = Color.Black;
        public Color OutlineColor {
            get => _outlineColor;
            set {
                if (_outlineColor == value) return;

                _outlineColor = value;
                OnPropertyChanged();
            }
        }

        private float _outlineThickness = 1;
        public float OutlineThickness {
            get {
                return _outlineThickness;
            }
            set {
                if (_outlineThickness != value) {
                    _outlineThickness = value;
                    Invalidate();
                }
            }
        }

        private Orientation _orientation;


        private Texture2D HighlightPixel;

        public MouseHighlight(Orientation orientation) {
            _orientation = orientation;

            //HighlightPixel = new Texture2D(GameServices.GetService<GraphicsService>().GraphicsDevice, 1, 1);
            HighlightPixel = HighlightPixel ?? ContentService.Textures.Pixel;
            //GeneratePixel();

            Invalidate();
        }

        public override void Invalidate() {
            base.Invalidate();
            
            //if (_orientation == Orientation.Horizontal) {
            //    _size = new Point(
            //        GameServices.GetService<GraphicsService>().Resolution.X, 
            //        (int)Math.Round(this.HighlightThickness, 0) + (int)Math.Round(this.OutlineThickness, 0) * 2
            //    );
            //} else {
            //    _size = new Point(
            //        (int)Math.Round(this.HighlightThickness, 0) + (int)Math.Round(this.OutlineThickness, 0) * 2,
            //        GameServices.GetService<GraphicsService>().Resolution.Y
            //    );
            //}
        }

        private void GeneratePixel() {
            HighlightPixel.SetData(new Color[] {Color.White});
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.None;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.Draw(HighlightPixel, bounds, this.OutlineColor);

            //int centerMargin = _orientation == Orientation.Horizontal ? bounds.Width / 2 - bounds.Height / 2 : bounds.Height / 2 - bounds.Width / 2;

            if (_orientation == Orientation.Horizontal) {
                spriteBatch.Draw(HighlightPixel, new Rectangle(0, (int)(this.OutlineThickness), bounds.Width, (int)(this.HighlightThickness)).OffsetBy(bounds.Location), this.HighlightColor);
            } else {
                spriteBatch.Draw(HighlightPixel, new Rectangle((int)(this.OutlineThickness), 0, (int)(this.HighlightThickness), bounds.Height).OffsetBy(bounds.Location), this.HighlightColor);
            }
        }

    }
}
