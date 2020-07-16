using System;

namespace Blish_HUD.Settings {
    public interface INumericComplianceRequisite<T> : IComplianceRequisite
        where T : IComparable<T> {

        T MinValue { get; }
        T MaxValue { get; }
    }
}
