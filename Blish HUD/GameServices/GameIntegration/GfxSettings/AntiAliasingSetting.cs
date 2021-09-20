// ReSharper disable InconsistentNaming
namespace Blish_HUD.GameIntegration.GfxSettings {
    public readonly struct AntiAliasingSetting {

        private const string SETTING_NONE     = "none";
        private const string SETTING_FXAA     = "fxaa";
        private const string SETTING_SMAALOW  = "smaa_low";
        private const string SETTING_SMAAHIGH = "smaa_high";

        private string Value { get; }

        private AntiAliasingSetting(string value) {
            this.Value = string.Intern(value);
        }

        public static AntiAliasingSetting? FromString(string value) {
            return value switch {
                SETTING_NONE => None,
                SETTING_FXAA => FXAA,
                SETTING_SMAALOW => SMAALow,
                SETTING_SMAAHIGH => SMAAHigh,
                _ => new AntiAliasingSetting(value)
            };
        }

        public override int GetHashCode() => this.Value.GetHashCode();

        public static implicit operator string(AntiAliasingSetting antiAliasingSetting) => antiAliasingSetting.Value;
        public static implicit operator AntiAliasingSetting(string value)               => new AntiAliasingSetting(value);

        public static AntiAliasingSetting None     { get; } = new AntiAliasingSetting(SETTING_NONE);
        public static AntiAliasingSetting FXAA     { get; } = new AntiAliasingSetting(SETTING_FXAA);
        public static AntiAliasingSetting SMAALow  { get; } = new AntiAliasingSetting(SETTING_SMAALOW);
        public static AntiAliasingSetting SMAAHigh { get; } = new AntiAliasingSetting(SETTING_SMAAHIGH);

    }
}