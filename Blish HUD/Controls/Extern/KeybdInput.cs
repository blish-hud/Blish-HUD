using System;
using System.Runtime.InteropServices;

namespace Blish_HUD.Controls.Extern
{
    [Obsolete("KeybdInput is obsolete.", true)]
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