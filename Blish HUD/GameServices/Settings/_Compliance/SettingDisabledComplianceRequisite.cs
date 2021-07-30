namespace Blish_HUD.Settings {
    public readonly struct SettingDisabledComplianceRequisite : IComplianceRequisite {

        public bool Disabled { get; }

        public SettingDisabledComplianceRequisite(bool disabled) {
            this.Disabled = disabled;
        }

    }
}
