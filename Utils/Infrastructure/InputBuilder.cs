using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Infrastructure
{
    public class InputBuilder : IEnumerable<Input>
    {
        private readonly List<Input> _inputList;
        public InputBuilder()
        {
            _inputList = new List<Input>();
        }
        public InputBuilder AddKeyPress(KeyCode keyCode)
        {
            AddKeyDown(keyCode);
            AddKeyUp(keyCode);
            return this;
        }
        public InputBuilder AddKeyUp(KeyCode keyCode)
        {
           _inputList.Add(new Input
            {
                Type = (uint)InputType.Keyboard,
                Data = new InputData()
                {
                    Keyboard = new KeyboardInput()
                    {
                        KeyCode = (uint)keyCode,
                        Scan = 0,
                        Flags = IsExtendedKey(keyCode) ? (uint)(KeyboardFlag.KeyUp | KeyboardFlag.ExtendedKey) : (uint)KeyboardFlag.KeyUp,
                        Time = 0,
                        ExtraInfo = IntPtr.Zero
                    }
                }
            });
            return this;
        }
        public InputBuilder AddKeyDown(KeyCode keyCode)
        {
            _inputList.Add(new Input
            {
                Type = (uint)InputType.Keyboard,
                Data = new InputData()
                {
                    Keyboard = new KeyboardInput()
                    {
                        KeyCode = (uint)keyCode,
                        Scan = 0,
                        Flags = IsExtendedKey(keyCode) ? (uint)KeyboardFlag.ExtendedKey : 0,
                        Time = 0,
                        ExtraInfo = IntPtr.Zero
                    }
                }
            });
            return this;
        }
        public bool IsExtendedKey(KeyCode keyCode)
        {
            if (keyCode == KeyCode.ALT ||
                keyCode == KeyCode.CONTROL ||
                keyCode == KeyCode.CTRL ||
                keyCode == KeyCode.RCONTROL ||
                keyCode == KeyCode.INSERT ||
                keyCode == KeyCode.DELETE ||
                keyCode == KeyCode.HOME ||
                keyCode == KeyCode.END ||
                keyCode == KeyCode.RIGHT ||
                keyCode == KeyCode.UP ||
                keyCode == KeyCode.LEFT ||
                keyCode == KeyCode.DOWN ||
                keyCode == KeyCode.NUMLOCK ||
                keyCode == KeyCode.CANCEL ||
                keyCode == KeyCode.SNAPSHOT ||
                keyCode == KeyCode.DIVIDE)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public Input this[int index]
        {
            get=> _inputList [index];
        }
        public Input[] ToArray()
        {
            return _inputList.ToArray();
        }

        public IEnumerator<Input> GetEnumerator()
        {
            return _inputList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
