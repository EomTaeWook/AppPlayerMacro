using Macro.Infrastructure.Interface;

namespace Macro.Infrastructure.Manager
{
    public class InputManager
    {
        public IKeyboardInput Keyboard { get; private set; }
        public IMouseInput Mouse { get; private set; }

        public InputManager()
        {
        }
        public void SetKeyboardInput(IKeyboardInput keyboardInput)
        {
            Keyboard = keyboardInput;
        }
        public void SetMouseInput(IMouseInput mouse)
        {
            Mouse = mouse;
        }
    }
}
