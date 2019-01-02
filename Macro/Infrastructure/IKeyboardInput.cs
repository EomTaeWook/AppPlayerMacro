using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Infrastructure;

namespace Macro.Infrastructure
{
    public interface IKeyboardInput
    {
        IKeyboardInput ModifiedKeyStroke(KeyCode modifierKeyCodes, KeyCode keyCodes);
        IKeyboardInput ModifiedKeyStroke(IEnumerable<KeyCode> modifierKeyCodes, IEnumerable<KeyCode> keyCodes);

        IKeyboardInput KeyUp(params KeyCode[] keyCodes);
        IKeyboardInput KeyUp(IEnumerable<KeyCode> keyCodes);

        IKeyboardInput KeyDown(params KeyCode[] keyCodes);
        IKeyboardInput KeyDown(IEnumerable<KeyCode> keyCodes);

        IKeyboardInput KeyPress(params KeyCode[] keyCodes);
        IKeyboardInput KeyPress(IEnumerable<KeyCode> keyCodes);
    }
}
