namespace Blish_HUD.GameIntegration.GfxSettings {
    public readonly struct CharModelQualitySetting {

        private const string SETTING_LOWEST  = "lowest";
        private const string SETTING_LOW     = "low";
        private const string SETTING_MEDIUM  = "medium";
        private const string SETTING_HIGH    = "high";
        private const string SETTING_HIGHEST = "highest";

        private string Value { get; }

        private CharModelQualitySetting(string value) {
            this.Value = string.Intern(value);
        }

        public static CharModelQualitySetting? FromString(string value) {
            return value switch {
                SETTING_LOWEST => Lowest,
                SETTING_LOW => Low,
                SETTING_MEDIUM => Medium,
                SETTING_HIGH => High,
                SETTING_HIGHEST => Highest,
                _ => new CharModelQualitySetting(value)
            };
        }

        public override int  GetHashCode()      => this.Value.GetHashCode();
        public override bool Equals(object obj) => obj != null && obj.GetHashCode() == GetHashCode();

        public static implicit operator string(CharModelQualitySetting charModelQualitySetting) => charModelQualitySetting.Value;
        public static implicit operator CharModelQualitySetting(string value)                   => new CharModelQualitySetting(value);

        public static CharModelQualitySetting Lowest  { get; } = new CharModelQualitySetting(SETTING_LOWEST);
        public static CharModelQualitySetting Low     { get; } = new CharModelQualitySetting(SETTING_LOW);
        public static CharModelQualitySetting Medium  { get; } = new CharModelQualitySetting(SETTING_MEDIUM);
        public static CharModelQualitySetting High    { get; } = new CharModelQualitySetting(SETTING_HIGH);
        public static CharModelQualitySetting Highest { get; } = new CharModelQualitySetting(SETTING_HIGHEST);

    }
}