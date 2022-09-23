using MahApps.Metro.Controls;

namespace Macro.UI
{
    public abstract class  UIItem : MetroWindow
    {
        public UIItem()
        {
            ShowCloseButton = false;
            ShowMinButton = false;
            ShowMaxRestoreButton = false;
            ShowTitleBar = false;
            ShowActivated = false;
            ShowInTaskbar = false;
            ResizeMode = System.Windows.ResizeMode.NoResize;
        }
    }
}
