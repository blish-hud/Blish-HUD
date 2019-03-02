using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Modules.TacO.Origin {
    /// <summary>
    /// Contains utility calls that aren't actually present in the TacO source.
    /// These calls are generally used to help with the transition from the original code
    /// to this module.
    /// </summary>
    public static class Util {

        /// <summary>
        /// Matches BoyC's implementation - sourced from <a href="https://stackoverflow.com/q/1032376/595437">here</a>.
        /// </summary>
        public static string GuidToBase64(Guid guid) {
            return Convert.ToBase64String(guid.ToByteArray()).Replace("/", "-").Replace("+", "_").Replace("=", "");
        }

        /// <summary>
        /// Matches BoyC's implementation - sourced from <a href="https://stackoverflow.com/q/1032376/595437">here</a>.
        /// </summary>
        public static Guid Base64ToGuid(string base64) {
            var guid = default(Guid);
            base64 = base64.Replace("-", "/").Replace("_", "+") + "==";

            try {
                guid = new Guid(Convert.FromBase64String(base64));
            } catch (Exception ex) {
                throw new Exception("Bad base64 conversion to GUID", ex);
            }

            return guid;
        }

    }
}
