namespace Blish_HUD.Settings {
    public readonly struct IntRangeRangeComplianceRequisite : INumericRangeComplianceRequisite<int> {

        public int MinValue { get; }
        public int MaxValue { get; }

        public IntRangeRangeComplianceRequisite(int minValue, int maxValue) {
            this.MinValue = minValue;
            this.MaxValue = maxValue;
        }

    }
}
