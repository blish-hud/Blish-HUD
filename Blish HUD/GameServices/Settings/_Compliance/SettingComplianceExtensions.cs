using System.Collections.Generic;

namespace Blish_HUD.Settings {
    public static class SettingComplianceExtensions {

        private static readonly Dictionary<SettingEntry, IComplianceRequisite> _complianceRequisites = new Dictionary<SettingEntry, IComplianceRequisite>();

        public static IComplianceRequisite GetComplianceRequisite(this SettingEntry setting) {
            return _complianceRequisites.ContainsKey(setting)
                       ? _complianceRequisites[setting]
                       : null;
        }

        #region INT COMPLIANCE

        private const int DEFAULT_MININT = 0;
        private const int DEFAULT_MAXINT = 100;

        public static void SetRange(this SettingEntry<int> setting, int minValue = DEFAULT_MININT, int maxValue = DEFAULT_MAXINT) {
            _complianceRequisites[setting] = new IntComplianceRequisite(minValue, maxValue);
        }

        #endregion

        #region FLOAT COMPLIANCE

        private const float DEFAULT_MINFLOAT = 0f;
        private const float DEFAULT_MAXFLOAT = 100f;
        
        public static void SetRange(this SettingEntry<float> setting, float minValue = DEFAULT_MINFLOAT, float maxValue = DEFAULT_MAXFLOAT) {
            _complianceRequisites[setting] = new FloatComplianceRequisite(minValue, maxValue);
        }

        #endregion

    }
}
