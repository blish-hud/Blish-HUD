using System;
using System.Globalization;

namespace Blish_HUD.Controls {

    public class FloatInput : NumberInputBase {

        private float _minValue = float.MinValue;

        private float _maxValue = float.MaxValue;

        private float _incrementAmount = 0.1f;

        private float _originalValue;

        public event EventHandler<EventArgs>? ValueChanged;

        /// <summary>
        /// Gets or sets the increment/decrement amount for spinner buttons and scroll wheel.
        /// </summary>
        public float IncrementAmount {
            get => _incrementAmount;
            set => SetProperty(ref _incrementAmount, value);
        }

        public float Value {
            get => InvariantUtil.TryParseFloat(Text, out float value) ? value : 0f;
            set {
                if (value < MinValue) {
                    value = MinValue;
                } else if (value > MaxValue) {
                    value = MaxValue;
                }

                string text = value.ToString(_formatString, NumberFormatInfo.InvariantInfo);
                if (!string.Equals(Text, text, StringComparison.Ordinal)) {
                    Text = text;
                    Invalidate();
                    OnValueChanged();
                }
            }
        }

        public float MinValue {
            get => _minValue;
            set {
                _minValue = value;
                if (Value < value) {
                    Value = value;
                }
            }
        }

        public float MaxValue {
            get => _maxValue;
            set {
                _maxValue = value;
                if (Value > value) {
                    Value = value;
                }
            }
        }

        protected override void IncrementValue() {
            Value += _incrementAmount;
        }

        protected override void DecrementValue() {
            Value -= _incrementAmount;
        }

        protected override void StoreOriginalValue() {
            _originalValue = Value;
        }

        protected override void ApplyTextAsValue() {
            if (InvariantUtil.TryParseFloat(_text, out float value)) {
                Value = value;
                Invalidate();
                OnValueChanged();
            } else {
                Value = _originalValue;
            }
        }

        protected override void HandleMouseWheelScrolled(int scrollDelta) {
            if (scrollDelta > 0) {
                Value += _incrementAmount;
            } else {
                Value -= _incrementAmount;
            }
        }

        private void OnValueChanged() {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void DisposeControl() {
            ValueChanged = null;
            base.DisposeControl();
        }
    }
}
