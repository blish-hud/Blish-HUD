namespace Blish_HUD.GameIntegration.GfxSettings {
    public readonly struct TextureDetailSetting {

        private const string SETTING_LOW     = "low";
        private const string SETTING_MEDIUM  = "medium";
        private const string SETTING_HIGH    = "high";

        private string Value { get; }

        private TextureDetailSetting(string value) {
            this.Value = string.Intern(value);
        }

        public static TextureDetailSetting? FromString(string value) {
            return value switch {
                SETTING_LOW => Low,
                SETTING_MEDIUM => Medium,
                SETTING_HIGH => High,
                _ => new TextureDetailSetting(value)
            };
        }

        public override int  GetHashCode()      => this.Value.GetHashCode();
        public override bool Equals(object obj) => obj != null && obj.GetHashCode() == GetHashCode();

        public static implicit operator string(TextureDetailSetting textureDetailSetting) => textureDetailSetting.Value;
        public static implicit operator TextureDetailSetting(string value)                => new TextureDetailSetting(value);

        public static TextureDetailSetting Low     { get; } = new TextureDetailSetting(SETTING_LOW);
        public static TextureDetailSetting Medium  { get; } = new TextureDetailSetting(SETTING_MEDIUM);
        public static TextureDetailSetting High    { get; } = new TextureDetailSetting(SETTING_HIGH);

    }
}