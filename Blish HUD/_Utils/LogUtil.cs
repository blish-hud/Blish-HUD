using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD {
    public static class LogUtil {

        public static void TargetDebug(string time, string level, string logger, string message) {
            System.Diagnostics.Debug.WriteLine($"{time}|{level.ToUpper()}|{logger}|{message}");
        }

    }
}
