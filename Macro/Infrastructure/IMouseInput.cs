using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro.Infrastructure
{
    public interface IMouseInput
    {
        IMouseInput MoveMouseBy(int pixelX, int pixelY);
        IMouseInput MoveMouseTo(int absoluteX, int absoluteY);
        IMouseInput MoveMouseToVirtualDesktop(int absoluteX, int absoluteY);

        IMouseInput LeftButtonDown();
        IMouseInput LeftButtonUp();
        IMouseInput LeftButtonClick();
        IMouseInput LeftButtonDoubleClick();

        IMouseInput RightButtonDown();
        IMouseInput RightButtonUp();
        IMouseInput RightButtonClick();
        IMouseInput RightButtonDoubleClick();
    }
}
