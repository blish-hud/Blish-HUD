using System;
using Blish_HUD.Controls;
using Newtonsoft.Json;

namespace Blish_HUD.Settings {

    public sealed class SettingEntry<T> : SettingEntry {

        public event EventHandler<ValueChangedEventArgs<T>> SettingChanged;

        private void OnSettingChanged(ValueChangedEventArgs<T> e) {
            GameService.Settings.SettingSave();

            OnPropertyChanged(nameof(this.Value));

            this.SettingChanged?.Invoke(this, e);
        }

        private T _value;

        [JsonProperty, JsonRequired]
        public T Value {
            get => _value;
            set {
                if (object.Equals(_value, value)) return;

                var prevValue = this.Value;
                _value = value;

                OnSettingChanged(new ValueChangedEventArgs<T>(prevValue, _value));
            }
        }

        protected override Type GetSettingType() {
            return typeof(T);
        }

        protected override object GetSettingValue() {
            return _value;
        }

        public SettingEntry() { /* NOOP */ }

        /// <summary>
        /// Creates a new <see cref="SettingEntry"/> of type <see cref="T"/>.
        /// </summary>
        /// <param name="value">The default value for the <see cref="SettingEntry{T}"/> if a value has not yet been saved in the settings.</param>
        protected SettingEntry(T value) {
            _value = value;
        }

        public static SettingEntry<T> InitSetting(T value) {
            var newSetting = new SettingEntry<T>(value);

            return newSetting;
        }

        public static SettingEntry<T> InitSetting(string entryKey, T value) {
            var newSetting = new SettingEntry<T>(value) {
                EntryKey = entryKey,

                _value = value,
            };

            return newSetting;
        }

    }

}
