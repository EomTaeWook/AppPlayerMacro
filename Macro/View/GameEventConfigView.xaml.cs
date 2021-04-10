using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Impl;
using Macro.Models;
using Macro.Models.ViewModel;
using Macro.UI;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Macro.View
{
    /// <summary>
    /// GameEventConfigView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GameEventConfigView : BaseEventConfigView<GameEventConfigViewModel>
    {
        private void InitEvent()
        {
            NotifyHelper.ScreenCaptureDataBind += NotifyHelper_ScreenCaptureDataBind;
            NotifyHelper.MousePositionDataBind += NotifyHelper_MousePositionDataBind;
            NotifyHelper.ConfigChanged += NotifyHelper_ConfigChanged;
            NotifyHelper.TreeGridViewFocus += NotifyHelper_TreeGridViewFocus;

            var radioButtons = this.FindChildren<RadioButton>();
            foreach (var button in radioButtons)
            {
                button.Click += RadioButton_Click;
            }
            var commonButtons = this.FindChildren<Button>();
            foreach (var button in commonButtons)
            {
                button.Click += Button_Click;
            }
            checkImageSearchRequired.Checked += CheckImageSearchRequired_Checked;
            checkImageSearchRequired.Unchecked += CheckImageSearchRequired_Checked;

            PreviewKeyDown += GameEventConfigView_PreviewKeyDown;

            treeSaves.SelectedItemChanged += TreeSaves_SelectedItemChanged;
            treeSaves.PreviewMouseLeftButtonDown += TreeSaves_PreviewMouseLeftButtonDown;
            treeSaves.MouseMove += TreeSaves_MouseMove;
            treeSaves.Drop += TreeSaves_Drop;

            checkSameImageDrag.Checked += CheckSameImageDrag_Checked;
            checkSameImageDrag.Unchecked += CheckSameImageDrag_Checked;
        }

        private void CheckSameImageDrag_Checked(object sender, RoutedEventArgs e)
        {
            if (checkSameImageDrag.IsChecked == true)
            {
                numMaxSameImageCount.Visibility = Visibility.Visible;
            }
            else
            {
                numMaxSameImageCount.Visibility = Visibility.Collapsed;
            }
        }
        private void NotifyHelper_TreeGridViewFocus(TreeGridViewFocusEventArgs obj)
        {
            //if (obj.Mode == InitialTab.Game)
            //{
            //    this.treeSaves.Focus();
            //}
        }
        private void TreeSaves_Drop(object sender, DragEventArgs e)
        {
            if (_isDrag)
            {
                _isDrag = false;
                var targetRow = treeSaves.TryFindFromPoint<TreeGridViewItem>(e.GetPosition(treeSaves));
                if (CurrentTreeViewItem == targetRow)
                    return;
                ItemContainerPositionChange(targetRow);
                var item = CurrentTreeViewItem.DataContext<GameEventTriggerModel>();
                Clear();
                NotifyHelper.InvokeNotify(NotifyEventType.TreeItemOrderChanged, new EventTriggerOrderChangedEventArgs()
                {
                    TriggerModel1 = item,
                    TriggerModel2 = targetRow?.DataContext<GameEventTriggerModel>()
                });
            }
        }
        private void TreeSaves_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDrag && e.LeftButton == MouseButtonState.Pressed)
            {
                var target = (sender as UIElement).TryFindFromPoint<TreeGridViewItem>(e.GetPosition(treeSaves));
                if (target == null)
                    return;

                _isDrag = true;
                CurrentTreeViewItem = target;

                DragDrop.DoDragDrop(CurrentTreeViewItem, new object(), DragDropEffects.Move);
            }
        }
        private void TreeSaves_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDrag = false;
        }
        private void TreeSaves_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (treeSaves.SelectedItem is GameEventTriggerModel item)
            {
                CurrentTreeViewItem = treeSaves.GetSelectItemFromObject<TreeGridViewItem>(treeSaves.SelectedItem) ?? _dummyTreeGridViewItem;
                NotifyHelper.InvokeNotify(NotifyEventType.SelctTreeViewItemChanged, new SelctTreeViewItemChangedEventArgs()
                {
                    TreeViewItem = CurrentTreeViewItem
                });

                if (CurrentTreeViewItem.DataContext<GameEventTriggerModel>().EventType == EventType.Keyboard)
                {
                    RadioButton_Click(rbKeyboard, null);
                }
                else if (CurrentTreeViewItem.DataContext<GameEventTriggerModel>().EventType == EventType.Mouse)
                {
                    RadioButton_Click(rbMouse, null);
                    //btnMouseWheel.IsEnabled = true;
                    //lblWheelData.Visibility = Visibility.Collapsed;
                    //gridWheelData.Visibility = Visibility.Collapsed;

                    //if (CurrentTreeViewItem.DataContext<EventTriggerModel>().MouseTriggerInfo.MouseInfoEventType == MouseEventType.Wheel)
                    //{
                    //    lblWheelData.Visibility = Visibility.Visible;
                    //    gridWheelData.Visibility = Visibility.Visible;
                    //}
                }
                else if (CurrentTreeViewItem.DataContext<GameEventTriggerModel>().EventType == EventType.Image)
                {
                    RadioButton_Click(rbImage, null);
                }
                else if (CurrentTreeViewItem.DataContext<GameEventTriggerModel>().EventType == EventType.RelativeToImage)
                {
                    RadioButton_Click(rbRelativeToImage, null);
                }
                btnTreeItemUp.Visibility = btnTreeItemDown.Visibility = Visibility.Visible;
                if (item.SubEventTriggers.Count != 0)
                {
                    lblRepeatSubItems.Visibility = Visibility.Visible;
                    gridRepeat.Visibility = Visibility.Visible;
                }
                else
                {
                    lblRepeatSubItems.Visibility = Visibility.Collapsed;
                    gridRepeat.Visibility = Visibility.Collapsed;
                }
                
            }
        }
        private void GameEventConfigView_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Clear();
                NotifyHelper.InvokeNotify(NotifyEventType.SelctTreeViewItemChanged, new SelctTreeViewItemChangedEventArgs());
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }

        private void NotifyHelper_ScreenCaptureDataBind(CaptureEventArgs e)
        {
            if (e.CaptureViewMode != CaptureViewMode.Game &&
                e.CaptureViewMode != CaptureViewMode.HP &&
                e.CaptureViewMode != CaptureViewMode.Mp)
            {
                return;
            }

            if (CurrentTreeViewItem == _dummyTreeGridViewItem)
            {
                CurrentTreeViewItem = new TreeGridViewItem()
                {
                    DataContext = new GameEventTriggerModel()
                };
            }
            if (RelativePosition == _dummyRelativePosition)
            {
                RelativePosition = new PointModel();
            }
            RadioButtonRefresh();
        }
        private void NotifyHelper_ConfigChanged(ConfigEventArgs config)
        {
            _repeatItems.Clear();
            foreach (var type in Enum.GetValues(typeof(RepeatType)))
            {
                if (Enum.TryParse($"Repeat{type.ToString()}", out Utils.Document.Label label))
                {
                    _repeatItems.Add(new KeyValuePair<RepeatType, string>((RepeatType)type, DocumentHelper.Get(label)));
                }
            }
        }
        private void CheckImageSearchRequired_Checked(object sender, RoutedEventArgs e)
        {
            if(checkImageSearchRequired.IsChecked.HasValue)
            {
                _contextViewModel.CurrentTreeViewItem.DataContext<GameEventTriggerModel>().ImageSearchRequired = checkImageSearchRequired.IsChecked.Value;
            }
            else
            {
                _contextViewModel.CurrentTreeViewItem.DataContext<GameEventTriggerModel>().ImageSearchRequired = false;
            }
        }

        private void NotifyHelper_MousePositionDataBind(MousePointEventArgs e)
        {
            if (e.MousePointViewMode != MousePointViewMode.Game)
            {
                return;
            }

            if (CurrentTreeViewItem == _dummyTreeGridViewItem)
            {
                CurrentTreeViewItem = new TreeGridViewItem()
                {
                    DataContext = new GameEventTriggerModel()
                };
            }
            CurrentTreeViewItem.DataContext<GameEventTriggerModel>().MonitorInfo = e.MonitorInfo;
            CurrentTreeViewItem.DataContext<GameEventTriggerModel>().MouseTriggerInfo = e.MouseTriggerInfo;
            foreach (var item in _mousePointViews)
            {
                item.Hide();
            }
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentTreeViewItem == _dummyTreeGridViewItem)
            {
                CurrentTreeViewItem = new TreeGridViewItem()
                {
                    DataContext = new GameEventTriggerModel()
                };
            }
            if (RelativePosition == _dummyRelativePosition)
            {
                RelativePosition = new PointModel();
            }

            if (sender.Equals(rbMouse))
            {
                CurrentTreeViewItem.DataContext<GameEventTriggerModel>().EventType = EventType.Mouse;
            }
            else if (sender.Equals(rbKeyboard))
            {
                CurrentTreeViewItem.DataContext<GameEventTriggerModel>().EventType = EventType.Keyboard;
            }
            else if (sender.Equals(rbImage))
            {
                CurrentTreeViewItem.DataContext<GameEventTriggerModel>().EventType = EventType.Image;
                if (CurrentTreeViewItem.DataContext<GameEventTriggerModel>().MouseTriggerInfo.MouseInfoEventType != MouseEventType.None)
                {
                    CurrentTreeViewItem.DataContext<GameEventTriggerModel>().MouseTriggerInfo = new MouseTriggerInfo();
                }
            }
            else if (sender.Equals(rbRelativeToImage))
            {
                CurrentTreeViewItem.DataContext<GameEventTriggerModel>().EventType = EventType.RelativeToImage;
                if (CurrentTreeViewItem.DataContext<GameEventTriggerModel>().MouseTriggerInfo.MouseInfoEventType != MouseEventType.None)
                {
                    CurrentTreeViewItem.DataContext<GameEventTriggerModel>().MouseTriggerInfo = new MouseTriggerInfo();
                }
                RelativePosition.X = CurrentTreeViewItem.DataContext<GameEventTriggerModel>().MouseTriggerInfo.StartPoint.X;
                RelativePosition.Y = CurrentTreeViewItem.DataContext<GameEventTriggerModel>().MouseTriggerInfo.StartPoint.Y;
            }
            RadioButtonRefresh();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(btnMouseCoordinate))
            {
                //lblWheelData.Visibility = Visibility.Collapsed;
                //gridWheelData.Visibility = Visibility.Collapsed;
                if (CurrentTreeViewItem.DataContext<GameEventTriggerModel>().MouseTriggerInfo.MouseInfoEventType != MouseEventType.None)
                {
                    CurrentTreeViewItem.DataContext<GameEventTriggerModel>().MouseTriggerInfo = new MouseTriggerInfo();
                }
                ShowMousePoisitionView();
            }
            else if (sender.Equals(btnTreeItemUp) || sender.Equals(btnTreeItemDown))
            {
                if (CurrentTreeViewItem == null)
                    return;
                var itemContainer = CurrentTreeViewItem.ParentItem == null ? this.DataContext<GameEventConfigViewModel>().TriggerSaves : CurrentTreeViewItem.ParentItem.DataContext<GameEventTriggerModel>().SubEventTriggers;
                var currentIndex = itemContainer.IndexOf(CurrentTreeViewItem.DataContext<GameEventTriggerModel>());

                if (currentIndex > 0 && sender.Equals(btnTreeItemUp))
                {
                    itemContainer.Swap(currentIndex, currentIndex - 1);
                    CurrentTreeViewItem = treeSaves.GetSelectItemFromObject<TreeGridViewItem>(itemContainer[currentIndex - 1]) ?? _dummyTreeGridViewItem;

                    NotifyHelper.InvokeNotify(NotifyEventType.EventTriggerOrderChanged, new EventTriggerOrderChangedEventArgs()
                    {
                        TriggerModel1 = itemContainer[currentIndex],
                        TriggerModel2 = itemContainer[currentIndex - 1]
                    });
                }
                else if (currentIndex < itemContainer.Count - 1 && sender.Equals(btnTreeItemDown))
                {
                    itemContainer.Swap(currentIndex, currentIndex + 1);
                    CurrentTreeViewItem = treeSaves.GetSelectItemFromObject<TreeGridViewItem>(itemContainer[currentIndex + 1]) ?? _dummyTreeGridViewItem;

                    NotifyHelper.InvokeNotify(NotifyEventType.EventTriggerOrderChanged, new EventTriggerOrderChangedEventArgs()
                    {
                        TriggerModel1 = itemContainer[currentIndex],
                        TriggerModel2 = itemContainer[currentIndex + 1]
                    });
                }
            }
            //else if(sender.Equals(btnMouseWheel))
            //{
            //    lblWheelData.Visibility = Visibility.Visible;
            //    gridWheelData.Visibility = Visibility.Visible;
            //    CurrentTreeViewItem.DataContext<EventTriggerModel>().MouseTriggerInfo.MouseInfoEventType = MouseEventType.Wheel;
            //}
            //else if(sender.Equals(btnWheelCancel))
            //{
            //    lblWheelData.Visibility = Visibility.Collapsed;
            //    gridWheelData.Visibility = Visibility.Collapsed;
            //    CurrentTreeViewItem.DataContext<EventTriggerModel>().MouseTriggerInfo = new MouseTriggerInfo()
            //    {
            //        WheelData = 0,
            //        MouseInfoEventType = MouseEventType.LeftClick,
            //        EndPoint = CurrentTreeViewItem.DataContext<EventTriggerModel>().MouseTriggerInfo.EndPoint,
            //        MiddlePoint = CurrentTreeViewItem.DataContext<EventTriggerModel>().MouseTriggerInfo.MiddlePoint,
            //        StartPoint = CurrentTreeViewItem.DataContext<EventTriggerModel>().MouseTriggerInfo.StartPoint
            //    };
            //}
        }
    }
}
