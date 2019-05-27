using System.Runtime.InteropServices;

namespace Blish_HUD.Controls.Extern
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Input
    {
        internal InputType type;
        internal InputUnion U;
        internal static int Size => Marshal.SizeOf(typeof(Input));
    }
}