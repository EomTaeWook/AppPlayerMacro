using Macro.Infrastructure.Interface;

namespace Macro.Infrastructure.Manager
{
    public class InputManager
    {
        public IKeyboardInput Keyboard { get; private set; }
        public IMouseInput Mouse { get; private set; }

        public InputManager(IKeyboardInput keyboard, IMouseInput mouse)
        {
            Mouse = mouse;
            Keyboard = keyboard;
        }
    }
}
