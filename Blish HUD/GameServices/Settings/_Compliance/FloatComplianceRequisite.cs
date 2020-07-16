namespace Blish_HUD.Settings {
    public struct FloatComplianceRequisite : INumericComplianceRequisite<float> {
        
        public float MinValue { get; }
        public float MaxValue { get; }

        public FloatComplianceRequisite(float minValue, float maxValue) {
            this.MinValue = minValue;
            this.MaxValue = maxValue;
        }

    }
}
