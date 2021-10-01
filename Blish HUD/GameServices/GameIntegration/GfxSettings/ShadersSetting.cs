namespace Blish_HUD.GameIntegration.GfxSettings {
    public readonly struct ShadersSetting {

        private const string SETTING_LOW     = "low";
        private const string SETTING_MEDIUM  = "medium";
        private const string SETTING_HIGH    = "high";

        private string Value { get; }

        private ShadersSetting(string value) {
            this.Value = string.Intern(value);
        }

        public static ShadersSetting? FromString(string value) {
            return value switch {
                SETTING_LOW => Low,
                SETTING_MEDIUM => Medium,
                SETTING_HIGH => High,
                _ => new ShadersSetting(value)
            };
        }

        public override int  GetHashCode()      => this.Value.GetHashCode();
        public override bool Equals(object obj) => obj != null && obj.GetHashCode() == GetHashCode();

        public static implicit operator string(ShadersSetting shadersSetting) => shadersSetting.Value;
        public static implicit operator ShadersSetting(string value)          => new ShadersSetting(value);

        public static ShadersSetting Low    { get; } = new ShadersSetting(SETTING_LOW);
        public static ShadersSetting Medium { get; } = new ShadersSetting(SETTING_MEDIUM);
        public static ShadersSetting High   { get; } = new ShadersSetting(SETTING_HIGH);

    }
}