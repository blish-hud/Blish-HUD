using System;
using System.Collections.Generic;
using System.Linq;

namespace Blish_HUD.Settings {
    public static class SettingComplianceExtensions {

        private static readonly Dictionary<SettingEntry, Dictionary<Type, IComplianceRequisite>> _complianceRequisites = new Dictionary<SettingEntry, Dictionary<Type, IComplianceRequisite>>();

        /// <summary>
        /// Returns the <see cref="IComplianceRequisite"/>s associated with a setting, if any have been specified.
        /// </summary>
        public static IEnumerable<IComplianceRequisite> GetComplianceRequisite(this SettingEntry setting) {
            return _complianceRequisites.ContainsKey(setting)
                       ? _complianceRequisites[setting].Values.ToList()
                       : Enumerable.Empty<IComplianceRequisite>();
        }

        private static void SetComplianceRequisite<T>(SettingEntry setting, T complianceRequisite)
            where T : IComplianceRequisite {

            if (!_complianceRequisites.ContainsKey(setting)) {
                _complianceRequisites[setting] = new Dictionary<Type, IComplianceRequisite>(2);
            }

            _complianceRequisites[setting][typeof(T)] = complianceRequisite;
        }

        #region GENERAL COMPLIANCE

        private const bool DEFAULT_DISABLED = true;

        /// <summary>
        /// Sets the setting to be disabled or enabled in the UI.
        /// </summary>
        public static void SetDisabled(this SettingEntry setting, bool disabled = DEFAULT_DISABLED) {
            SetComplianceRequisite(setting, new SettingDisabledComplianceRequisite(disabled));
        }

        /// <summary>
        /// Sets the validation function used to indicate if the value is valid for the setting when changed via the UI.
        /// </summary>
        public static void SetValidation<T>(this SettingEntry<T> setting, Func<T, SettingValidationResult> validationFunc) {
            SetComplianceRequisite(setting, new SettingValidationComplianceRequisite<T>(validationFunc));
        }

        #endregion

        #region INT COMPLIANCE

        private const int DEFAULT_MININT = 0;
        private const int DEFAULT_MAXINT = 100;

        /// <summary>
        /// Sets the minimum and maximum <c>int</c> value a user can set the setting to from the UI.
        /// </summary>
        public static void SetRange(this SettingEntry<int> setting, int minValue = DEFAULT_MININT, int maxValue = DEFAULT_MAXINT) {
            SetComplianceRequisite(setting, new IntRangeRangeComplianceRequisite(minValue, maxValue));
        }

        #endregion

        #region FLOAT COMPLIANCE

        private const float DEFAULT_MINFLOAT = 0f;
        private const float DEFAULT_MAXFLOAT = 100f;

        /// <summary>
        /// Sets the minimum and maximum <c>float</c> value a user can set the setting to from the UI.
        /// </summary>
        public static void SetRange(this SettingEntry<float> setting, float minValue = DEFAULT_MINFLOAT, float maxValue = DEFAULT_MAXFLOAT) {
            SetComplianceRequisite(setting, new FloatRangeRangeComplianceRequisite(minValue, maxValue));
        }

        #endregion

        #region ENUM COMPLIANCE

        /// <summary>
        /// Limits the enum values a user can set the setting to in the UI to just the provided values.
        /// </summary>
        public static void SetIncluded<T>(this SettingEntry<T> setting, params T[] included) where T : Enum {
            SetComplianceRequisite(setting, new EnumInclusionComplianceRequisite<T>(included));
        }

        /// <summary>
        /// Limits the enum values a user can set the setting to in the UI to anything except for the provided values.
        /// </summary>
        public static void SetExcluded<T>(this SettingEntry<T> setting, params T[] excluded) where T : Enum {
            T[] values = EnumUtil.GetCachedValues<T>();

            SetComplianceRequisite(setting, new EnumInclusionComplianceRequisite<T>(values.Except(excluded).ToArray()));
        }

        #endregion

    }
}
