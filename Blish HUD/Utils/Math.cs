using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Utils {
    public static class Maths {

        public static int Clamp(int value, int lowBound, int highBound) { return System.Math.Max(System.Math.Min(value, highBound), lowBound); }

    }
}
