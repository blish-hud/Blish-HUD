namespace Blish_HUD.Settings {
    public readonly struct FloatRangeRangeComplianceRequisite : INumericRangeComplianceRequisite<float> {
        
        public float MinValue { get; }
        public float MaxValue { get; }

        public FloatRangeRangeComplianceRequisite(float minValue, float maxValue) {
            this.MinValue = minValue;
            this.MaxValue = maxValue;
        }

    }
}
