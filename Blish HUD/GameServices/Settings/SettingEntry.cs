using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Blish_HUD.Settings {

    internal class SettingEntry<T> : ISettingEntry<T> {

        private T value;
     
        public event EventHandler SettingUpdated;
        public event EventHandler<ValueChangedEventArgs<T>> SettingChanged;


        [JsonPropertyName("Key")]
        public string EntryKey { get; set; }

        [JsonPropertyName("Value")]
        public T Value {
            get => value;
            set {
                if (Equals(this.value, value)) return;

                var prevValue = this.value;
                this.value = value;

                AttachSettingChangedListener(value);
                DetachSettingChangedListener(prevValue);

                OnSettingChanged(new ValueChangedEventArgs<T>(prevValue, value));
            }
        }


        private void AttachSettingChangedListener(T value) {
            if (value is INotifyPropertyChanged valueWithNotifyPropertyChanged) {
                valueWithNotifyPropertyChanged.PropertyChanged += Value_PropertyChanged;
            }
            if (value is INotifyCollectionChanged valueWithNotifyCollectionChanged) {
                valueWithNotifyCollectionChanged.CollectionChanged += Value_CollectionChanged;
            }
        }

        private void DetachSettingChangedListener(T value) {
            if (value == null) {
                return;
            }

            if (value is INotifyPropertyChanged valueWithNotifyPropertyChanged) {
                valueWithNotifyPropertyChanged.PropertyChanged -= Value_PropertyChanged;
            }
            if (value is INotifyCollectionChanged valueWithNotifyCollectionChanged) {
                valueWithNotifyCollectionChanged.CollectionChanged -= Value_CollectionChanged;
            }
        }


        private void Value_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            OnSettingChanged(new ValueChangedEventArgs<T>(value, value));
        }

        private void Value_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            OnSettingChanged(new ValueChangedEventArgs<T>(value, value));
        }

        private void OnSettingChanged(ValueChangedEventArgs<T> e) {
            OnPropertyChanged(nameof(Value));
            this.SettingChanged?.Invoke(this, e);
            this.SettingUpdated?.Invoke(this, new EventArgs());
        }


        #region Property Binding

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

}
