using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Utils;
using Utils.Infrastructure;

namespace Macro.Infrastructure
{
    public class InputManager
    {
        public static void ModifiedKeyStroke(KeyCode modifierKeyCodes, KeyCode keyCodes)
        {
            ModifiedKeyStroke(new KeyCode[] { modifierKeyCodes }, new KeyCode[] { keyCodes });
        }

        public static void ModifiedKeyStroke(IEnumerable<KeyCode> modifierKeyCodes, IEnumerable<KeyCode> keyCodes)
        {
            var builder = new InputBuilder();
            KeyDown(builder, modifierKeyCodes);
            KeyPress(builder, keyCodes);
            KeyUp(builder, modifierKeyCodes);
            NativeHelper.SendInput((uint)builder.Count, builder.ToArray(), Marshal.SizeOf(typeof(Input)));
        }
        public static void KeyUp(params KeyCode[] keyCodes)
        {
            if (keyCodes != null)
                KeyUp(keyCodes);
        }

        public static void KeyUp(IEnumerable<KeyCode> keyCodes)
        {
            var builder = new InputBuilder();
            KeyUp(builder, keyCodes);
            NativeHelper.SendInput((uint)builder.Count, builder.ToArray(), Marshal.SizeOf(typeof(Input)));
        }

        public static void KeyDown(params KeyCode[] keyCodes)
        {
            if (keyCodes != null)
                KeyDown(keyCodes);
        }

        public static void KeyDown(IEnumerable<KeyCode> keyCodes)
        {
            var builder = new InputBuilder();
            KeyDown(builder, keyCodes);
            NativeHelper.SendInput((uint)builder.Count, builder.ToArray(), Marshal.SizeOf(typeof(Input)));
        }

        public static void KeyPress(params KeyCode[] keyCodes)
        {
            if (keyCodes != null)
                KeyPress(keyCodes);
        }

        public static void KeyPress(IEnumerable<KeyCode> keyCodes)
        {
            var builder = new InputBuilder();
            KeyPress(builder, keyCodes);
            NativeHelper.SendInput((uint)builder.Count, builder.ToArray(), Marshal.SizeOf(typeof(Input)));
        }

        public static void KeyDown(InputBuilder builder, IEnumerable<KeyCode> keyCodes)
        {
            if (keyCodes == null)
                return;

            foreach (var key in keyCodes)
                builder.AddKeyDown(key);
        }
        private static void KeyPress(InputBuilder builder, IEnumerable<KeyCode> keyCodes)
        {
            if (keyCodes == null)
                return;

            foreach (var key in keyCodes)
                builder.AddKeyPress(key);
        }
        private static void KeyUp(InputBuilder builder, IEnumerable<KeyCode> keyCodes)
        {
            if (keyCodes == null)
                return;

            foreach (var key in keyCodes)
                builder.AddKeyUp(key);
        }
    }
}
