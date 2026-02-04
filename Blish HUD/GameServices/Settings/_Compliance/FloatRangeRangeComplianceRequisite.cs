using System.Globalization;

namespace Blish_HUD.Settings {
    public readonly struct FloatRangeRangeComplianceRequisite : INumericRangeComplianceRequisite<float> {
        
        public float MinValue { get; }
        public float MaxValue { get; }
        public float IncrementAmount { get; }

        public FloatRangeRangeComplianceRequisite(float minValue, float maxValue, float incrementAmount) {
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.IncrementAmount = incrementAmount;
        }

        public string GetFormatString() {
            string incrementString = IncrementAmount.ToString(CultureInfo.InvariantCulture);
            int decimalIndex = incrementString.IndexOf('.');
            
            if (decimalIndex < 0) {
                return "F0";
            }

            int decimalPlaces = incrementString.Length - decimalIndex - 1;
            return $"F{decimalPlaces}";
        }

    }
}
