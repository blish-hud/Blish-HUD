using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Blish_HUD.Utils {
    public static class Mouse {

        [DllImport("user32.dll")]
        static extern IntPtr LoadCursorFromFile(string lpFileName);

        [DllImport("user32.dll")]
        static extern IntPtr SetCursor(IntPtr hCursor);

        [DllImport("user32.dll")]
        static extern bool SetSystemCursor(IntPtr hcur, uint id);

        [DllImport("user32.dll")]
        static extern bool DestroyCursor(IntPtr hCursor);

        private const uint OCR_NORMAL = 32512;

        static Cursor ColoredCursor;

        static IntPtr ncuri;

        public static void SetCursor(string cursorFile) {
            var nCur = (Bitmap)Image.FromFile(cursorFile);
            Graphics.FromImage(nCur);
            ncuri = nCur.GetHicon();

            SetSystemCursor(ncuri, OCR_NORMAL);
        }

        public static void RestoreCursor() {
            
        }

    }
}
