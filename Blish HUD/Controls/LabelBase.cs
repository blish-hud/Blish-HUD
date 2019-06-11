using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD.Controls {
    public abstract class LabelBase : Control {

        private CachedStringRender _labelRender;

        protected bool _cacheLabel = false;

        protected string _text;

        protected BitmapFont _font;

        protected Color _textColor = Color.White;

        protected Utils.DrawUtil.HorizontalAlignment _horizontalAlignment = Utils.DrawUtil.HorizontalAlignment.Left;

        protected Utils.DrawUtil.VerticalAlignment _verticalAlignment = Utils.DrawUtil.VerticalAlignment.Top;

        protected bool _showShadow = false;

        protected bool _strokeText = false;

        protected Color _shadowColor = Color.Black;

        protected bool _autoSizeWidth = false;

        protected bool _autoSizeHeight = false;

        public LabelBase() {
            _font = Content.DefaultFont14;
        }

        protected override CaptureType CapturesInput() {
            return string.IsNullOrEmpty(this.BasicTooltipText) ? CaptureType.None : CaptureType.Mouse;
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
                var textSize = GetTextDimensions();

                if (_autoSizeWidth) {
                    lblRegionWidth = (int)Math.Ceiling(textSize.Width + (_showShadow || _strokeText ? 1 : 0));
                }

                if (_autoSizeHeight) {
                    lblRegionHeight = (int)Math.Ceiling(textSize.Height + (_showShadow || _strokeText ? 1 : 0));
                }
            }

            LabelRegion = new Point(lblRegionWidth, lblRegionHeight);

            this.Size = LabelRegion;

            if (_cacheLabel) {
                _labelRender = CachedStringRender.GetCachedStringRender(_text,
                                                                       _font,
                                                                       new Rectangle(Point.Zero, LabelRegion),
                                                                       _textColor,
                                                                       false,
                                                                       _strokeText,
                                                                       1,
                                                                       _horizontalAlignment,
                                                                       _verticalAlignment);
            }
        }

        protected Size2 GetTextDimensions(string text = null) {
            return _font.MeasureString(text ?? _text);
        }
        
        protected void DrawText(SpriteBatch spriteBatch, Rectangle bounds, string text = null) {
            text = text ?? _text;

            if (_font == null || string.IsNullOrEmpty(text)) { return; }

            if (_showShadow && !_strokeText)
                spriteBatch.DrawStringOnCtrl(this, text, _font, bounds.OffsetBy(1, 1), _shadowColor, false, _horizontalAlignment, _verticalAlignment);
            
            if (_cacheLabel && _labelRender != null)
                spriteBatch.DrawOnCtrl(this, _labelRender.CachedRender, bounds);
            else
                spriteBatch.DrawStringOnCtrl(this, text, _font, bounds, _textColor, false, _strokeText, 1, _horizontalAlignment, _verticalAlignment);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            DrawText(spriteBatch, bounds, _text);
        }

        protected override void DisposeControl() {
            
        }

    }
}
