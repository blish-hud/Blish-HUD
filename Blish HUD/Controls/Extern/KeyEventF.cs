using System;

namespace Blish_HUD.Controls.Extern
{
    [Flags]
    internal enum KeyEventF : uint
    {
        EXTENDEDKEY = 0x0001,
        KEYUP = 0x0002,
        SCANCODE = 0x0008,
        UNICODE = 0x0004
    }
}