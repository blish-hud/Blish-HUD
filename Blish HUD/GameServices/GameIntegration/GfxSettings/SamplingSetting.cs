namespace Blish_HUD.GameIntegration.GfxSettings {
    public readonly struct SamplingSetting {

        private const string SETTING_SUBSAMPLE   = "subsample";
        private const string SETTING_NATIVE      = "native";
        private const string SETTING_SUPERSAMPLE = "supersample";

        private string Value { get; }

        private SamplingSetting(string value) {
            this.Value = string.Intern(value);
        }

        public static SamplingSetting? FromString(string value) {
            return value switch {
                SETTING_SUBSAMPLE => Subsample,
                SETTING_NATIVE => Native,
                SETTING_SUPERSAMPLE => Supersample,
                _ => new SamplingSetting(value)
            };
        }

        public override int  GetHashCode()      => this.Value.GetHashCode();
        public override bool Equals(object obj) => obj != null && obj.GetHashCode() == GetHashCode();

        public static implicit operator string(SamplingSetting samplingSetting) => samplingSetting.Value;
        public static implicit operator SamplingSetting(string value)           => new SamplingSetting(value);

        public static SamplingSetting Subsample   { get; } = new SamplingSetting(SETTING_SUBSAMPLE);
        public static SamplingSetting Native      { get; } = new SamplingSetting(SETTING_NATIVE);
        public static SamplingSetting Supersample { get; } = new SamplingSetting(SETTING_SUPERSAMPLE);

    }
}