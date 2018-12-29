using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Infrastructure;

namespace Macro.Extensions
{
    public static class KeyCodeExtension
    {
        public static bool IsExtendedKey(this KeyCode keyCode)
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
    }
}
