namespace Blish_HUD.GameIntegration.GfxSettings {
    public readonly struct EnvironmentSetting {

        private const string SETTING_LOW     = "low";
        private const string SETTING_MEDIUM  = "medium";
        private const string SETTING_HIGH    = "high";

        private string Value { get; }

        private EnvironmentSetting(string value) {
            this.Value = string.Intern(value);
        }

        public static EnvironmentSetting? FromString(string value) {
            return value switch {
                SETTING_LOW => Low,
                SETTING_MEDIUM => Medium,
                SETTING_HIGH => High,
                _ => new EnvironmentSetting(value)
            };
        }

        public override int  GetHashCode()      => this.Value.GetHashCode();
        public override bool Equals(object obj) => obj != null && obj.GetHashCode() == GetHashCode();

        public static implicit operator string(EnvironmentSetting environmentSetting) => environmentSetting.Value;
        public static implicit operator EnvironmentSetting(string value)            => new EnvironmentSetting(value);

        public static EnvironmentSetting Low    { get; } = new EnvironmentSetting(SETTING_LOW);
        public static EnvironmentSetting Medium { get; } = new EnvironmentSetting(SETTING_MEDIUM);
        public static EnvironmentSetting High   { get; } = new EnvironmentSetting(SETTING_HIGH);

    }
}