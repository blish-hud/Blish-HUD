﻿using System.Runtime.InteropServices;
namespace Blish_HUD.Input.WinApi {
    [StructLayout(LayoutKind.Sequential)]
    internal struct Input {
        internal        InputType  type;
        internal        InputUnion U;
        internal static int        Size => Marshal.SizeOf(typeof(Input));
    }

    internal enum InputType : uint {
        MOUSE    = 0,
        KEYBOARD = 1,
        HARDWARE = 2
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct InputUnion {
        [FieldOffset(0)]
        internal MouseUtil.MouseInput mi;
        [FieldOffset(0)]
        internal KeyboardUtil.KeybdInput ki;
        [FieldOffset(0)]
        internal HardwareInput hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HardwareInput {
        internal int   uMsg;
        internal short wParamL;
        internal short wParamH;
    }
}