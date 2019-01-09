using Macro.Extensions;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using Utils.Document;

namespace Macro.View
{
    /// <summary>
    /// SettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingView : UserControl
    {
        private void SettingView_Loaded(object sender, RoutedEventArgs e)
        {
            EventInit();
            Init();
        }
        private void EventInit()
        {
            btnSave.Click += Button_Click;
        }

        private void TxtSavePath_StylusButtonDown(object sender, System.Windows.Input.StylusButtonEventArgs e)
        {
            var dlg = new SaveFileDialog();
            if(dlg.ShowDialog() == true)
            {
                string path = dlg.FileName;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if(btn.Equals(btnSave))
            {
                if(TryModelValidate(_dummy, out Message error))
                {
                    _taskQueue.Enqueue(Save);
                }
                else
                {
                    
                }
            }
        }
    }
}
