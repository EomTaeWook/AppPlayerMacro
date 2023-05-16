using Kosher.Framework;
using System.Collections.Generic;
using System.Windows;

namespace Macro.Infrastructure.Manager
{
    public class UIManager : Singleton<UIManager>
    {
        private List<Window> _activePopup = new List<Window>();
        public void AddPopup<T>() where T: Window, new()
        {
            foreach(var item in _activePopup)
            {
                if(item is T == true)
                {
                    item.Activate();
                    return;
                }
            }

            var window = new T();
            _activePopup.Add(window);
            window.Owner = Application.Current.MainWindow;
            window.ShowInTaskbar = false;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Show();
        }
        public void ClosePopup(Window popup)
        {
            foreach(var item in _activePopup)
            {
                if(popup == item)
                {
                    popup.Close();
                    break;
                }
            }
            _activePopup.Remove(popup);
        }
    }
}
