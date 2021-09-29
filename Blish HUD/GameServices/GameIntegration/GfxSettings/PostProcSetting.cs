namespace Blish_HUD.GameIntegration.GfxSettings {
    public readonly struct PostProcSetting {

        private const string SETTING_LOW     = "low";
        private const string SETTING_MEDIUM  = "medium";
        private const string SETTING_HIGH    = "high";

        private string Value { get; }

        private PostProcSetting(string value) {
            this.Value = string.Intern(value);
        }

        public static PostProcSetting? FromString(string value) {
            return value switch {
                SETTING_LOW => Low,
                SETTING_MEDIUM => Medium,
                SETTING_HIGH => High,
                _ => new PostProcSetting(value)
            };
        }

        public override int  GetHashCode()      => this.Value.GetHashCode();
        public override bool Equals(object obj) => obj != null && obj.GetHashCode() == GetHashCode();

        public static implicit operator string(PostProcSetting postProcSetting) => postProcSetting.Value;
        public static implicit operator PostProcSetting(string value)           => new PostProcSetting(value);

        public static PostProcSetting Low    { get; } = new PostProcSetting(SETTING_LOW);
        public static PostProcSetting Medium { get; } = new PostProcSetting(SETTING_MEDIUM);
        public static PostProcSetting High   { get; } = new PostProcSetting(SETTING_HIGH);

    }
}