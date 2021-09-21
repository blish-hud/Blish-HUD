namespace Blish_HUD.GameIntegration.GfxSettings {
    public readonly struct FrameLimitSetting {

        private const string SETTING_UNLIMITED = "unlimited";
        private const string SETTING_60        = "60";
        private const string SETTING_30        = "30";

        private string Value { get; }

        private FrameLimitSetting(string value) {
            this.Value = string.Intern(value);
        }

        public static FrameLimitSetting? FromString(string value) {
            return value switch {
                SETTING_UNLIMITED => Unlimited,
                SETTING_60 => Value60,
                SETTING_30 => Value30,
                _ => new FrameLimitSetting(value)
            };
        }

        public override int GetHashCode() => this.Value.GetHashCode();

        public static implicit operator string(FrameLimitSetting frameLimitSetting) => frameLimitSetting.Value;
        public static implicit operator FrameLimitSetting(string value)             => new FrameLimitSetting(value);

        public static FrameLimitSetting Unlimited { get; } = new FrameLimitSetting(SETTING_UNLIMITED);
        public static FrameLimitSetting Value60   { get; } = new FrameLimitSetting(SETTING_60);
        public static FrameLimitSetting Value30   { get; } = new FrameLimitSetting(SETTING_30);

    }
}
