using System;
using System.Collections.Generic;
using System.Linq;

namespace Blish_HUD.Settings {
    public static class SettingComplianceExtensions {

        private static readonly Dictionary<ISettingEntry, Dictionary<Type, IComplianceRequisite>> _complianceRequisites = new Dictionary<ISettingEntry, Dictionary<Type, IComplianceRequisite>>();

        /// <summary>
        /// Returns the <see cref="IComplianceRequisite"/>s associated with a setting, if any have been specified.
        /// </summary>
        public static IEnumerable<IComplianceRequisite> GetComplianceRequisite<T>(this ISettingEntry<T> setting) {
            return _complianceRequisites.ContainsKey(setting)
                       ? _complianceRequisites[setting].Values.ToList()
                       : Enumerable.Empty<IComplianceRequisite>();
        }

        private static void SetComplianceRequisite<TEntry, TCompliance>(ISettingEntry<TEntry> setting, TCompliance complianceRequisite)
            where TCompliance : IComplianceRequisite {

            if (!_complianceRequisites.ContainsKey(setting)) {
                _complianceRequisites[setting] = new Dictionary<Type, IComplianceRequisite>(2);
            }

            _complianceRequisites[setting][typeof(TCompliance)] = complianceRequisite;
        }

        #region GENERAL COMPLIANCE

        private const bool DEFAULT_DISABLED = true;

        /// <summary>
        /// Sets the setting to be disabled or enabled in the UI.
        /// </summary>
        public static void SetDisabled<T>(this ISettingEntry<T> setting, bool disabled = DEFAULT_DISABLED) {
            SetComplianceRequisite(setting, new SettingDisabledComplianceRequisite(disabled));
        }

        #endregion

        #region INT COMPLIANCE

        private const int DEFAULT_MININT = 0;
        private const int DEFAULT_MAXINT = 100;

        /// <summary>
        /// Sets the minimum and maximum <c>int</c> value a user can set the setting to from the UI.
        /// </summary>
        public static void SetRange(this ISettingEntry<int> setting, int minValue = DEFAULT_MININT, int maxValue = DEFAULT_MAXINT) {
            SetComplianceRequisite(setting, new IntRangeRangeComplianceRequisite(minValue, maxValue));
        }

        #endregion

        #region FLOAT COMPLIANCE

        private const float DEFAULT_MINFLOAT = 0f;
        private const float DEFAULT_MAXFLOAT = 100f;

        /// <summary>
        /// Sets the minimum and maximum <c>float</c> value a user can set the setting to from the UI.
        /// </summary>
        public static void SetRange(this ISettingEntry<float> setting, float minValue = DEFAULT_MINFLOAT, float maxValue = DEFAULT_MAXFLOAT) {
            SetComplianceRequisite(setting, new FloatRangeRangeComplianceRequisite(minValue, maxValue));
        }

        #endregion

        #region ENUM COMPLIANCE

        /// <summary>
        /// Limits the enum values a user can set the setting to in the UI to just the provided values.
        /// </summary>
        public static void SetIncluded<T>(this ISettingEntry<T> setting, params T[] included) where T : Enum {
            SetComplianceRequisite(setting, new EnumInclusionComplianceRequisite<T>(included));
        }

        /// <summary>
        /// Limits the enum values a user can set the setting to in the UI to anything except for the provided values.
        /// </summary>
        public static void SetExcluded<T>(this ISettingEntry<T> setting, params T[] excluded) where T : Enum {
            T[] values = EnumUtil.GetCachedValues<T>();

            SetComplianceRequisite(setting, new EnumInclusionComplianceRequisite<T>(values.Except(excluded).ToArray()));
        }

        #endregion

    }
}
