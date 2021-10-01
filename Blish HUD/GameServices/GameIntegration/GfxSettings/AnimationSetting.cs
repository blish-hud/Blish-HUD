namespace Blish_HUD.GameIntegration.GfxSettings {
    public readonly struct AnimationSetting {

        private const string SETTING_LOW     = "low";
        private const string SETTING_MEDIUM  = "medium";
        private const string SETTING_HIGH    = "high";

        private string Value { get; }

        private AnimationSetting(string value) {
            this.Value = string.Intern(value);
        }

        public static AnimationSetting? FromString(string value) {
            return value switch {
                SETTING_LOW => Low,
                SETTING_MEDIUM => Medium,
                SETTING_HIGH => High,
                _ => new AnimationSetting(value)
            };
        }

        public override int  GetHashCode()      => this.Value.GetHashCode();
        public override bool Equals(object obj) => obj != null && obj.GetHashCode() == GetHashCode();

        public static implicit operator string(AnimationSetting animationSetting) => animationSetting.Value;
        public static implicit operator AnimationSetting(string value)            => new AnimationSetting(value);

        public static AnimationSetting Low    { get; } = new AnimationSetting(SETTING_LOW);
        public static AnimationSetting Medium { get; } = new AnimationSetting(SETTING_MEDIUM);
        public static AnimationSetting High   { get; } = new AnimationSetting(SETTING_HIGH);

    }
}