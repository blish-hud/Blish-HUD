using System;

namespace Blish_HUD.Controls.Extern
{
    [Obsolete("InputType is obsolete.", true)]
    internal enum InputType : uint
    {
        MOUSE = 0,
        KEYBOARD = 1,
        HARDWARE = 2
    }
}