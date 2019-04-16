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
            set => SetProperty(ref _text, value);
        }

        private BitmapFont _font;
        public BitmapFont Font {
            get => _font;
            set => SetProperty(ref _font, value);
        }

        private Color _textColor = Color.White;
        public Color TextColor {
            get => _textColor;
            set => SetProperty(ref _textColor, value);
        }

        private Utils.DrawUtil.HorizontalAlignment _horizontalAlignment = Utils.DrawUtil.HorizontalAlignment.Left;
        public Utils.DrawUtil.HorizontalAlignment HorizontalAlignment {
            get => _horizontalAlignment;
            set => SetProperty(ref _horizontalAlignment, value);
        }

        private Utils.DrawUtil.VerticalAlignment _verticalAlignment = Utils.DrawUtil.VerticalAlignment.Top;
        public Utils.DrawUtil.VerticalAlignment VerticalAlignment {
            get => _verticalAlignment;
            set => SetProperty(ref _verticalAlignment, value);
        }

        private bool _showShadow = false;
        public bool ShowShadow {
            get => _showShadow;
            set => SetProperty(ref _showShadow, value);
        }

        private bool _strokeShadow = false;
        public bool StrokeShadow {
            get => _strokeShadow;
            set => SetProperty(ref _strokeShadow, value);
        }

        private Color _shadowColor = Color.Black;
        public Color ShadowColor {
            get => _shadowColor;
            set => SetProperty(ref _shadowColor, value);
        }

        private bool _autoSizeWidth = false;
        public bool AutoSizeWidth {
            get => _autoSizeWidth;
            set => SetProperty(ref _autoSizeWidth, value);
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.None;
        }

        private bool _autoSizeHeight = false;
        public bool AutoSizeHeight {
            get => _autoSizeHeight;
            set => SetProperty(ref _autoSizeHeight, value);
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

        protected void DrawText(SpriteBatch spriteBatch, Rectangle bounds, string text) {
            if (this.ShowShadow)
                Utils.DrawUtil.DrawAlignedText(spriteBatch, this.Font, text, bounds.OffsetBy(1, 1).OffsetBy(LeftOffset, 0), this.ShadowColor, this.HorizontalAlignment, this.VerticalAlignment);

            if (this.ShowShadow && this.StrokeShadow) {

                Utils.DrawUtil.DrawAlignedText(spriteBatch, this.Font, text, bounds.OffsetBy(0, -2).OffsetBy(LeftOffset, 0), this.ShadowColor * 0.5f, this.HorizontalAlignment, this.VerticalAlignment);
                Utils.DrawUtil.DrawAlignedText(spriteBatch, this.Font, text, bounds.OffsetBy(2, 0).OffsetBy(LeftOffset, 0),  this.ShadowColor * 0.5f, this.HorizontalAlignment, this.VerticalAlignment);
                Utils.DrawUtil.DrawAlignedText(spriteBatch, this.Font, text, bounds.OffsetBy(0, 2).OffsetBy(LeftOffset, 0),  this.ShadowColor * 0.5f, this.HorizontalAlignment, this.VerticalAlignment);
                Utils.DrawUtil.DrawAlignedText(spriteBatch, this.Font, text, bounds.OffsetBy(-2, 0).OffsetBy(LeftOffset, 0), this.ShadowColor * 0.5f, this.HorizontalAlignment, this.VerticalAlignment);

                Utils.DrawUtil.DrawAlignedText(spriteBatch, this.Font, text, bounds.OffsetBy(-1, -1).OffsetBy(LeftOffset, 0), this.ShadowColor * 0.5f, this.HorizontalAlignment, this.VerticalAlignment);
                Utils.DrawUtil.DrawAlignedText(spriteBatch, this.Font, text, bounds.OffsetBy(-1, 1).OffsetBy(LeftOffset, 0),  this.ShadowColor * 0.5f, this.HorizontalAlignment, this.VerticalAlignment);
                Utils.DrawUtil.DrawAlignedText(spriteBatch, this.Font, text, bounds.OffsetBy(1, -1).OffsetBy(LeftOffset, 0),  this.ShadowColor * 0.5f, this.HorizontalAlignment, this.VerticalAlignment);

                Utils.DrawUtil.DrawAlignedText(spriteBatch, this.Font, text, bounds.OffsetBy(0, -1).OffsetBy(LeftOffset, 0), this.ShadowColor * 0.5f, this.HorizontalAlignment, this.VerticalAlignment);
                Utils.DrawUtil.DrawAlignedText(spriteBatch, this.Font, text, bounds.OffsetBy(1, 0).OffsetBy(LeftOffset, 0),  this.ShadowColor * 0.5f, this.HorizontalAlignment, this.VerticalAlignment);
                Utils.DrawUtil.DrawAlignedText(spriteBatch, this.Font, text, bounds.OffsetBy(0, 1).OffsetBy(LeftOffset, 0),  this.ShadowColor * 0.5f, this.HorizontalAlignment, this.VerticalAlignment);
                Utils.DrawUtil.DrawAlignedText(spriteBatch, this.Font, text, bounds.OffsetBy(-1, 0).OffsetBy(LeftOffset, 0), this.ShadowColor * 0.5f, this.HorizontalAlignment, this.VerticalAlignment);
            }

            Utils.DrawUtil.DrawAlignedText(spriteBatch, this.Font, text, bounds.OffsetBy(LeftOffset, 0), this.TextColor, this.HorizontalAlignment, this.VerticalAlignment);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            if (this.Font == null || string.IsNullOrWhiteSpace(this.Text)) { return; }

            DrawText(spriteBatch, bounds, this.Text);
        }

    }
}
