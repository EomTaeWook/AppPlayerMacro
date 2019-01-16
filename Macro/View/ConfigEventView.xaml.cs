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
                Model = item;
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

            grdSaves.PreviewMouseLeftButtonDown += GrdSaves_PreviewMouseLeftButtonDown;
            grdSaves.MouseMove += GrdSaves_MouseMove;
            grdSaves.MouseLeave += GrdSaves_MouseLeave;
            grdSaves.PreviewMouseUp += GrdSaves_MouseLeave;

            Unloaded += ConfigEventView_Unloaded;
        }

        private void GrdSaves_MouseLeave(object sender, MouseEventArgs e)
        {
            dragPopup.IsOpen = false;
            if (_isDrag)
            {
                var viewModel = DataContext as ConfigEventViewModel;
                var row = (sender as UIElement).TryFindFromPoint<DataGridRow>(e.GetPosition(grdSaves));
                if(row != null)
                {
                    var target = row.Item as EventTriggerModel;
                    if (viewModel.DragItem != target)
                    {
                        var targetIndex = viewModel.TriggerSaves.IndexOf(target);
                        var selectIndex = viewModel.TriggerSaves.IndexOf(viewModel.DragItem);
                        viewModel.TriggerSaves.Swap(targetIndex, selectIndex);

                        NotifyHelper.InvokeNotify(Infrastructure.EventType.EventTriggerOrderChanged, new EventTriggerOrderChangedEventArgs()
                        {
                            TriggerModel1 = target,
                            TriggerModel2 = viewModel.DragItem
                        });
                    }
                }
                viewModel.DragItem = _dummy;
            }
            _isDrag = false;
        }

        private void GrdSaves_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDrag || e.LeftButton != MouseButtonState.Pressed)
                return;
            Point position = e.GetPosition(grdSaves);
            var row = (sender as UIElement).TryFindFromPoint<DataGridRow>(position);
            if (row != null)
            {
                if (!dragPopup.IsOpen)
                {
                    dragPopup.IsOpen = true;
                    var popupSize = new Size(dragPopup.ActualWidth, dragPopup.ActualHeight);
                    dragPopup.PlacementRectangle = new Rect(e.GetPosition(this), popupSize);
                }
            }
        }

        private void GrdSaves_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var row = (sender as UIElement).TryFindFromPoint<DataGridRow>(e.GetPosition(grdSaves));
            if (row == null)
                return;
            _isDrag = true;
            (DataContext as ConfigEventViewModel).DragItem = row.Item as EventTriggerModel;
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
