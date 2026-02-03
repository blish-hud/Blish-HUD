using System;
using System.Globalization;

namespace Blish_HUD.Controls {

    /// <summary>
    /// A numeric input control for floating-point values with spinner buttons and scroll wheel support.
    /// </summary>
    public class FloatInput : NumberInputBase {

        private float _minValue = float.MinValue;

        private float _maxValue = float.MaxValue;

        private float _incrementAmount = 0.1f;

        private float _originalValue;

        /// <summary>
        /// Occurs when the <see cref="Value"/> property changes.
        /// </summary>
        public event EventHandler<EventArgs>? ValueChanged;

        /// <summary>
        /// Gets or sets the current floating-point value.
        /// The value is clamped between <see cref="MinValue"/> and <see cref="MaxValue"/>.
        /// </summary>
        public float Value {
            get => float.TryParse(Text, out float value) ? value : 0f;
            set {
                if (value < MinValue) {
                    value = MinValue;
                } else if (value > MaxValue) {
                    value = MaxValue;
                }

                string text = value.ToString(_formatString, NumberFormatInfo.CurrentInfo);
                if (!string.Equals(Text, text, StringComparison.Ordinal)) {
                    Text = text;
                    Invalidate();
                    OnValueChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum allowed value.
        /// If the current <see cref="Value"/> is less than this, it will be clamped.
        /// </summary>
        public float MinValue {
            get => _minValue;
            set {
                _minValue = value;
                if (Value < value) {
                    Value = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum allowed value.
        /// If the current <see cref="Value"/> is greater than this, it will be clamped.
        /// </summary>
        public float MaxValue {
            get => _maxValue;
            set {
                _maxValue = value;
                if (Value > value) {
                    Value = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the increment/decrement amount for spinner buttons and scroll wheel.
        /// </summary>
        public float IncrementAmount {
            get => _incrementAmount;
            set => SetProperty(ref _incrementAmount, value);
        }

        /// <inheritdoc />
        protected override void IncrementValue() {
            Value += _incrementAmount;
        }

        /// <inheritdoc />
        protected override void DecrementValue() {
            Value -= _incrementAmount;
        }

        /// <inheritdoc />
        protected override void StoreOriginalValue() {
            _originalValue = Value;
        }

        /// <inheritdoc />
        protected override void ApplyTextAsValue() {
            if (float.TryParse(_text, out float value)) {
                Value = value;
                Invalidate();
                OnValueChanged();
            } else {
                Value = _originalValue;
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override void DisposeControl() {
            ValueChanged = null;
            base.DisposeControl();
        }
    }
}
