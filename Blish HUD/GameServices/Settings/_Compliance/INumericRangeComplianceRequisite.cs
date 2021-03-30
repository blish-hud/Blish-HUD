using System;

namespace Blish_HUD.Settings {
    public interface INumericRangeComplianceRequisite<T> : IComplianceRequisite
        where T : IComparable<T> {

        T MinValue { get; }
        T MaxValue { get; }
    }
}
