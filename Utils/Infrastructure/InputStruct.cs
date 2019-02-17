using System;
using System.Runtime.InteropServices;

namespace Utils.Infrastructure
{
    [Flags]
    internal enum KeyboardFlag : uint
    {
        ExtendedKey = 0x0001,
        KeyUp = 0x0002,
        Unicode = 0x0004,
        ScanCode = 0x0008,
    }
    [Flags]
    internal enum MouseFlag: uint
    {
        Move = 0x0001,
        LeftDown = 0x0002,
        LeftUp = 0x0004,
        RightDown = 0x0008,
        RightUp = 0x0010,
        MiddleDown = 0x0020,
        MiddleUp = 0x0040,
        XDown = 0x0080,
        XUp = 0x0100,
        VerticalWheel = 0x0800,
        HorizontalWheel = 0x1000,
        VirtualDesk = 0x4000,
        Absolute = 0x8000,
    }
    public enum MouseButton
    {
        LeftButton,
        RightButton,
    }

    public enum InputType : uint
    {
        Mouse = 0,
        Keyboard,
        Hardware
    }

    public enum WindowMessage : uint
    {
        None = 0x000,

        KeyDown = 0x100,
        KeyUp,
        Char,

        SysKeyDown = 0x104,
        SysKeyUp,

        SysCommand = 0x112,

        MouseMove = 0x200,
        LButtonDown = 0x201,
        LButtonUp,
        LButtonDoubleClick,
        RButtonDown,
        RButtonUp,
        RButtonDoubleClick
    }
    public enum ScanCode : uint
    {
        ESC = 0x01,

        KEY_1,
        KEY_2,
        KEY_3,
        KEY_4,
        KEY_5,
        KEY_6,
        KEY_7,
        KEY_8,
        KEY_9,
        KEY_0,

        SUBTRACT = 0x0C,
        UNDERSCORE = 0x0C,
        ADD = 0x0D,
        DOUBLEHYPHEN = 0x0D,
        BACKSPACE,
        TAB,

        KEY_Q = 0x10,
        KEY_W,
        KEY_E,
        KEY_R,
        KEY_T,
        KEY_Y,
        KEY_U,
        KEY_I,
        KEY_O,
        KEY_P,

        ENTER = 0x1C,
        CONTROL = 0x1D,
        CTRL= 0x1D,

        KEY_A = 0x1E,
        KEY_S,
        KEY_D,
        KEY_F,
        KEY_G,
        KEY_H,
        KEY_J,
        KEY_K,
        KEY_L,

        LSHIFT = 0x2A,

        KEY_Z = 0x2C,
        KEY_X,
        KEY_C,
        KEY_V,
        KEY_B,
        KEY_N,
        KEY_M,

        RSHIFT = 0x36,
        ALT = 0x38,
        SPACE = 0x39,

        F1 = 0x3B,
        F2,
        F3,
        F4,
        F5,
        F6 = 0x40,
        F7,
        F8,
        F9,
        F10,
        F11 = 0x85,
        F12 = 0x86,

        NUMPAD_0 = 0x52,
        NUMPAD_1 = 0x4F,
        NUMPAD_2 = 0x50,
        NUMPAD_3 = 0x51,
        NUMPAD_4 = 0x4B,
        NUMPAD_5 = 0x4C,
        NUMPAD_6 = 0x4D,
        NUMPAD_7 = 0x47,
        NUMPAD_8 = 0x48,
        NUMPAD_9 = 0x49,
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInput
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardInput
    {
        public ushort KeyCode;
        public ushort Scan;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct HardwareInput
    {
        public uint Msg;
        public ushort LParam;
        public ushort HParam;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct InputData
    {
        [FieldOffset(0)]
        public MouseInput Mouse;
        [FieldOffset(0)]
        public KeyboardInput Keyboard;
        [FieldOffset(0)]
        public HardwareInput Hardware;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Input
    {
        public uint Type;
        public InputData Data;
    }
}
