using System;

namespace Blish_HUD.Settings {

    public readonly struct SettingValidationComplianceRequisite<T> : IComplianceRequisite {

        public Func<T, SettingValidationResult> ValidationFunc { get; }

        public SettingValidationComplianceRequisite(Func<T, SettingValidationResult> validationFunc) {
            this.ValidationFunc = validationFunc;
        }

    }
}
