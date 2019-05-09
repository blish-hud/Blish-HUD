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
    public class LabelBase:Control {

        protected string _text;
        /// <summary>
        /// The text this control should show.
        /// </summary>
        public string Text {
            get => _text;
            set => SetProperty(ref _text, value, true);
        }

        protected BitmapFont _font;
        /// <summary>
        /// The font the <see cref="Text"/> will be rendered in.
        /// </summary>
        public BitmapFont Font {
            get => _font;
            set => SetProperty(ref _font, value, true);
        }

        protected Color _textColor = Color.White;
        /// <summary>
        /// The color of the <see cref="Text"/>.
        /// </summary>
        public Color TextColor {
            get => _textColor;
            set => SetProperty(ref _textColor, value);
        }

        protected Utils.DrawUtil.HorizontalAlignment _horizontalAlignment = Utils.DrawUtil.HorizontalAlignment.Left;
        public Utils.DrawUtil.HorizontalAlignment HorizontalAlignment {
            get => _horizontalAlignment;
            set => SetProperty(ref _horizontalAlignment, value);
        }

        protected Utils.DrawUtil.VerticalAlignment _verticalAlignment = Utils.DrawUtil.VerticalAlignment.Top;
        public Utils.DrawUtil.VerticalAlignment VerticalAlignment {
            get => _verticalAlignment;
            set => SetProperty(ref _verticalAlignment, value);
        }

        protected bool _showShadow = false;
        /// <summary>
        /// If enabled, a 1px offset shadow will be applied behind the rendered text.
        /// </summary>
        public bool ShowShadow {
            get => _showShadow;
            set => SetProperty(ref _showShadow, value, true);
        }

        protected bool _strokeText = false;
        /// <summary>
        /// If enabled, a stroke effect will be applied to the text to make it more visible.
        /// <see cref="ShadowColor"/> will set the color of the stroke.
        /// </summary>
        public bool StrokeText {
            get => _strokeText;
            set => SetProperty(ref _strokeText, value, true);
        }

        protected Color _shadowColor = Color.Black;
        /// <summary>
        /// If either <see cref="ShowShadow"/> or <see cref="StrokeText"/> is enabled, they will
        /// be drawn in this color.
        /// </summary>
        public Color ShadowColor {
            get => _shadowColor;
            set => SetProperty(ref _shadowColor, value);
        }

        private bool _autoSizeWidth = false;
        /// <summary>
        /// If enabled, the <see cref="Control.Width"/> of this control will change to match the width of the text.
        /// </summary>
        public bool AutoSizeWidth {
            get => _autoSizeWidth;
            set => SetProperty(ref _autoSizeWidth, value);
        }

        private bool _autoSizeHeight = false;
        /// <summary>
        /// If enabled, the <see cref="Control.Height"/> of this control will change to match the height of the text.
        /// </summary>
        public bool AutoSizeHeight {
            get => _autoSizeHeight;
            set => SetProperty(ref _autoSizeHeight, value);
        }

        public LabelBase() : base() {
            this.Font = Content.DefaultFont14;
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.None;
        }

        /// <summary>
        /// If either <see cref="AutoSizeWidth"/> or <see cref="AutoSizeHeight"/> is enabled,
        /// this will indicate the size of the label region after <see cref="RecalculateLayout"/>
        /// has completed.
        /// </summary>
        protected Point LabelRegion = Point.Zero;

        public override void RecalculateLayout() {
            int lblRegionWidth  = _size.X;
            int lblRegionHeight = _size.Y;

            if (_autoSizeWidth || _autoSizeHeight) {
                var textSize = _font.MeasureString(_text);

                if (this.AutoSizeWidth) {
                    lblRegionWidth = (int)Math.Ceiling(textSize.Width + (_showShadow || _strokeText ? 1 : 0));
                }

                if (this.AutoSizeHeight) {
                    lblRegionHeight = (int)Math.Ceiling(textSize.Height + (_showShadow || _strokeText ? 1 : 0));
                }
            }

            LabelRegion = new Point(lblRegionWidth, lblRegionHeight);

            this.Size = LabelRegion;
        }
        
        protected void DrawText(SpriteBatch spriteBatch, Rectangle bounds, string text = null) {
            text = text ?? _text;

            if (_font == null || string.IsNullOrWhiteSpace(text)) { return; }

            if (_showShadow && !_strokeText)
                spriteBatch.DrawStringOnCtrl(this, text, _font, bounds.OffsetBy(1, 1), _shadowColor, false, _horizontalAlignment, _verticalAlignment);
            
            spriteBatch.DrawStringOnCtrl(this, text, _font, bounds, _textColor, false, _strokeText, 1, _horizontalAlignment, _verticalAlignment);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            DrawText(spriteBatch, bounds, _text);
        }

    }
}
