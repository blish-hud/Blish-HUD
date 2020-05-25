using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Controls
{
    public class CounterBox : Control
    {

        private readonly Texture2D MinusSprite;
        private readonly Texture2D PlusSprite;
        private int _valueWidth = 100;
        public int ValueWidth
        {
            get => _valueWidth;
            set
            {
                if (_valueWidth == value) return;
                _valueWidth = value;
                Invalidate();
            }
        }
        private string _prefix = "";
        public string Prefix
        {
            get => _prefix;
            set
            {
                if (string.Equals(_prefix, value)) return;
                _prefix = value;
                Invalidate();
            }
        }
        private string _suffix = "";
        public string Suffix
        {
            get => _suffix;
            set
            {
                if (string.Equals(_suffix, value)) return;
                _suffix = value;
                Invalidate();
            }
        }
        private int _value = 1;
        public int Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                if (value < _minValue || value > _maxValue) return;
                _value = value;
                Invalidate();
            }
        }
        private int _numerator = 1;
        public int Numerator
        {
            get => _numerator;
            set
            {
                if (_numerator == value) return;
                _numerator = value;
            }
        }
        private int _maxValue = 1;
        public int MaxValue
        {
            get => _maxValue;
            set
            {
                if (_maxValue == value) return;
                if (value < _minValue) return;
                _maxValue = value;
                Invalidate();
            }
        }
        private int _minValue = 0;
        public int MinValue
        {
            get => _minValue;
            set
            {
                if (_minValue == value) return;
                if (value > _maxValue) return;
                _minValue = value;
                Invalidate();
            }
        }
        private bool _exponential;
        public bool Exponential
        {
            get => _exponential;
            set
            {
                if (_exponential == value) return;
                _exponential = value;
            }
        }
        public CounterBox()
        {
            MinusSprite = MinusSprite ?? Content.GetTexture("minus");
            PlusSprite = PlusSprite ?? Content.GetTexture("plus");
            this.MouseMoved += CounterBoxOnMouseMoved;
            this.MouseLeft += CounterBoxOnMouseLeft;
            this.LeftMouseButtonPressed += CounterBoxOnLeftMouseButtonPressed;
            this.Size = new Point(150, 20);
        }
        private bool _mouseOverPlus = false;
        private bool MouseOverPlus
        {
            get => _mouseOverPlus;
            set
            {
                if (_mouseOverPlus == value) return;
                _mouseOverPlus = value;
                Invalidate();
            }
        }
        private bool _mouseOverMinus = false;
        private bool MouseOverMinus
        {
            get => _mouseOverMinus;
            set
            {
                if (_mouseOverMinus == value) return;
                _mouseOverMinus = value;
                Invalidate();
            }
        }
        private void CounterBoxOnMouseLeft(object sender, MouseEventArgs e)
        {
            this.MouseOverPlus = false;
            this.MouseOverMinus = false;
        }
        private void CounterBoxOnMouseMoved(object sender, MouseEventArgs e)
        {
            var relPos = e.MouseState.Position - this.AbsoluteBounds.Location;

            if (this.MouseOver)
            {

                this.MouseOverMinus = relPos.X < 17 && relPos.X > 0;
                this.MouseOverPlus = relPos.X < 36 + this.ValueWidth && relPos.X > 19 + this.ValueWidth;

            }
            else
            {
                this.MouseOverMinus = false;
                this.MouseOverPlus = false;
            }
        }
        private void CounterBoxOnLeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (_mouseOverMinus) {
                if (_exponential) {
                    var halfed = _value / 2;

                    _value = halfed < _minValue ? _minValue : halfed;

                } else {
                    var difference = _value - _numerator;

                    if (difference >= _minValue) {
                        _value = difference;
                    }

                    this.Invalidate();
                }
            }
            if (_mouseOverPlus) {
                if (_exponential)
                {
                    var doubled = this.Value + this.Value;
                    _value = doubled > _maxValue ? _maxValue : doubled;

                } else {
                    var summation = _value + _numerator;
                    if (summation <= _maxValue)
                    {
                        _value = summation;
                    }
                }
                Invalidate();
            }
        }
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (_mouseOverMinus && GameService.Input.Mouse.State.LeftButton == ButtonState.Pressed) {
                spriteBatch.DrawOnCtrl(this, MinusSprite, new Rectangle(2, 2, 15, 15), Color.White);
            } else {
                spriteBatch.DrawOnCtrl(this, MinusSprite, new Rectangle(0, 0, 17, 17), Color.White);
            }
            var combine = _prefix + _value + _suffix;
            spriteBatch.DrawStringOnCtrl(this, combine, Content.DefaultFont14, new Rectangle(18, 0, _valueWidth, 17), Color.White, false, true, 1, HorizontalAlignment.Center, VerticalAlignment.Middle);

            if (_mouseOverPlus && GameService.Input.Mouse.State.LeftButton == ButtonState.Pressed) {
                spriteBatch.DrawOnCtrl(this, PlusSprite, new Rectangle(21 + _valueWidth, 2, 15, 15), Color.White);
            } else {
                spriteBatch.DrawOnCtrl(this, PlusSprite, new Rectangle(19 + _valueWidth, 0, 17, 17), Color.White);
            }
        }
    }
}