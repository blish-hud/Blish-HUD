using System;

namespace Blish_HUD.Settings {
    public interface INumericRangeComplianceRequisite<T> : IComplianceRequisite
        where T : IComparable<T> {

        public T MinValue { get; }
        public T MaxValue { get; }
    }
}
