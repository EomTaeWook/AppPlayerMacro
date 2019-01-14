using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Models;
using Macro.Models.ViewModel;
using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EventType = Macro.Models.EventType;

namespace Macro.View
{
    /// <summary>
    /// ConfigEventView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ConfigEventView : UserControl
    {
        public event SelectTriggerHandler SelectData;
        public delegate void SelectTriggerHandler(EventTriggerModel model);
        public delegate Point GetDragDropPosition(IInputElement theElement);

        private int _prevRowIndex = -1;
        private void ConfigEventView_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                grdSaves.SelectedItem = null;
                Model = _dummy;
                SelectData(null);
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }

        private void GrdSaves_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as DataGrid).SelectedItem is EventTriggerModel item)
            {
                Model = new EventTriggerModel(item);
                SelectData(item);
                if(Model.EventType == EventType.Keyboard)
                {
                    btnMouseCoordinate.Visibility = Visibility.Collapsed;
                    txtKeyboardCmd.Visibility = Visibility.Visible;
                }
                else if(Model.EventType == EventType.Mouse)
                {
                    btnMouseCoordinate.Visibility = Visibility.Visible;
                    txtKeyboardCmd.Visibility = Visibility.Collapsed;
                }
                e.Handled = true;
            }
        }
        private void ConfigEventView_Loaded(object sender, RoutedEventArgs e)
        {
            InitEvent();
            Init();
        }
        
        private void InitEvent()
        {
            var radioButtons = this.FindChildren<RadioButton>();
            foreach (var button in radioButtons)
            {
                button.Click += RadioButton_Click;
            }

            btnMouseCoordinate.Click += Button_Click;
            grdSaves.SelectionChanged += GrdSaves_SelectionChanged;
            PreviewKeyDown += ConfigEventView_PreviewKeyDown;

            NotifyHelper.MousePositionDataBind += (args) => 
            {
                if (Model == _dummy)
                {
                    Model = new EventTriggerModel();
                }
                Model.MonitorInfo = args.MonitorInfo;
                Model.MousePoint = args.MousePoint;
                foreach (var item in _mousePointViews)
                {
                    item.Hide();
                }
            };

            grdSaves.Drop += GrdSaves_Drop;
            grdSaves.PreviewMouseLeftButtonDown += GrdSaves_PreviewMouseLeftButtonDown;

            Unloaded += ConfigEventView_Unloaded;
        }
        
        private void GrdSaves_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _prevRowIndex = CurrentRowIndex(e.GetPosition);
            if (_prevRowIndex < 0)
                return;
            grdSaves.SelectedIndex = _prevRowIndex;

            var item = grdSaves.Items[_prevRowIndex] as EventTriggerModel;
            if (item == null)
                return;
            DragDropEffects dragdropeffects = DragDropEffects.Move;
            if (DragDrop.DoDragDrop(grdSaves, item, dragdropeffects) != DragDropEffects.None)
            {
                grdSaves.SelectedItem = item;
            }
        }

        private void GrdSaves_Drop(object sender, DragEventArgs e)
        {
            if(_prevRowIndex < 0)
                return;
            int index = CurrentRowIndex(e.GetPosition);
            if (index < 0)
                return;
            if (index == _prevRowIndex)
                return;
            if (index == grdSaves.Items.Count - 1)
                return;
            var item = grdSaves.Items[index];
            grdSaves.Items.RemoveAt(index);
            grdSaves.Items.Insert(index, item);
        }

        private void ConfigEventView_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in _mousePointViews)
            {
                item.Close();
            }
            _mousePointViews.Clear();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(btnMouseCoordinate))
            {
                ShowMousePoisitionView();
            }
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (Model == _dummy)
            {
                Model = new EventTriggerModel();
            }

            if (sender.Equals(rbMouse))
            {
                btnMouseCoordinate.Visibility = Visibility.Visible;
                txtKeyboardCmd.Visibility = Visibility.Collapsed;

                Model.EventType = EventType.Mouse;
            }
            else if (sender.Equals(rbKeyboard))
            {
                btnMouseCoordinate.Visibility = Visibility.Collapsed;
                txtKeyboardCmd.Visibility = Visibility.Visible;

                Model.EventType = EventType.Keyboard;
            }
        }
        
    }
}
