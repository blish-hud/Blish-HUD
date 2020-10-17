using System;

namespace Blish_HUD.Settings {
    public struct EnumComplianceRequisite<T> : IComplianceRequisite
        where T : Enum {

        public T[] IncludedValues { get; }

        public EnumComplianceRequisite(params T[] includedValues) {
            this.IncludedValues = includedValues;
        }

    }
}
