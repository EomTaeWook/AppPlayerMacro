using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Models;
using Macro.Models.ViewModel;
using Macro.UI;
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
        private void ConfigEventView_Loaded(object sender, RoutedEventArgs e)
        {
            InitEvent();
            Init();
        }
        
        private void InitEvent()
        {
            NotifyHelper.ScreenCaptureDataBind += NotifyHelper_ScreenCaptureDataBind;
            NotifyHelper.MousePositionDataBind += NotifyHelper_MousePositionDataBind;

            var radioButtons = this.FindChildren<RadioButton>();
            foreach (var button in radioButtons)
            {
                button.Click += RadioButton_Click;
            }

            btnMouseCoordinate.Click += Button_Click;
            PreviewKeyDown += ConfigEventView_PreviewKeyDown;

            treeSaves.SelectedItemChanged += TreeSaves_SelectedItemChanged;
            treeSaves.PreviewMouseLeftButtonDown += TreeSaves_PreviewMouseLeftButtonDown;
            treeSaves.MouseMove += TreeSaves_MouseMove;
            treeSaves.Drop += TreeSaves_Drop;

            Unloaded += ConfigEventView_Unloaded;
        }
        private void TreeSaves_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(treeSaves.SelectedItem is EventTriggerModel item)
            {
                (DataContext as ConfigEventViewModel).SelectTreeItem = treeSaves.GetSelectItemFromObject<TreeGridViewItem>(treeSaves.SelectedItem);
                Model = item;
                NotifyHelper.InvokeNotify(Infrastructure.EventType.SelectEventTriggerChanged, new SelectEventTriggerChangedEventArgs()
                {
                    TriggerModel = Model,
                    TreeViewItem = (DataContext as ConfigEventViewModel).SelectTreeItem
                });

                if (Model.EventType == EventType.Keyboard)
                {
                    RadioButton_Click(rbKeyboard, null);
                }
                else if (Model.EventType == EventType.Mouse)
                {
                    RadioButton_Click(rbMouse, null);
                }
                else if (Model.EventType == EventType.Image)
                {
                    RadioButton_Click(rbImage, null);
                }
            }
        }

        private void TreeSaves_Drop(object sender, DragEventArgs e)
        {
            if(_isDrag)
            {
                _isDrag = false;
                ItemContainerPositionChange(e.GetPosition(treeSaves));
                (DataContext as ConfigEventViewModel).SelectTreeItem = null;
            }
        }
        private void TreeSaves_MouseMove(object sender, MouseEventArgs e)
        {
            if(!_isDrag && e.LeftButton == MouseButtonState.Pressed)
            {
                var target = (sender as UIElement).TryFindFromPoint<TreeGridViewItem>(e.GetPosition(treeSaves));
                if (target == null)
                    return;

                _isDrag = true;
                (DataContext as ConfigEventViewModel).SelectTreeItem = target;

                DragDrop.DoDragDrop((DataContext as ConfigEventViewModel).SelectTreeItem, new object(), DragDropEffects.Move);
            }
        }

        private void TreeSaves_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDrag = false;
  
        }
        private void ConfigEventView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Clear();
                NotifyHelper.InvokeNotify(Infrastructure.EventType.SelectEventTriggerChanged, new SelectEventTriggerChangedEventArgs());
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }

        private void NotifyHelper_MousePositionDataBind(MousePointEventArgs args)
        {
            if (Model == _dummy)
                Model = new EventTriggerModel();

            Model.MonitorInfo = args.MonitorInfo;
            Model.MousePoint = args.MousePoint;
            foreach (var item in _mousePointViews)
            {
                item.Hide();
            }
        }

        private void NotifyHelper_ScreenCaptureDataBind(CaptureEventArgs args)
        {
            if (Model == _dummy)
                Model = new EventTriggerModel();
            RadioButtonRefresh();
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
                Model.EventType = EventType.Mouse;
            }
            else if (sender.Equals(rbKeyboard))
            {
                Model.EventType = EventType.Keyboard;
            }
            else if(sender.Equals(rbImage))
            {
                Model.EventType = EventType.Image;
            }
            RadioButtonRefresh();
        }        
    }
}
