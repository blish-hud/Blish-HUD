namespace Blish_HUD.GameIntegration.GfxSettings {
    public readonly struct CharModelLimitSetting {

        private const string SETTING_LOWEST  = "lowest";
        private const string SETTING_LOW     = "low";
        private const string SETTING_MEDIUM  = "medium";
        private const string SETTING_HIGH    = "high";
        private const string SETTING_HIGHEST = "highest";

        private string Value { get; }

        private CharModelLimitSetting(string value) {
            this.Value = string.Intern(value);
        }

        public static CharModelLimitSetting? FromString(string value) {
            return value switch {
                SETTING_LOWEST => Lowest,
                SETTING_LOW => Low,
                SETTING_MEDIUM => Medium,
                SETTING_HIGH => High,
                SETTING_HIGHEST => Highest,
                _ => new CharModelLimitSetting(value)
            };
        }

        public override int GetHashCode() => this.Value.GetHashCode();

        public static implicit operator string(CharModelLimitSetting charModelLimitSetting) => charModelLimitSetting.Value;
        public static implicit operator CharModelLimitSetting(string value)                 => new CharModelLimitSetting(value);

        public static CharModelLimitSetting Lowest  { get; } = new CharModelLimitSetting(SETTING_LOWEST);
        public static CharModelLimitSetting Low     { get; } = new CharModelLimitSetting(SETTING_LOW);
        public static CharModelLimitSetting Medium  { get; } = new CharModelLimitSetting(SETTING_MEDIUM);
        public static CharModelLimitSetting High    { get; } = new CharModelLimitSetting(SETTING_HIGH);
        public static CharModelLimitSetting Highest { get; } = new CharModelLimitSetting(SETTING_HIGHEST);

    }
}