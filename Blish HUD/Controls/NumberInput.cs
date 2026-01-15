using System;
using System.Globalization;

namespace Blish_HUD.Controls {

    public class NumberInput : NumberInputBase {

        private int _minValue = int.MinValue;

        private int _maxValue = int.MaxValue;

        private int _originalValue;

        public event EventHandler<EventArgs>? ValueChanged;

        public int Value {
            get => int.TryParse(Text, out int value) ? value : 0;
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

        public int MinValue {
            get => _minValue;
            set {
                _minValue = value;
                if (Value < value) {
                    Value = value;
                }
            }
        }

        public int MaxValue {
            get => _maxValue;
            set {
                _maxValue = value;
                if (Value > value) {
                    Value = value;
                }
            }
        }

        protected override void IncrementValue() {
            Value++;
        }

        protected override void DecrementValue() {
            Value--;
        }

        protected override void StoreOriginalValue() {
            _originalValue = Value;
        }

        protected override void ApplyTextAsValue() {
            if (int.TryParse(_text, out int value)) {
                Value = value;
                Invalidate();
                OnValueChanged();
            } else {
                Value = _originalValue;
            }
        }

        protected override void HandleMouseWheelScrolled(int scrollDelta) {
            if (scrollDelta > 0) {
                Value++;
            } else {
                Value--;
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
