using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD.Controls {
    public class Label:Control {

        private string _text = "";
        public string Text {
            get => _text;
            set {
                if (_text == value) return;

                _text = value;

                OnPropertyChanged(nameof(this.Text));
            }
        }

        private BitmapFont _font;
        public BitmapFont Font {
            get => _font;
            set {
                if (_font == value) return;

                _font = value;

                OnPropertyChanged(nameof(this.Font));
            }
        }

        private Color _textColor = Color.White;
        public Color TextColor {
            get => _textColor;
            set {
                if (_textColor == value) return;

                _textColor = value;

                OnPropertyChanged(nameof(this.TextColor));
            }
        }

        private Utils.DrawUtil.HorizontalAlignment _horizontalAlignment = Utils.DrawUtil.HorizontalAlignment.Left;
        public Utils.DrawUtil.HorizontalAlignment HorizontalAlignment {
            get => _horizontalAlignment;
            set {
                if (_horizontalAlignment == value) return;

                _horizontalAlignment = value; 

                OnPropertyChanged(nameof(this.HorizontalAlignment));
            }
        }

        private Utils.DrawUtil.VerticalAlignment _verticalAlignment = Utils.DrawUtil.VerticalAlignment.Top;
        public Utils.DrawUtil.VerticalAlignment VerticalAlignment {
            get => _verticalAlignment;
            set {
                if (_verticalAlignment == value) return;

                _verticalAlignment = value;

                OnPropertyChanged(nameof(this.VerticalAlignment));
            }
        }

        private bool _showShadow = false;
        public bool ShowShadow {
            get => _showShadow;
            set {
                if (_showShadow == value) return;

                _showShadow = value;

                OnPropertyChanged(nameof(this.ShowShadow));
            }
        }

        private bool _strokeShadow = false;
        public bool StrokeShadow {
            get => _strokeShadow;
            set {
                _strokeShadow = value;

                OnPropertyChanged(nameof(this.StrokeShadow));
            }
        }

        private Color _shadowColor = Color.Black;
        public Color ShadowColor {
            get => _shadowColor;
            set {
                if (_shadowColor == value) return;

                _shadowColor = value;
                
                OnPropertyChanged(nameof(this.ShadowColor));
            }
        }

        private bool _autoSizeWidth = false;
        public bool AutoSizeWidth {
            get => _autoSizeWidth;
            set {
                if (_autoSizeWidth == value) return;

                _autoSizeWidth = value;

                OnPropertyChanged(nameof(this.AutoSizeWidth));
            }
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.None;
        }

        private bool _autoSizeHeight = false;
        public bool AutoSizeHeight {
            get => _autoSizeHeight;
            set {
                if (_autoSizeHeight == value) return;

                _autoSizeHeight = value; 

                OnPropertyChanged(nameof(this.AutoSizeHeight));
            }
        }

        protected int LeftOffset = 0;

        // TODO: Control can fail if no size is specified, it's set to auto-size, and no text is provided

        public Label() : base() {
            this.Font = Content.DefaultFont14;
        }

        public override void Invalidate() {
            if (this.AutoSizeWidth) this.Width = (int)Math.Ceiling(this.Font.MeasureString(this.Text).Width) + LeftOffset;

            if (this.AutoSizeHeight) this.Height = (int)Math.Ceiling(this.Font.MeasureString(this.Text).Height) + LeftOffset;

            base.Invalidate();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            if (this.Font == null) { return; }

            if (this.ShowShadow)
                Utils.DrawUtil.DrawAlignedText(spriteBatch, this.Font, this.Text, bounds.OffsetBy(1, 1).OffsetBy(LeftOffset, 0), this.ShadowColor, this.HorizontalAlignment, this.VerticalAlignment);

            if (this.ShowShadow && this.StrokeShadow) {
                Utils.DrawUtil.DrawAlignedText(spriteBatch, this.Font, this.Text, bounds.OffsetBy(-1, -1).OffsetBy(LeftOffset, 0), this.ShadowColor, this.HorizontalAlignment, this.VerticalAlignment);
                Utils.DrawUtil.DrawAlignedText(spriteBatch, this.Font, this.Text, bounds.OffsetBy(-1, 1).OffsetBy(LeftOffset, 0), this.ShadowColor, this.HorizontalAlignment, this.VerticalAlignment);
                Utils.DrawUtil.DrawAlignedText(spriteBatch, this.Font, this.Text, bounds.OffsetBy(1, -1).OffsetBy(LeftOffset, 0), this.ShadowColor, this.HorizontalAlignment, this.VerticalAlignment);
            }
            
            Utils.DrawUtil.DrawAlignedText(spriteBatch, this.Font, this.Text, bounds.OffsetBy(LeftOffset, 0), this.TextColor, this.HorizontalAlignment, this.VerticalAlignment);
        }

    }
}
