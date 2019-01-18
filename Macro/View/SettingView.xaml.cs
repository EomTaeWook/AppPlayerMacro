using Macro.Extensions;
using Macro.Models.ViewModel;
using MahApps.Metro.Controls;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if(btn.Equals(btnSave))
            {
                var model = (DataContext as SettingViewModel).Config;
                if (TryModelValidate(model, out Message error))
                {
                    _taskQueue.Enqueue(Save, model);
                }
                else
                {
                    (Application.Current.MainWindow as MetroWindow).MessageShow("Error", DocumentHelper.Get(error));
                }
            }
        }
    }
}
