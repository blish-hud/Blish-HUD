using System.Runtime.InteropServices;

namespace Blish_HUD.Input.WinApi {
    internal static class CursorExtern {

        [DllImport("user32.dll")]
        private static extern bool GetCursorInfo(ref CursorInfo pci);

        internal static CursorInfo GetCursorInfo() {
            var pci = new CursorInfo {
                CbSize = Marshal.SizeOf(typeof(CursorInfo))
            };
            GetCursorInfo(ref pci);
            return pci;
        }

    }
}
