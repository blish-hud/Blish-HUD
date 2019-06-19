using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD.Controls {
    public class Label : LabelBase {

        /// <summary>
        /// The text this <see cref="Label"/> should show.
        /// </summary>
        public string Text {
            get => _text;
            set {
                if (SetProperty(ref _text, value, true) && (_autoSizeWidth || _autoSizeHeight)) {
                    RecalculateLayout();
                }
            }
        }

        /// <summary>
        /// The font the <see cref="Text"/> will be rendered in.
        /// </summary>
        public BitmapFont Font {
            get => _font;
            set {
                if (SetProperty(ref _font, value, true) && (_autoSizeWidth || _autoSizeHeight)) {
                    RecalculateLayout();
                }
            }
        }

        /// <summary>
        /// The color of the <see cref="Text"/>.
        /// </summary>
        public Color TextColor {
            get => _textColor;
            set => SetProperty(ref _textColor, value);
        }

        public Utils.DrawUtil.HorizontalAlignment HorizontalAlignment {
            get => _horizontalAlignment;
            set => SetProperty(ref _horizontalAlignment, value);
        }

        public Utils.DrawUtil.VerticalAlignment VerticalAlignment {
            get => _verticalAlignment;
            set => SetProperty(ref _verticalAlignment, value);
        }

        public bool WrapText {
            get => _wrapText;
            set => SetProperty(ref _wrapText, value, true);
        }

        /// <summary>
        /// If enabled, a 1px offset shadow will be applied behind the rendered text.
        /// </summary>
        public bool ShowShadow {
            get => _showShadow;
            set => SetProperty(ref _showShadow, value, true);
        }

        /// <summary>
        /// If enabled, a stroke effect will be applied to the text to make it more visible.
        /// <see cref="ShadowColor"/> will set the color of the stroke.
        /// </summary>
        public bool StrokeText {
            get => _strokeText;
            set => SetProperty(ref _strokeText, value, true);
        }

        /// <summary>
        /// If either <see cref="ShowShadow"/> or <see cref="StrokeText"/> is enabled, they will
        /// be drawn in this color.
        /// </summary>
        public Color ShadowColor {
            get => _shadowColor;
            set => SetProperty(ref _shadowColor, value);
        }

        /// <summary>
        /// If enabled, the <see cref="Control.Width"/> of this control will change to match the width of the text.
        /// </summary>
        public bool AutoSizeWidth {
            get => _autoSizeWidth;
            set {
                if (SetProperty(ref _autoSizeWidth, value, true) && (_autoSizeWidth || _autoSizeHeight)) {
                    RecalculateLayout();
                }
            }
        }

        /// <summary>
        /// If enabled, the <see cref="Control.Height"/> of this control will change to match the height of the text.
        /// </summary>
        public bool AutoSizeHeight {
            get => _autoSizeHeight;
            set {
                if (SetProperty(ref _autoSizeHeight, value, true) && (_autoSizeWidth || _autoSizeHeight)) {
                    RecalculateLayout();
                }
            }
        }

        public Label() : base() {
            _cacheLabel = false;
        }


        public override void RecalculateLayout() {
            base.RecalculateLayout();

            this.Size = LabelRegion;
        }

    }
}
