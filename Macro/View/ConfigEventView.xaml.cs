using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Models;
using Macro.Models.ViewModel;
using Macro.UI;
using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Utils;
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

            //grdSaves.SelectionChanged += GrdSaves_SelectionChanged;
            //grdSaves.PreviewMouseLeftButtonDown += GrdSaves_PreviewMouseLeftButtonDown;
            //grdSaves.MouseMove += GrdSaves_MouseMove;
            //grdSaves.MouseLeave += GrdSaves_MouseLeave;
            //grdSaves.PreviewMouseUp += GrdSaves_MouseLeave;

            treeSaves.SelectedItemChanged += TreeSaves_SelectedItemChanged;
            treeSaves.PreviewMouseLeftButtonDown += TreeSaves_PreviewMouseLeftButtonDown;
            treeSaves.MouseMove += TreeSaves_MouseMove;
            treeSaves.MouseLeftButtonUp += TreeSaves_MouseLeave;
            treeSaves.MouseLeave += TreeSaves_MouseLeave;

            Unloaded += ConfigEventView_Unloaded;
        }

        private void TreeSaves_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(treeSaves.SelectedItem is EventTriggerModel item)
            {
                Model = item;
                SelectData(Model);
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
            e.Handled = true;
        }

        private void TreeSaves_MouseLeave(object sender, MouseEventArgs e)
        {
            if(_isDrag && e.LeftButton == MouseButtonState.Released)
            {
                e.Handled = true;
                _isDrag = false;
                var viewModel = DataContext as EventTriggerModel;
                var targetRow = (sender as UIElement).TryFindFromPoint<TreeGridViewItem>(e.GetPosition(treeSaves));
                if (targetRow != null)
                {
                    var dragItem = (DataContext as ConfigEventViewModel).DragTreeItem;
                    if (targetRow == dragItem)
                        return;

                    if (targetRow.ParentItem == null && dragItem.ParentItem == null)
                    {
                        var targetIndex = (DataContext as ConfigEventViewModel).TriggerSaves.IndexOf(targetRow.DataContext as EventTriggerModel);
                        var item = dragItem.DataContext as EventTriggerModel;
                        (DataContext as ConfigEventViewModel).TriggerSaves.Remove(item);
                        (DataContext as ConfigEventViewModel).TriggerSaves.Insert(targetIndex, item);
                    }
                    else if (targetRow.ParentItem != dragItem.ParentItem)
                    {
                        var targetItem = targetRow.DataContext as EventTriggerModel;
                        var item = dragItem.DataContext as EventTriggerModel;
                        (dragItem.ParentItem.DataContext as EventTriggerModel).SubEventTriggers.Remove(item);
                        targetItem.SubEventTriggers.Add(item);
                    }
                    else if (targetRow.ParentItem == dragItem.ParentItem)
                    {
                        var targetParent = (targetRow.ParentItem.DataContext as EventTriggerModel);
                        var targetIndex = targetParent.SubEventTriggers.IndexOf(targetRow.DataContext as EventTriggerModel);
                        var item = dragItem.DataContext as EventTriggerModel;
                        targetParent.SubEventTriggers.Remove(item);
                        targetParent.SubEventTriggers.Insert(targetIndex, item);
                    }
                }
                (DataContext as ConfigEventViewModel).DragTreeItem = null;
            }
        }
        private void TreeSaves_MouseMove(object sender, MouseEventArgs e)
        {
            if(_isDrag && e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(treeSaves, (DataContext as ConfigEventViewModel).DragTreeItem.DataContext, DragDropEffects.Move);
            }
        }

        private void TreeSaves_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var target = (sender as UIElement).TryFindFromPoint<TreeGridViewItem>(e.GetPosition(treeSaves));
            if (target == null)
                return;
            _isDrag = true;
            (DataContext as ConfigEventViewModel).DragTreeItem = target;
            OnPreviewMouseLeftButtonDown(e);
        }
        private void ConfigEventView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Clear();
                SelectData(null);
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

        //private void GrdSaves_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    dragPopup.IsOpen = false;
        //    if (_isDrag)
        //    {
        //        var viewModel = DataContext as ConfigEventViewModel;
        //        var row = (sender as UIElement).TryFindFromPoint<DataGridRow>(e.GetPosition(grdSaves));
        //        if(row != null)
        //        {
        //            var target = row.Item as EventTriggerModel;
        //            if (viewModel.DragItem != target)
        //            {
        //                var targetIndex = viewModel.TriggerSaves.IndexOf(target);
        //                viewModel.TriggerSaves.Remove(viewModel.DragItem);
        //                viewModel.TriggerSaves.Insert(targetIndex, viewModel.DragItem);

        //                NotifyHelper.InvokeNotify(Infrastructure.EventType.EventTriggerOrderChanged, new EventTriggerOrderChangedEventArgs()
        //                {
        //                    TriggerModel1 = target,
        //                    TriggerModel2 = viewModel.DragItem
        //                });
        //            }
        //        }
        //        viewModel.DragItem = _dummy;
        //    }
        //    _isDrag = false;
        //}

        //private void GrdSaves_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (!_isDrag || e.LeftButton != MouseButtonState.Pressed)
        //        return;
        //    Point position = e.GetPosition(grdSaves);
        //    var row = (sender as UIElement).TryFindFromPoint<DataGridRow>(position);
        //    if (row != null)
        //    {
        //        if (!dragPopup.IsOpen)
        //        {
        //            dragPopup.IsOpen = true;
        //            var popupSize = new Size(dragPopup.ActualWidth, dragPopup.ActualHeight);
        //            dragPopup.PlacementRectangle = new Rect(e.GetPosition(this), popupSize);
        //        }
        //    }
        //}

        //private void GrdSaves_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    var row = (sender as UIElement).TryFindFromPoint<DataGridRow>(e.GetPosition(grdSaves));
        //    if (row == null)
        //        return;
        //    _isDrag = true;
        //    (DataContext as ConfigEventViewModel).DragItem = row.Item as EventTriggerModel;
        //}

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
