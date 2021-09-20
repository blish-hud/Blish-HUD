namespace Blish_HUD.GameIntegration.GfxSettings {
    public readonly struct ReflectionsSetting {

        private const string SETTING_NONE    = "none";
        private const string SETTING_TERRAIN = "terrain";
        private const string SETTING_ALL     = "all";

        private string Value { get; }

        private ReflectionsSetting(string value) {
            this.Value = string.Intern(value);
        }

        public static ReflectionsSetting? FromString(string value) {
            return value switch {
                SETTING_NONE => None,
                SETTING_TERRAIN => Terrain,
                SETTING_ALL => All,
                _ => new ReflectionsSetting(value)
            };
        }

        public override int GetHashCode() => this.Value.GetHashCode();

        public static implicit operator string(ReflectionsSetting reflectionsSetting) => reflectionsSetting.Value;
        public static implicit operator ReflectionsSetting(string value)              => new ReflectionsSetting(value);

        public static ReflectionsSetting None    { get; } = new ReflectionsSetting(SETTING_NONE);
        public static ReflectionsSetting Terrain { get; } = new ReflectionsSetting(SETTING_TERRAIN);
        public static ReflectionsSetting All     { get; } = new ReflectionsSetting(SETTING_ALL);

    }
}