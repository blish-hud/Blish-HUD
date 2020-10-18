using System;
using System.Collections.Generic;
using System.Linq;

namespace Blish_HUD.Settings {
    public static class SettingComplianceExtensions {

        private static readonly Dictionary<SettingEntry, IComplianceRequisite> _complianceRequisites = new Dictionary<SettingEntry, IComplianceRequisite>();

        /// <summary>
        /// Returns the <see cref="IComplianceRequisite"/> associated with a setting, if one has been specified.
        /// </summary>
        public static IComplianceRequisite GetComplianceRequisite(this SettingEntry setting) {
            return _complianceRequisites.ContainsKey(setting)
                       ? _complianceRequisites[setting]
                       : null;
        }

        #region INT COMPLIANCE

        private const int DEFAULT_MININT = 0;
        private const int DEFAULT_MAXINT = 100;

        /// <summary>
        /// Sets the minimum and maximum <c>int</c> value a user can set the setting to from the UI.
        /// </summary>
        public static void SetRange(this SettingEntry<int> setting, int minValue = DEFAULT_MININT, int maxValue = DEFAULT_MAXINT) {
            _complianceRequisites[setting] = new IntComplianceRequisite(minValue, maxValue);
        }

        #endregion

        #region FLOAT COMPLIANCE

        private const float DEFAULT_MINFLOAT = 0f;
        private const float DEFAULT_MAXFLOAT = 100f;

        /// <summary>
        /// Sets the minimum and maximum <c>float</c> value a user can set the setting to from the UI.
        /// </summary>
        public static void SetRange(this SettingEntry<float> setting, float minValue = DEFAULT_MINFLOAT, float maxValue = DEFAULT_MAXFLOAT) {
            _complianceRequisites[setting] = new FloatComplianceRequisite(minValue, maxValue);
        }

        #endregion

        #region ENUM COMPLIANCE

        /// <summary>
        /// Limits the enum values a user can set the setting to in the UI to just the provided values.
        /// </summary>
        public static void SetIncluded<T>(this SettingEntry<T> setting, params T[] included) where T : Enum {
            _complianceRequisites[setting] = new EnumComplianceRequisite<T>(included);
        }

        /// <summary>
        /// Limits the enum values a user can set the setting to in the UI to anything except for the provided values.
        /// </summary>
        public static void SetExcluded<T>(this SettingEntry<T> setting, params T[] excluded) where T : Enum {
            T[] values = EnumUtil.GetCachedValues<T>();

            _complianceRequisites[setting] = new EnumComplianceRequisite<T>(values.Except(excluded).ToArray());
        }

        #endregion

    }
}
