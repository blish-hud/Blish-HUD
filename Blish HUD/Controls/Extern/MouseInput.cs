using System;
using System.Runtime.InteropServices;

namespace Blish_HUD.Controls.Extern
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MouseInput
    {
        internal int dx;
        internal int dy;
        internal int mouseData;
        internal MouseEventF dwFlags;
        internal uint time;
        internal UIntPtr dwExtraInfo;
    }
}