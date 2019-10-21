using Macro.Models.ViewModel;
using Macro.View;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Utils;

namespace Macro.Infrastructure.Impl
{
    public abstract class BaseEventConfigView<ViewModel> : UserControl 
                                                where ViewModel : IBaseNotifyEventConfigViewModel
    {

        protected readonly List<MousePositionView> _mousePointViews;
        protected bool _isDrag;
        protected readonly ObservableCollection<KeyValuePair<RepeatType, string>> _repeatItems;

        public BaseEventConfigView()
        {
            _mousePointViews = new List<MousePositionView>();

            _repeatItems = new ObservableCollection<KeyValuePair<RepeatType, string>>();

            _mousePointViews = new List<MousePositionView>();

            foreach (var item in DisplayHelper.MonitorInfo())
            {
                _mousePointViews.Add(new MousePositionView(item));
            }

            Application.Current.MainWindow.Unloaded += MainWindow_Unloaded;
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in _mousePointViews)
            {
                item.Close();
            }
            _mousePointViews.Clear();
        }

        protected void ShowMousePoisitionView()
        {
            foreach (var item in _mousePointViews)
            {
                item.ShowActivate();
            }
        }
    }
}
