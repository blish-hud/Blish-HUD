namespace Blish_HUD.GameIntegration.GfxSettings {
    public readonly struct ShadowsSetting {

        private const string SETTING_OFF    = "off";
        private const string SETTING_LOW    = "low";
        private const string SETTING_MEDIUM = "medium";
        private const string SETTING_HIGH   = "high";
        private const string SETTING_ULTRA  = "ultra";

        private string Value { get; }

        private ShadowsSetting(string value) {
            this.Value = string.Intern(value);
        }

        public static ShadowsSetting? FromString(string value) {
            return value switch {
                SETTING_OFF => Off,
                SETTING_LOW => Low,
                SETTING_MEDIUM => Medium,
                SETTING_HIGH => High,
                SETTING_ULTRA => Ultra,
                _ => new ShadowsSetting(value)
            };
        }

        public override int  GetHashCode()      => this.Value.GetHashCode();
        public override bool Equals(object obj) => obj != null && obj.GetHashCode() == GetHashCode();

        public static implicit operator string(ShadowsSetting shadowsSetting) => shadowsSetting.Value;
        public static implicit operator ShadowsSetting(string value)          => new ShadowsSetting(value);

        public static ShadowsSetting Off    { get; } = new ShadowsSetting(SETTING_OFF);
        public static ShadowsSetting Low    { get; } = new ShadowsSetting(SETTING_LOW);
        public static ShadowsSetting Medium { get; } = new ShadowsSetting(SETTING_MEDIUM);
        public static ShadowsSetting High   { get; } = new ShadowsSetting(SETTING_HIGH);
        public static ShadowsSetting Ultra  { get; } = new ShadowsSetting(SETTING_ULTRA);

    }
}