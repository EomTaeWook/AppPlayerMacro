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
        public uint KeyCode;
        public uint Scan;
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
