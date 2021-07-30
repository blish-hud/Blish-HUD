using System;

namespace Blish_HUD.Settings {
    public readonly struct EnumInclusionComplianceRequisite<T> : IComplianceRequisite
        where T : Enum {

        public T[] IncludedValues { get; }

        public EnumInclusionComplianceRequisite(params T[] includedValues) {
            this.IncludedValues = includedValues;
        }

    }
}
