namespace Blish_HUD.GameIntegration.GfxSettings {
    public readonly struct LodDistanceSetting {

        private const string SETTING_LOW    = "low";
        private const string SETTING_MEDIUM = "medium";
        private const string SETTING_HIGH   = "high";
        private const string SETTING_ULTRA  = "ultra";

        private string Value { get; }

        private LodDistanceSetting(string value) {
            this.Value = string.Intern(value);
        }

        public static LodDistanceSetting? FromString(string value) {
            return value switch {
                SETTING_LOW => Low,
                SETTING_MEDIUM => Medium,
                SETTING_HIGH => High,
                SETTING_ULTRA => Ultra,
                _ => new LodDistanceSetting(value)
            };
        }

        public override int GetHashCode() => this.Value.GetHashCode();

        public static implicit operator string(LodDistanceSetting lodDistanceSetting) => lodDistanceSetting.Value;
        public static implicit operator LodDistanceSetting(string value)              => new LodDistanceSetting(value);

        public static LodDistanceSetting Low    { get; } = new LodDistanceSetting(SETTING_LOW);
        public static LodDistanceSetting Medium { get; } = new LodDistanceSetting(SETTING_MEDIUM);
        public static LodDistanceSetting High   { get; } = new LodDistanceSetting(SETTING_HIGH);
        public static LodDistanceSetting Ultra  { get; } = new LodDistanceSetting(SETTING_ULTRA);

    }
}