using Dignus.DependencyInjection.Attributes;
using Macro.Infrastructure.Interface;

namespace Macro.Infrastructure.Manager
{
    [Injectable(Dignus.DependencyInjection.LifeScope.Transient)]
    public class InputManager
    {
        public IKeyboardInput Keyboard { get; private set; }
        public IMouseInput Mouse { get; private set; }

        public InputManager(IKeyboardInput keyboardInput, IMouseInput mouse)
        {
            Keyboard = keyboardInput;
            Mouse = mouse;
        }
    }
}
