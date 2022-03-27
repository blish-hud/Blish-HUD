using System;
using System.Runtime.InteropServices;

namespace Blish_HUD.Controls.Extern
{
    [Obsolete("InputUnion is obsolete.", true)]
    [StructLayout(LayoutKind.Explicit)]
    internal struct InputUnion
    {
        [FieldOffset(0)]
        internal MouseInput mi;
        [FieldOffset(0)]
        internal KeybdInput ki;
        [FieldOffset(0)]
        internal HardwareInput hi;
    }
}