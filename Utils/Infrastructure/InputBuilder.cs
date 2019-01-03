using System;
using System.Collections;
using System.Collections.Generic;

namespace Utils.Infrastructure
{
    public class InputBuilder : IEnumerable<Input>
    {
        private readonly List<Input> _inputList;
        public InputBuilder()
        {
            _inputList = new List<Input>();
        }
        #region Mouse
        public InputBuilder AddRelativeMouseMove(int x, int y)
        {
            var input = new Input { Type = (uint)InputType.Mouse };
            input.Data.Mouse.Flags = (uint)MouseFlag.Move;
            input.Data.Mouse.X = x;
            input.Data.Mouse.Y = y;
            _inputList.Add(input);
            return this;
        }
        public InputBuilder AddAbsoluteMouseMove(int absoluteX, int absoluteY)
        {
            var input = new Input { Type = (uint)InputType.Mouse };
            input.Data.Mouse.Flags = (uint)(MouseFlag.Move | MouseFlag.Absolute);
            input.Data.Mouse.X = absoluteX;
            input.Data.Mouse.Y = absoluteY;
            _inputList.Add(input);
            return this;
        }
        public InputBuilder AddAbsoluteMouseMoveOnVirtualDesktop(int absoluteX, int absoluteY)
        {
            var input = new Input { Type = (uint)InputType.Mouse };
            input.Data.Mouse.Flags = (uint)(MouseFlag.Move | MouseFlag.Absolute | MouseFlag.VirtualDesk);
            input.Data.Mouse.X = absoluteX;
            input.Data.Mouse.Y = absoluteY;
            _inputList.Add(input);
            return this;
        }
        public InputBuilder AddMouseButtonDown(MouseButton button)
        {
            var input = new Input { Type = (uint)InputType.Mouse };
            input.Data.Mouse.Flags = (uint)MouseButtonToMouseFlag(button);
            _inputList.Add(input);
            
            return this;
        }
        public InputBuilder AddMouseButtonUp(MouseButton button)
        {
            var input = new Input { Type = (uint)InputType.Mouse };
            input.Data.Mouse.Flags = (uint)MouseButtonToMouseFlag(button, true);
            _inputList.Add(input);

            return this;
        }
        public InputBuilder AddMouseButtonClick(MouseButton button)
        {
            return AddMouseButtonDown(button).AddMouseButtonUp(button);
        }
        public InputBuilder AddMouseButtonDoubleClick(MouseButton button)
        {
            return AddMouseButtonClick(button).AddMouseButtonClick(button);
        }

        private MouseFlag MouseButtonToMouseFlag(MouseButton button, bool isMouseUp = false)
        {
            switch(button)
            {
                case MouseButton.LeftButton:
                    return isMouseUp ? MouseFlag.LeftUp : MouseFlag.LeftDown;
                case MouseButton.RightButton:
                    return isMouseUp ? MouseFlag.RightUp : MouseFlag.RightDown;
                default:
                    return isMouseUp ? MouseFlag.LeftUp : MouseFlag.LeftDown;
            }
        }
        #endregion Mouse
        #region Keyboard
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
        #endregion Keyboard

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
