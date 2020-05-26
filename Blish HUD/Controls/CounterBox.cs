using System;
using System.Diagnostics;
using System.Timers;
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
        /// <summary>
        /// The width of the value's display space (ie. the gap between the increment and the decrement button.)
        /// </summary>
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
        /// <summary>
        /// Optional prefix to be prepended to the displayed value.
        /// </summary>
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
        /// <summary>
        /// Optional suffix to be appended to the displayed value.
        /// </summary>
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
        private int _numerator = 1;
        /// <summary>
        /// The numerator by which to increment or decrement the value of the counterbox.
        /// </summary>
        public int Numerator
        {
            get => _numerator;
            set => _numerator = value;
        }
        private int _maxValue = 1;
        /// <summary>
        /// The maximum value of the counterbox. Cannot be lesser than MinValue, thus should be assigned BEFORE MinValue.
        /// </summary>
        public int MaxValue
        {
            get => _maxValue;
            set
            {
                if (value < _minValue) return;
                _maxValue = value;
                Invalidate();
            }
        }
        private int _minValue;
        /// <summary>
        /// The minimum value of the counterbox. Cannot be greater than MaxValue, thus should be assigned AFTER MaxValue;
        /// </summary>
        public int MinValue
        {
            get => _minValue;
            set
            {
                if (value > _maxValue) return;
                _minValue = value;
                Invalidate();
            }
        }
        private bool _exponential;
        /// <summary>
        /// If set, doubles the value when incrementing and halfs it when decrementing.
        /// </summary>
        public bool Exponential
        {
            get => _exponential;
            set => _exponential = value;
        }
        private int _value = 1;
        /// <summary>
        /// The value of the counterbox. Cannot be greater than MaxValue and lesser than MinValue, thus should be assigned AFTER both;
        /// </summary>
        public int Value {
            get => _value;
            set {
                _value = MathHelper.Clamp(value, MinValue, MaxValue);
                Invalidate();
            }
        }
        private bool _pressed;
        private readonly Stopwatch _holdTimerFast = new Stopwatch();
        private Timer _holdTimer;
        private const int HOLD_MILISECONDS = 700;
        public CounterBox() {
            _holdTimer = new Timer(HOLD_MILISECONDS);
            MinusSprite = MinusSprite ?? Content.GetTexture("minus");
            PlusSprite = PlusSprite ?? Content.GetTexture("plus");
            this.MouseMoved += OnMouseMoved;
            this.MouseLeft += OnMouseLeft;
            this.LeftMouseButtonPressed += OnLeftMouseButtonPressed;
            this.LeftMouseButtonReleased += OnLeftMouseButtonReleased;
            this.Disposed += delegate { _holdTimer?.Close(); _holdTimerFast?.Stop(); };
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
        private void OnMouseLeft(object sender, MouseEventArgs e) {
            _pressed = false;
            this.MouseOverPlus = false;
            this.MouseOverMinus = false;
            ResetHoldTimer();
        }
        private void OnMouseMoved(object sender, MouseEventArgs e)
        {
            var relPos = e.MouseState.Position - this.AbsoluteBounds.Location;

            if (this.MouseOver)
            {
                this.MouseOverMinus = relPos.X < 17 && relPos.X > 0;
                this.MouseOverPlus = relPos.X < 36 + this.ValueWidth && relPos.X > 19 + this.ValueWidth;

                if (!_mouseOverMinus && !_mouseOverPlus) ResetHoldTimer();

            } else {
                this.MouseOverMinus = false;
                this.MouseOverPlus = false;

            }
        }

        private void OnLeftMouseButtonReleased(object sender, MouseEventArgs e) {
            _pressed = false;
            ResetHoldTimer();
        }
        private void OnLeftMouseButtonPressed(object sender, MouseEventArgs e) {
            _pressed = true;
            ChangeValue();
            _holdTimer.Elapsed += delegate {
                ChangeValue();
                _holdTimer.Interval = _holdTimerFast.ElapsedMilliseconds > 2000 ? (_holdTimerFast.ElapsedMilliseconds > 4000 ? 25 : 50) : 100;
            };
            _holdTimer.Start();
            _holdTimerFast.Start();
        }
        private void ChangeValue() {
            if (_mouseOverMinus)
                if (_exponential)
                    Value /= 2;
                else
                    Value -= _numerator;

            if (_mouseOverPlus)
                if (_exponential)
                    Value *= 2;
                else
                    Value += _numerator;
        }
        private void ResetHoldTimer() {
            _holdTimer.Stop();
            _holdTimer.Dispose();
            _holdTimer = new Timer(HOLD_MILISECONDS);
            _holdTimerFast.Reset();
        }
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (_mouseOverMinus && _pressed) {
                spriteBatch.DrawOnCtrl(this, MinusSprite, new Rectangle(2, 2, 15, 15), Color.White);
            } else {
                spriteBatch.DrawOnCtrl(this, MinusSprite, new Rectangle(0, 0, 17, 17), Color.White);
            }
            var combine = _prefix + _value + _suffix;
            spriteBatch.DrawStringOnCtrl(this, combine, Content.DefaultFont14, new Rectangle(18, 0, _valueWidth, 17), Color.White, false, true, 1, HorizontalAlignment.Center, VerticalAlignment.Middle);

            if (_mouseOverPlus && _pressed) {
                spriteBatch.DrawOnCtrl(this, PlusSprite, new Rectangle(21 + _valueWidth, 2, 15, 15), Color.White);
            } else {
                spriteBatch.DrawOnCtrl(this, PlusSprite, new Rectangle(19 + _valueWidth, 0, 17, 17), Color.White);
            }
        }
    }
}