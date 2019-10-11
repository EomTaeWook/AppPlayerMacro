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

namespace Macro.View
{
    /// <summary>
    /// GameEventConfigView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GameEventConfigView : BaseEventConfigView<GameEventConfigViewModel>
    {
        private void InitEvent()
        {
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

            NotifyHelper.MousePositionDataBind += NotifyHelper_MousePositionDataBind;
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
