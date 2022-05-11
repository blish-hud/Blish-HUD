using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD.Controls {

    internal class FormattedTextPart {
        public BitmapFont Font { get; }

        public bool IsBold { get; }

        public bool IsItalic { get; }

        public bool IsStrikeThrough { get; }

        public bool IsUnderlined { get; }

        public string Text { get; }

        public Action Link { get; }

        public Texture2D PrefixImage { get; }
        
        public ContentService.FontSize FontSize { get; }
        
        public ContentService.FontFace FontFace { get; }
        
        public Color TextColor { get; }

        public Color HoverColor { get; }

        public FormattedTextPart(
            bool isBold,
            bool isItalic,
            bool isStrikeThrough,
            bool isUnderlined,
            string text,
            Action link,
            Texture2D prefixImage,
            Color textColor,
            Color hoverColor,
            ContentService.FontSize fontSize,
            ContentService.FontFace fontFace) {
            this.IsBold = isBold;
            this.IsItalic = isItalic;
            this.IsStrikeThrough = isStrikeThrough;
            this.IsUnderlined = isUnderlined;
            this.Text = text;
            this.Link = link;
            this.PrefixImage = prefixImage;
            this.HoverColor = hoverColor;
            this.FontSize = fontSize;
            this.FontFace = fontFace;
            this.TextColor = textColor == default ? Color.White : textColor;

            var style = ContentService.FontStyle.Regular;

            if (this.IsItalic) {
                style = ContentService.FontStyle.Italic;
            } else if (this.IsBold) {
                style = ContentService.FontStyle.Bold;
            }

            this.Font = GameService.Content.GetFont(this.FontFace, this.FontSize, style);
        }
    }

    public class FormattedTextPartBuilder {
        private readonly string text;
        private bool isBold;
        private bool isItalic;
        private bool isStrikeThrough;
        private bool isUnderlined;
        private Action link;
        private Texture2D prefixImage;
        private Color textColor;
        private Color hoverColor = Color.LightBlue;
        private ContentService.FontSize fontSize = ContentService.FontSize.Size18;

        public FormattedTextPartBuilder(string text) {
            this.text = text;
        }

        public FormattedTextPartBuilder MakeBold() {
            if (this.isItalic) {
                throw new InvalidOperationException("Cant make text italic and bold");
            }

            this.isBold = true;
            return this;
        }

        public FormattedTextPartBuilder MakeItalic() {
            if (this.isBold) {
                throw new InvalidOperationException("Cant make text italic and bold");
            }

            this.isItalic = true;
            return this;
        }

        public FormattedTextPartBuilder MakeStrikeThrough() {
            this.isStrikeThrough = true;
            return this;
        }

        public FormattedTextPartBuilder MakeUnderlined() {
            this.isUnderlined = true;
            return this;
        }

        public FormattedTextPartBuilder SetLink(Action onLink) {
            this.link = onLink;
            return this;
        }

        public FormattedTextPartBuilder SetHyperLink(string link) {
            this.link = new Action(() => System.Diagnostics.Process.Start(link));
            return this;
        }

        public FormattedTextPartBuilder SetPrefixImage(Texture2D prefixImage) {
            this.prefixImage = prefixImage;
            return this;
        }

        public FormattedTextPartBuilder SetTextColor(Color textColor) {
            this.textColor = textColor;
            return this;
        }

        public FormattedTextPartBuilder SetHoverColor(Color hoverColor) {
            this.hoverColor = hoverColor;
            return this;
        }

        public FormattedTextPartBuilder SetFontSize(ContentService.FontSize fontSize) {
            this.fontSize = fontSize;
            return this;
        }

        internal FormattedTextPart Build()
            => new FormattedTextPart(this.isBold, this.isItalic, this.isStrikeThrough, this.isUnderlined, this.text, this.link, this.prefixImage, this.textColor, this.hoverColor, this.fontSize, ContentService.FontFace.Menomonia);
    }

    public class FormattedText : Control {
        private List<(RectangleWrapper Rectangle, FormattedTextPart Text, string StringText)> rectangles = new List<(RectangleWrapper, FormattedTextPart, string)>();
        private IEnumerable<FormattedTextPart> parts;
        private readonly bool wrapText;
        private readonly bool autoSizeWidth;
        private readonly HorizontalAlignment horizontalAlignment;
        private readonly VerticalAlignment verticalAlignment;
        private FormattedTextPart hoveredTextPart;

        internal FormattedText(IEnumerable<FormattedTextPart> parts, bool wrapText, bool autoSizeWidth, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment) {
            this.parts = parts;
            this.wrapText = wrapText;
            this.autoSizeWidth = autoSizeWidth;
            this.horizontalAlignment = horizontalAlignment;
            this.verticalAlignment = verticalAlignment;
        }

        public override void RecalculateLayout() {
            this.InitializeRectangles();
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e) {
            if (this.hoveredTextPart != null) {
                this.hoveredTextPart.Link?.Invoke();
            }
        }

        private Rectangle HandleFirstTextPart(FormattedTextPart item, string firstText) {
            var textSize = item.Font.MeasureString(firstText);

            var rectangle = new Rectangle(0, 0, (int)Math.Ceiling(textSize.Width), (int)Math.Ceiling(textSize.Height));

            if (rectangles.Count > 0) {
                var lastRectangle = rectangles[rectangles.Count - 1];
                rectangle.X = lastRectangle.Rectangle.X + lastRectangle.Rectangle.Width;
                rectangle.Y = lastRectangle.Rectangle.Y;
            }

            return rectangle;
        }

        private Rectangle HandleMultiLineText(FormattedTextPart item, string text) {
            var textSize = item.Font.MeasureString(text);
            var possibleLastYRectangles = rectangles.OrderByDescending(x => x.Rectangle.Y).GroupBy(x => x.Rectangle.Y).First();
            var lastYRectangle = possibleLastYRectangles.FirstOrDefault(x => x.Rectangle.Height != default);

            if (lastYRectangle == default) {
                lastYRectangle = possibleLastYRectangles.First();
            }

            return new Rectangle(0, lastYRectangle.Rectangle.Y + lastYRectangle.Rectangle.Height, (int)Math.Ceiling(textSize.Width), (int)Math.Ceiling(textSize.Height));
        }

        private void InitializeRectangles() {
            this.rectangles.Clear();
            foreach (var item in parts) {
                var splittedText = item.Text.Split(new[] { "\n" }, StringSplitOptions.None).ToList();
                var firstText = splittedText[0];
                var rectangle = this.HandleFirstTextPart(item, firstText);

                if (this.wrapText && rectangle.X + rectangle.Width > this.Width) {
                    splittedText = DrawUtil.WrapText(item.Font, firstText, this.Width - rectangle.X).Split(new[] { "\n" }, StringSplitOptions.None).Concat(splittedText.Skip(1)).ToList();
                    rectangle = this.HandleFirstTextPart(item, firstText);
                }

                rectangles.Add((new RectangleWrapper(rectangle), item, firstText));

                for (int i = 1; i < splittedText.Count; i++) {
                    rectangle = this.HandleMultiLineText(item, splittedText[i]);
                    if (this.wrapText && rectangle.X + rectangle.Width > this.Width) {
                        splittedText.InsertRange(i + 1, DrawUtil.WrapText(item.Font, splittedText[i], this.Width - rectangle.X).Split(new[] { "\n" }, StringSplitOptions.None));
                        splittedText.RemoveAt(i);

                        rectangle = this.HandleMultiLineText(item, splittedText[i]);
                    }

                    rectangles.Add((new RectangleWrapper(rectangle), item, splittedText[i]));

                }
            }

            if (this.autoSizeWidth) {
                this.Width = this.rectangles.GroupBy(x => x.Rectangle.Y).Select(x => x.Select(y => y.Rectangle.Width).Sum()).Max();
            }

            this.HandleHorizontalAlignment();
            this.HandleVerticalAlignment();

            // Needs to be done after vertical alignment bc it will change the height of the individual rectangles
            // and therefor can't be recognized as one row anymore
            this.HandleFontSizeDifferences();
        }

        private void HandleFontSizeDifferences() {
            var rows = this.rectangles.GroupBy(x => x.Rectangle.Y).ToArray();

            foreach (var item in rows) {
                var maxHeightInRow = item.Max(x => x.Rectangle.Height);

                foreach (var rectangle in item) {
                    var offset = item.Key + maxHeightInRow - rectangle.Rectangle.Y - rectangle.Rectangle.Height;
                    rectangle.Rectangle.Y += (int)Math.Floor(offset / 2.0);
                }
            }
        }

        private void HandleHorizontalAlignment() {
            if (this.horizontalAlignment != HorizontalAlignment.Left) {
                foreach (var item in this.rectangles.GroupBy(x => x.Rectangle.Y)) {
                    if (horizontalAlignment == HorizontalAlignment.Center) {
                        var combinedWidth = item.Sum(x => x.Rectangle.Width);
                        var firstRectangleX = (this.Width / 2) - (combinedWidth / 2);

                        var nextRectangleX = firstRectangleX;
                        foreach (var rectangle in item) {
                            rectangle.Rectangle.X = nextRectangleX;
                            nextRectangleX = rectangle.Rectangle.X + rectangle.Rectangle.Width;
                        }
                    } else if (horizontalAlignment == HorizontalAlignment.Right) {
                        var reversedOrder = item.Reverse().ToArray();
                        var nextRectangleX = this.Width - reversedOrder.First().Rectangle.Width;
                        for (int i = 0; i < reversedOrder.Length; i++) {
                            reversedOrder[i].Rectangle.X = nextRectangleX;

                            if (i != reversedOrder.Length - 1) {
                                nextRectangleX = reversedOrder[i].Rectangle.X - reversedOrder[i + 1].Rectangle.Width;
                            }
                        }
                    }
                }
            }
        }

        private void HandleVerticalAlignment() {
            if (this.verticalAlignment == VerticalAlignment.Middle) {
                var yGroups = this.rectangles.GroupBy(x => x.Rectangle.Y).ToArray();
                var combinedHeight = yGroups.Select(x => x.OrderByDescending(x => x.Rectangle.Height).First().Rectangle.Height).Sum();
                var firstRectangleY = (this.Height / 2) - (combinedHeight / 2);
                var nextRectangleY = firstRectangleY;
                foreach (var item in yGroups) {
                    foreach (var rectangle in item) {
                        rectangle.Rectangle.Y = nextRectangleY;
                    }
                    var maxHeightInRow = item.Max(x => x.Rectangle.Height);
                    nextRectangleY += maxHeightInRow;
                }
            } else if (this.verticalAlignment == VerticalAlignment.Bottom) {
                var yGroups = this.rectangles.GroupBy(x => x.Rectangle.Y).Reverse().ToArray();
                var nextRectangleY = this.Height - yGroups.First().Max(x => x.Rectangle.Height);
                foreach (var item in yGroups) {
                    var maxHeightInRow = item.Max(x => x.Rectangle.Height);

                    foreach (var rectangle in item) {
                        rectangle.Rectangle.Y = nextRectangleY;
                    }
                    nextRectangleY -= maxHeightInRow;
                }
            }
        }

        public override void DoUpdate(GameTime gameTime) {

            var hoverSet = false;
            foreach (var rectangle in this.rectangles) {
                var destinationRectangle = rectangle.Rectangle.Rectangle.ToBounds(this.AbsoluteBounds);
                var mousePosition = GameService.Input.Mouse.Position;
                if (rectangle.Text.Link != null && mousePosition.X > destinationRectangle.X && mousePosition.X < destinationRectangle.X + destinationRectangle.Width && mousePosition.Y > destinationRectangle.Y && mousePosition.Y < destinationRectangle.Y + destinationRectangle.Height) {
                    this.hoveredTextPart = rectangle.Text;
                    hoverSet = true;
                }
            }

            if (!hoverSet) {
                this.hoveredTextPart = null;
            }

            base.DoUpdate(gameTime);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            foreach (var rectangle in this.rectangles) {
                var destinationRectangle = rectangle.Rectangle.Rectangle.ToBounds(this.AbsoluteBounds);
                var textColor = rectangle.Text.TextColor;

                if (this.hoveredTextPart != null && rectangle.Text == this.hoveredTextPart) {
                    textColor = rectangle.Text.HoverColor;
                }

                spriteBatch.DrawString(rectangle.Text.Font, rectangle.StringText, new Vector2(destinationRectangle.X, destinationRectangle.Y), textColor);

                if (rectangle.Text.IsUnderlined) {
                    spriteBatch.DrawLine(new Vector2(destinationRectangle.X, destinationRectangle.Y + destinationRectangle.Height), new Vector2(destinationRectangle.X + destinationRectangle.Width, destinationRectangle.Y + destinationRectangle.Height), textColor, thickness: 2);
                }

                if (rectangle.Text.IsStrikeThrough) {
                    // TODO: Still seemed not centered
                    spriteBatch.DrawLine(new Vector2(destinationRectangle.X, destinationRectangle.Y + (destinationRectangle.Height / 2)), new Vector2(destinationRectangle.X + destinationRectangle.Width, destinationRectangle.Y + (destinationRectangle.Height / 2)), textColor, thickness: 2);
                }
            }
        }

        private class RectangleWrapper {
            public Rectangle Rectangle { get; set; }

            public int X {
                get => this.Rectangle.X;
                set {
                    var rectangle = this.Rectangle;
                    rectangle.X = value;
                    this.Rectangle = rectangle;
                }
            }

            public int Y {
                get => this.Rectangle.Y;
                set {
                    var rectangle = this.Rectangle;
                    rectangle.Y = value;
                    this.Rectangle = rectangle;
                }
            }

            public int Width {
                get => this.Rectangle.Width;
                set {
                    var rectangle = this.Rectangle;
                    rectangle.Width = value;
                    this.Rectangle = rectangle;
                }
            }

            public int Height {
                get => this.Rectangle.Height;
                set {
                    var rectangle = this.Rectangle;
                    rectangle.Height = value;
                    this.Rectangle = rectangle;
                }
            }

            public RectangleWrapper(Rectangle rectangle) {
                this.Rectangle = rectangle;
            }
        }
    }

    public class FormattedTextBuilder {
        private readonly List<FormattedTextPart> parts = new List<FormattedTextPart>();
        private bool wrapText;
        private int width;
        private bool autoSizeWidth;
        private HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left;
        private VerticalAlignment verticalAlignment = VerticalAlignment.Middle;

        public FormattedTextBuilder CreatePart(string text, Action<FormattedTextPartBuilder> creationFunc) {
            var builder = new FormattedTextPartBuilder(text);
            creationFunc?.Invoke(builder);
            this.parts.Add(builder.Build());
            return this;
        }

        public FormattedTextBuilder Wrap() {
            this.wrapText = true;
            return this;
        }

        public FormattedTextBuilder SetWidth(int width) {
            this.autoSizeWidth = false;
            this.width = width;
            return this;
        }

        public FormattedTextBuilder AutoSizeWidth() {
            this.width = default;
            this.autoSizeWidth = true;
            return this;
        }

        public FormattedTextBuilder SetHorizontalAlignment(HorizontalAlignment horizontalAlignment) {
            this.horizontalAlignment = horizontalAlignment;
            return this;
        }

        public FormattedTextBuilder SetVerticalAlignment(VerticalAlignment verticalAlignment) {
            this.verticalAlignment = verticalAlignment;
            return this;
        }

        public FormattedText Build()
            => new FormattedText(this.parts, this.wrapText, this.autoSizeWidth, this.horizontalAlignment, this.verticalAlignment) {
                Width = this.width,
            };
    }

    public abstract class LabelBase : Control {

        private CachedStringRender _labelRender;
        protected string _text;
        protected BitmapFont _font;
        protected bool _cacheLabel = false;
        protected Color _textColor = Color.White;
        protected HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Left;
        protected VerticalAlignment _verticalAlignment = VerticalAlignment.Middle;
        protected bool _wrapText = false;
        protected bool _showShadow = false;
        protected bool _strokeText = false;
        protected Color _shadowColor = Color.Black;
        protected bool _autoSizeWidth = false;
        protected bool _autoSizeHeight = false;

        protected LabelBase() {
            _font = Content.DefaultFont14;
        }

        /// <summary>
        /// If either <see cref="_autoSizeWidth"/> or <see cref="_autoSizeHeight"/> is enabled,
        /// this will indicate the size of the label region after <see cref="RecalculateLayout"/>
        /// has completed.
        /// </summary>
        protected Point LabelRegion = Point.Zero;

        public override void RecalculateLayout() {
            int lblRegionWidth = _size.X;
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
            text = text ?? _text;

            if (!_autoSizeWidth && _wrapText) {
                text = DrawUtil.WrapText(_font, text, LabelRegion.X > 0 ? LabelRegion.X : _size.X);
            }

            return _font.MeasureString(text ?? _text);
        }

        protected void DrawText(SpriteBatch spriteBatch, Rectangle bounds, string text = null) {
            text = text ?? _text;

            if (_font == null || string.IsNullOrEmpty(text)) return;

            if (_showShadow && !_strokeText) {
                spriteBatch.DrawStringOnCtrl(this, text, _font, bounds.OffsetBy(1, 1), _shadowColor, _wrapText, _horizontalAlignment, _verticalAlignment);
            }

            if (_cacheLabel && _labelRender != null) {
                spriteBatch.DrawOnCtrl(this, _labelRender.CachedRender, bounds);
            } else {
                spriteBatch.DrawStringOnCtrl(this, text, _font, bounds, _textColor, _wrapText, _strokeText, 1, _horizontalAlignment, _verticalAlignment);
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            DrawText(spriteBatch, bounds, _text);
        }

    }
}