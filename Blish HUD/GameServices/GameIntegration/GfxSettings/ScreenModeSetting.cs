namespace Blish_HUD.GameIntegration.GfxSettings {
    public readonly struct ScreenModeSetting {

        private const string SETTING_WINDOWED           = "windowed";
        private const string SETTING_FULLSCREEN         = "fullscreen";
        private const string SETTING_WINDOWEDFULLSCREEN = "windowed_fullscreen";

        private string Value { get; }

        private ScreenModeSetting(string value) {
            this.Value = string.Intern(value);
        }

        public static ScreenModeSetting? FromString(string value) {
            return value switch {
                SETTING_WINDOWED => Windowed,
                SETTING_FULLSCREEN => Fullscreen,
                SETTING_WINDOWEDFULLSCREEN => WindowedFullscreen,
                _ => new ScreenModeSetting(value)
            };
        }

        public override int  GetHashCode()      => this.Value.GetHashCode();
        public override bool Equals(object obj) => obj != null && obj.GetHashCode() == GetHashCode();

        public static implicit operator string(ScreenModeSetting screenModeSetting) => screenModeSetting.Value;
        public static implicit operator ScreenModeSetting(string value)             => new ScreenModeSetting(value);

        public static ScreenModeSetting Windowed           { get; } = new ScreenModeSetting(SETTING_WINDOWED);
        public static ScreenModeSetting Fullscreen         { get; } = new ScreenModeSetting(SETTING_FULLSCREEN);
        public static ScreenModeSetting WindowedFullscreen { get; } = new ScreenModeSetting(SETTING_WINDOWEDFULLSCREEN);

    }
}