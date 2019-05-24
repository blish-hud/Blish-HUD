using System.Runtime.InteropServices;

namespace Blish_HUD.Controls.Extern
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct HardwareInput
    {
        internal int uMsg;
        internal short wParamL;
        internal short wParamH;
    }
}