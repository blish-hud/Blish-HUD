namespace Blish_HUD.Settings {
    public struct IntComplianceRequisite : INumericComplianceRequisite<int> {

        public int MinValue { get; }
        public int MaxValue { get; }

        public IntComplianceRequisite(int minValue, int maxValue) {
            this.MinValue = minValue;
            this.MaxValue = maxValue;
        }

    }
}
