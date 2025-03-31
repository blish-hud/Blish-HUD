using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD.Controls {
    public class FormattedLabel : Control {
        private readonly List<(RectangleWrapper Rectangle, FormattedLabelPart Text, object ToDraw)> _rectangles = new List<(RectangleWrapper, FormattedLabelPart, object)>();
        private readonly IEnumerable<FormattedLabelPart> _parts;
        private readonly bool _wrapText;
        private readonly bool _autoSizeWidth;
        private readonly bool _autoSizeHeight;
        private readonly HorizontalAlignment _horizontalAlignment;
        private readonly VerticalAlignment _verticalAlignment;
        private FormattedLabelPart _hoveredTextPart;
        private bool finishedInitialization = false;

        internal FormattedLabel(IEnumerable<FormattedLabelPart> parts, bool wrapText, bool autoSizeWidth, bool autoSizeHeight, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment) {
            _parts = parts;
            _wrapText = wrapText;
            _autoSizeWidth = autoSizeWidth;
            _autoSizeHeight = autoSizeHeight;
            _horizontalAlignment = horizontalAlignment;
            _verticalAlignment = verticalAlignment;
        }

        public override void RecalculateLayout()
            => InitializeRectangles();

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e) => _hoveredTextPart?.Link?.Invoke();

        private Rectangle HandleFirstTextPart(FormattedLabelPart item, string firstText) {
            var textSize = item.Font.MeasureString(firstText);

            var rectangle = new Rectangle(0, 0, (int)Math.Ceiling(textSize.Width), (int)Math.Ceiling(textSize.Height));

            if (_rectangles.Count > 0) {
                var lastRectangle = _rectangles[_rectangles.Count - 1];
                rectangle.X = lastRectangle.Rectangle.X + lastRectangle.Rectangle.Width;
                rectangle.Y = lastRectangle.Rectangle.Y;
            }

            return rectangle;
        }

        private Rectangle HandleMultiLineText(FormattedLabelPart item, string text) {
            var textSize = item.Font.MeasureString(text);
            var possibleLastYRectangles = _rectangles.OrderByDescending(x => x.Rectangle.Y).GroupBy(x => x.Rectangle.Y).First();
            var lastYRectangle = possibleLastYRectangles.FirstOrDefault(x => x.Rectangle.Height != default);

            if (lastYRectangle == default) {
                lastYRectangle = possibleLastYRectangles.First();
            }

            return new Rectangle(0, lastYRectangle.Rectangle.Y + lastYRectangle.Rectangle.Height, (int)Math.Ceiling(textSize.Width), (int)Math.Ceiling(textSize.Height));
        }

        private void InitializeRectangles() {
            // No need to initialize anything if there is no space
            if (this.Width == 0 && !_autoSizeWidth) {
                return;
            }

            finishedInitialization = false;
            _rectangles.Clear();
            foreach (var item in _parts) {
                if (item.PrefixImage != null) {
                    var imageRectangle = new Rectangle(0, 0, item.PrefixImageSize.X, item.PrefixImageSize.Y);
                    if (_rectangles.Count > 0) {
                        var lastRectangle = _rectangles[_rectangles.Count - 1];
                        imageRectangle.X = lastRectangle.Rectangle.X + lastRectangle.Rectangle.Width;
                        imageRectangle.Y = lastRectangle.Rectangle.Y;
                    }

                    if (_wrapText && imageRectangle.X + imageRectangle.Width > this.Width) {
                        var possibleLastYRectangles = _rectangles.OrderByDescending(x => x.Rectangle.Y).GroupBy(x => x.Rectangle.Y).First();
                        var lastYRectangle = possibleLastYRectangles.FirstOrDefault(x => x.Rectangle.Height != default);
                        if (lastYRectangle == default) {
                            lastYRectangle = possibleLastYRectangles.First();
                        }

                        imageRectangle.X = 0;
                        imageRectangle.Y = lastYRectangle.Rectangle.Y + lastYRectangle.Rectangle.Height;
                    }

                    _rectangles.Add((new RectangleWrapper(imageRectangle), item, item.PrefixImage));
                }

                var splittedText = item.Text.Split(new[] { "\n" }, StringSplitOptions.None).ToList();
                string firstText = splittedText[0];
                var rectangle = HandleFirstTextPart(item, firstText);
                bool wrapped = false;
                if (_wrapText && rectangle.X + rectangle.Width > this.Width) {
                    var tempSplittedText = DrawUtil.WrapText(item.Font, firstText, this.Width - rectangle.X).Split(new[] { "\n" }, StringSplitOptions.None).ToList();
                    splittedText = new[] { string.Join("", tempSplittedText.Skip(1)) }.Concat(splittedText.Skip(1)).ToList();
                    firstText = tempSplittedText[0];
                    rectangle = HandleFirstTextPart(item, firstText);
                    wrapped = true;
                }

                _rectangles.Add((new RectangleWrapper(rectangle), item, firstText));

                for (int i = wrapped ? 0 : 1; i < splittedText.Count; i++) {
                    rectangle = HandleMultiLineText(item, splittedText[i]);
                    if (_wrapText && rectangle.X + rectangle.Width > this.Width) {
                        splittedText.InsertRange(i + 1, DrawUtil.WrapText(item.Font, splittedText[i], this.Width - rectangle.X).Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries));
                        splittedText.RemoveAt(i);

                        var newRectangle = HandleMultiLineText(item, splittedText[i]);

                        if (newRectangle == rectangle) {
                            // Nothing changed from previous iteration, therefor this loop would run infinitely. This means, the width is too small to even hold one of the words
                            // Maybe throw an exception here?
                            return;
                        }

                        rectangle = newRectangle;
                    }

                    _rectangles.Add((new RectangleWrapper(rectangle), item, splittedText[i]));
                }

                if (item.SuffixImage != null) {
                    var imageRectangle = new Rectangle(0, 0, item.SuffixImageSize.X, item.SuffixImageSize.Y);
                    if (_rectangles.Count > 0) {
                        var lastRectangle = _rectangles[_rectangles.Count - 1];
                        imageRectangle.X = lastRectangle.Rectangle.X + lastRectangle.Rectangle.Width;
                        imageRectangle.Y = lastRectangle.Rectangle.Y;
                    }

                    if (_wrapText && imageRectangle.X + imageRectangle.Width > this.Width) {
                        var possibleLastYRectangles = _rectangles.OrderByDescending(x => x.Rectangle.Y).GroupBy(x => x.Rectangle.Y).First();
                        var lastYRectangle = possibleLastYRectangles.FirstOrDefault(x => x.Rectangle.Height != default);
                        if (lastYRectangle == default) {
                            lastYRectangle = possibleLastYRectangles.First();
                        }

                        imageRectangle.X = 0;
                        imageRectangle.Y = lastYRectangle.Rectangle.Y + lastYRectangle.Rectangle.Height;
                    }

                    _rectangles.Add((new RectangleWrapper(imageRectangle), item, item.SuffixImage));
                }
            }

            if (_autoSizeWidth) {
                this.Width = _rectangles.GroupBy(x => x.Rectangle.Y).Select(x => x.Select(y => y.Rectangle.Width).Sum()).Max();
            }

            if (_autoSizeHeight) {
                this.Height = _rectangles.GroupBy(x => x.Rectangle.Y).Select(x => x.Max(x => x.Rectangle.Height)).Sum();
            }

            HandleHorizontalAlignment();
            HandleVerticalAlignment();

            // Needs to be done after vertical alignment bc it will change the height of the individual rectangles
            // and therefor can't be recognized as one row anymore
            HandleFontSizeDifferences();

            finishedInitialization = true;
        }

        private void HandleFontSizeDifferences() {
            var rows = _rectangles.GroupBy(x => x.Rectangle.Y).ToArray();

            foreach (var item in rows) {
                var maxHeightInRowRectangle = item.OrderByDescending(x => x.Rectangle.Height).First();

                foreach (var rectangle in item) {
                    int offset = maxHeightInRowRectangle.Rectangle.Height - rectangle.Rectangle.Height;
                    rectangle.Rectangle.Y += (int)Math.Floor(offset / 2.0);
                }
            }
        }

        private void HandleHorizontalAlignment() {
            if (_horizontalAlignment != HorizontalAlignment.Left) {
                foreach (var item in _rectangles.GroupBy(x => x.Rectangle.Y)) {
                    if (_horizontalAlignment == HorizontalAlignment.Center) {
                        int combinedWidth = item.Sum(x => x.Rectangle.Width);
                        int firstRectangleX = (this.Width / 2) - (combinedWidth / 2);

                        int nextRectangleX = firstRectangleX;
                        foreach (var rectangle in item) {
                            rectangle.Rectangle.X = nextRectangleX;
                            nextRectangleX = rectangle.Rectangle.X + rectangle.Rectangle.Width;
                        }
                    } else if (_horizontalAlignment == HorizontalAlignment.Right) {
                        var reversedOrder = item.Reverse().ToArray();
                        int nextRectangleX = this.Width - reversedOrder.First().Rectangle.Width;
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
            if (_verticalAlignment == VerticalAlignment.Middle) {
                var yGroups = _rectangles.GroupBy(x => x.Rectangle.Y).ToArray();
                int combinedHeight = yGroups.Select(x => x.OrderByDescending(x => x.Rectangle.Height).First().Rectangle.Height).Sum();
                int firstRectangleY = (this.Height / 2) - (combinedHeight / 2);
                int nextRectangleY = firstRectangleY;
                foreach (var item in yGroups) {
                    foreach (var rectangle in item) {
                        rectangle.Rectangle.Y = nextRectangleY;
                    }

                    int maxHeightInRow = item.Max(x => x.Rectangle.Height);
                    nextRectangleY += maxHeightInRow;
                }
            } else if (_verticalAlignment == VerticalAlignment.Bottom) {
                var yGroups = _rectangles.GroupBy(x => x.Rectangle.Y).Reverse().ToArray();
                int nextRectangleY = this.Height - yGroups.First().Max(x => x.Rectangle.Height);
                foreach (var item in yGroups) {
                    int maxHeightInRow = item.Max(x => x.Rectangle.Height);

                    foreach (var rectangle in item) {
                        rectangle.Rectangle.Y = nextRectangleY;
                    }

                    nextRectangleY -= maxHeightInRow;
                }
            }
        }

        public override void DoUpdate(GameTime gameTime) {
            var mousePosition = GameService.Input.Mouse.Position;
            if (finishedInitialization &&
                mousePosition.X > this.AbsoluteBounds.X &&
                mousePosition.X < this.AbsoluteBounds.X + this.AbsoluteBounds.Width &&
                mousePosition.Y > this.AbsoluteBounds.Y &&
                mousePosition.Y < this.AbsoluteBounds.Y + this.AbsoluteBounds.Height) {
                bool hoverSet = false;
                foreach (var rectangle in _rectangles) {
                    var destinationRectangle = rectangle.Rectangle.Rectangle.ToBounds(this.AbsoluteBounds);
                    if (rectangle.Text.Link != null &&
                        mousePosition.X > destinationRectangle.X &&
                        mousePosition.X < destinationRectangle.X + destinationRectangle.Width &&
                        mousePosition.Y > destinationRectangle.Y &&
                        mousePosition.Y < destinationRectangle.Y + destinationRectangle.Height) {
                        _hoveredTextPart = rectangle.Text;
                        hoverSet = true;
                    }
                }

                if (!hoverSet) {
                    _hoveredTextPart = null;
                }

                base.DoUpdate(gameTime);
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            if (finishedInitialization) {
                float absoluteOpacity = this.AbsoluteOpacity();

                foreach (var rectangle in _rectangles) {
                    var destinationRectangle = rectangle.Rectangle.Rectangle.ToBounds(this.AbsoluteBounds);
                    var textColor = rectangle.Text.TextColor;

                    if (_hoveredTextPart != null && rectangle.Text == _hoveredTextPart) {
                        textColor = rectangle.Text.HoverColor;
                    }

                    if (rectangle.ToDraw is string stringText) {
                        spriteBatch.DrawString(rectangle.Text.Font, stringText, destinationRectangle.Location.ToVector2(), textColor * absoluteOpacity);
                    } else if (rectangle.ToDraw is AsyncTexture2D texture) {
                        spriteBatch.Draw(texture, destinationRectangle, Color.White * absoluteOpacity);
                    }

                    if (rectangle.Text.IsUnderlined) {
                        spriteBatch.DrawLine(new Vector2(destinationRectangle.X, destinationRectangle.Y + destinationRectangle.Height), new Vector2(destinationRectangle.X + destinationRectangle.Width, destinationRectangle.Y + destinationRectangle.Height), textColor * absoluteOpacity, thickness: 2);
                    }

                    if (rectangle.Text.IsStrikeThrough) {
                        // TODO: Still seemed not centered
                        spriteBatch.DrawLine(new Vector2(destinationRectangle.X, destinationRectangle.Y + (destinationRectangle.Height / 2)), new Vector2(destinationRectangle.X + destinationRectangle.Width, destinationRectangle.Y + (destinationRectangle.Height / 2)), textColor * absoluteOpacity, thickness: 2);
                    }
                }
            }
        }

        protected override void DisposeControl() {
            foreach (var item in _parts) {
                item.Dispose();
            }

            base.DisposeControl();
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
}