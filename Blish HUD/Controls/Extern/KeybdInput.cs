using System;
using System.Runtime.InteropServices;

namespace Blish_HUD.Controls.Extern
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct KeybdInput
    {
        internal VirtualKeyShort wVk;
        internal ScanCodeShort wScan;
        internal KeyEventF dwFlags;
        internal int time;
        internal UIntPtr dwExtraInfo;
    }
}