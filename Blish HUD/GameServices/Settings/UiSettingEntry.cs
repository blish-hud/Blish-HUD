using System;
using System.Text.Json.Serialization;

namespace Blish_HUD.Settings {

    internal class UiSettingEntry<T> : SettingEntry<T>, IUiSettingEntry<T> {

        [JsonIgnore]
        public Func<string> GetDescriptionFunc { get; set; } =
            () => string.Empty;

        [JsonIgnore]
        public Func<string> GetDisplayNameFunc { get; set; } =
            () => string.Empty;


        [JsonIgnore]
        public string Description =>
            this.GetDescriptionFunc();

        [JsonIgnore]
        public string DisplayName =>
            this.GetDisplayNameFunc();

    }

}
