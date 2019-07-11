using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Blish_HUD.Controls
{
    public class CounterBox : Control
    {

        private Texture2D MinusSprite;
        private Texture2D PlusSprite;
        private int _valueWidth;
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
                _value = value;
                Invalidate();
            }
        }
        private int _numerator;
        public int Numerator
        {
            get => _numerator;
            set
            {
                if (_numerator == value) return;
                _numerator = value;
            }
        }
        private int _maxValue;
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
        private int _minValue;
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
        private bool _exponential = false;
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
            this.MouseMoved += CounterBox_MouseMoved;
            this.MouseLeft += CounterBox_MouseLeft;
            this.LeftMouseButtonPressed += CounterBox_LeftMouseButtonPressed;
            this.Size = new Point(150, 20);
        }
        private bool _mouseOverPlus = false;
        public bool MouseOverPlus
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
        public bool MouseOverMinus
        {
            get => _mouseOverMinus;
            set
            {
                if (_mouseOverMinus == value) return;
                _mouseOverMinus = value;
                Invalidate();
            }
        }
        private void CounterBox_MouseLeft(object sender, MouseEventArgs e)
        {
            this.MouseOverPlus = false;
            this.MouseOverMinus = false;
        }
        private void CounterBox_MouseMoved(object sender, MouseEventArgs e)
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
        private void CounterBox_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (this.MouseOverMinus) {
                if (this.Exponential)
                {
                    var halfed = this.Value / 2;
                    if (halfed >= this.MinValue)
                    {
                        this.Value = halfed;
                    }
                } else {
                    var difference = this.Value - this.Numerator;
                    if (difference >= this.MinValue)
                    {
                        this.Value = difference;
                    }
                    this.Invalidate();
                }
            }
            if (this.MouseOverPlus) {
                if (this.Exponential)
                {
                    var doubled = this.Value + this.Value;
                    if (doubled <= this.MaxValue)
                    {
                        this.Value = doubled;
                    }
                } else {
                    var summation = this.Value + this.Numerator;
                    if (summation <= this.MaxValue)
                    {
                        this.Value = summation;
                    }
                }
                this.Invalidate();
            }
        }
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this.MouseOverMinus) {
                spriteBatch.DrawOnCtrl(this, MinusSprite, new Rectangle(2, 2, 15, 15), Color.White);
            } else {
                spriteBatch.DrawOnCtrl(this, MinusSprite, new Rectangle(0, 0, 17, 17), Color.White);
            }
            var combine = this.Prefix + this.Value.ToString() + this.Suffix;
            spriteBatch.DrawStringOnCtrl(this, combine, Content.DefaultFont14, new Rectangle(18, 0, this.ValueWidth, 17), Color.White, false, true, 1, HorizontalAlignment.Center, VerticalAlignment.Middle);

            if (this.MouseOverPlus) {
                spriteBatch.DrawOnCtrl(this, PlusSprite, new Rectangle(21 + this.ValueWidth, 2, 15, 15), Color.White);
            } else {
                spriteBatch.DrawOnCtrl(this, PlusSprite, new Rectangle(19 + this.ValueWidth, 0, 17, 17), Color.White);
            }
        }
    }
}