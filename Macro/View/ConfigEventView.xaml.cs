using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Models;
using Macro.Models.ViewModel;
using Macro.UI;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
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
            NotifyHelper.ConfigChanged += NotifyHelper_ConfigChanged;

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

            PreviewKeyDown += ConfigEventView_PreviewKeyDown;

            treeSaves.SelectedItemChanged += TreeSaves_SelectedItemChanged;
            treeSaves.PreviewMouseLeftButtonDown += TreeSaves_PreviewMouseLeftButtonDown;
            treeSaves.MouseMove += TreeSaves_MouseMove;
            treeSaves.Drop += TreeSaves_Drop;

            comboRepeatSubItem.SelectionChanged += ComboRepeatSubItem_SelectionChanged;

            Unloaded += ConfigEventView_Unloaded;
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

        private void ComboRepeatSubItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender.Equals(comboRepeatSubItem) && comboRepeatSubItem.SelectedItem is KeyValuePair<RepeatType, string> item)
            {
                if(item.Key == RepeatType.Count)
                {
                    numRepeatCount.Visibility = Visibility.Visible;
                }
                else
                {
                    numRepeatCount.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(btnMouseCoordinate))
            {
                ShowMousePoisitionView();
            }
            else if (sender.Equals(btnTreeItemUp) || sender.Equals(btnTreeItemDown))
            {
                if (CurrentTreeViewItem == null)
                    return;
                var itemContainer = CurrentTreeViewItem.ParentItem == null ? this.DataContext<ConfigEventViewModel>().TriggerSaves : CurrentTreeViewItem.ParentItem.DataContext<EventTriggerModel>().SubEventTriggers;
                var currentIndex = itemContainer.IndexOf(CurrentTreeViewItem.DataContext<EventTriggerModel>());

                if (currentIndex > 0 && sender.Equals(btnTreeItemUp))
                {
                    itemContainer.Swap(currentIndex, currentIndex - 1);
                    CurrentTreeViewItem = treeSaves.GetSelectItemFromObject<TreeGridViewItem>(itemContainer[currentIndex - 1]) ?? _dummyTreeGridViewItem;
                }
                else if(currentIndex < itemContainer.Count - 1 && sender.Equals(btnTreeItemDown))
                {
                    itemContainer.Swap(currentIndex, currentIndex + 1);
                    CurrentTreeViewItem = treeSaves.GetSelectItemFromObject<TreeGridViewItem>(itemContainer[currentIndex + 1]) ?? _dummyTreeGridViewItem;
                }
            }
        }

        private void TreeSaves_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(treeSaves.SelectedItem is EventTriggerModel item)
            {
                CurrentTreeViewItem = treeSaves.GetSelectItemFromObject<TreeGridViewItem>(treeSaves.SelectedItem) ?? _dummyTreeGridViewItem;
                NotifyHelper.InvokeNotify(Infrastructure.EventType.SelctTreeViewItemChanged, new SelctTreeViewItemChangedEventArgs()
                {
                    TreeViewItem = CurrentTreeViewItem
                });

                if (CurrentTreeViewItem.DataContext<EventTriggerModel>().EventType == EventType.Keyboard)
                {
                    RadioButton_Click(rbKeyboard, null);
                }
                else if (CurrentTreeViewItem.DataContext<EventTriggerModel>().EventType == EventType.Mouse)
                {
                    RadioButton_Click(rbMouse, null);
                }
                else if (CurrentTreeViewItem.DataContext<EventTriggerModel>().EventType == EventType.Image)
                {
                    RadioButton_Click(rbImage, null);
                }
                else if(CurrentTreeViewItem.DataContext<EventTriggerModel>().EventType == EventType.RelativeToImage)
                {
                    RadioButton_Click(rbRelativeToImage, null);
                }
                btnTreeItemUp.Visibility = btnTreeItemDown.Visibility = Visibility.Visible;
                if(item.SubEventTriggers.Count != 0)
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

        private void TreeSaves_Drop(object sender, DragEventArgs e)
        {
            if(_isDrag)
            {
                _isDrag = false;
                var targetRow = treeSaves.TryFindFromPoint<TreeGridViewItem>(e.GetPosition(treeSaves));
                if (CurrentTreeViewItem == targetRow)
                    return;
                ItemContainerPositionChange(targetRow);
                var item = CurrentTreeViewItem.DataContext<EventTriggerModel>();
                Clear();
                NotifyHelper.InvokeNotify(Infrastructure.EventType.EventTriggerOrderChanged, new EventTriggerOrderChangedEventArgs()
                {
                    TriggerModel1 = item,
                    TriggerModel2 = targetRow?.DataContext<EventTriggerModel>()
                });
                
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
                CurrentTreeViewItem = target;

                DragDrop.DoDragDrop(CurrentTreeViewItem, new object(), DragDropEffects.Move);
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
                NotifyHelper.InvokeNotify(Infrastructure.EventType.SelctTreeViewItemChanged, new SelctTreeViewItemChangedEventArgs());
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }

        private void NotifyHelper_MousePositionDataBind(MousePointEventArgs args)
        {
            if (CurrentTreeViewItem == _dummyTreeGridViewItem)
            {
                CurrentTreeViewItem = new TreeGridViewItem()
                {
                    DataContext = new EventTriggerModel()
                };
            }
            CurrentTreeViewItem.DataContext<EventTriggerModel>().MonitorInfo = args.MonitorInfo;
            CurrentTreeViewItem.DataContext<EventTriggerModel>().MouseTriggerInfo = args.MouseTriggerInfo;
            foreach (var item in _mousePointViews)
            {
                item.Hide();
            }
        }

        private void NotifyHelper_ScreenCaptureDataBind(CaptureEventArgs args)
        {
            if (CurrentTreeViewItem == _dummyTreeGridViewItem)
            {
                CurrentTreeViewItem = new TreeGridViewItem()
                {
                    DataContext = new EventTriggerModel()
                };
            }
            if(RelativePosition == _dummyRelativePosition)
            {
                RelativePosition = new PointModel();
            }
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

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentTreeViewItem == _dummyTreeGridViewItem)
            {
                CurrentTreeViewItem = new TreeGridViewItem()
                {
                    DataContext = new EventTriggerModel()
                };
            }
            if(RelativePosition == _dummyRelativePosition)
            {
                RelativePosition = new PointModel();
            }

            if (sender.Equals(rbMouse))
            {
                CurrentTreeViewItem.DataContext<EventTriggerModel>().EventType = EventType.Mouse;
            }
            else if (sender.Equals(rbKeyboard))
            {
                CurrentTreeViewItem.DataContext<EventTriggerModel>().EventType = EventType.Keyboard;
            }
            else if(sender.Equals(rbImage))
            {
                CurrentTreeViewItem.DataContext<EventTriggerModel>().EventType = EventType.Image;
            }
            else if(sender.Equals(rbRelativeToImage))
            {
                CurrentTreeViewItem.DataContext<EventTriggerModel>().EventType = EventType.RelativeToImage;
                if(CurrentTreeViewItem.DataContext<EventTriggerModel>().MouseTriggerInfo.MouseInfoEventType != MouseEventType.None)
                {
                    CurrentTreeViewItem.DataContext<EventTriggerModel>().MouseTriggerInfo = new MouseTriggerInfo();
                }
                RelativePosition.X = CurrentTreeViewItem.DataContext<EventTriggerModel>().MouseTriggerInfo.StartPoint.X;
                RelativePosition.Y = CurrentTreeViewItem.DataContext<EventTriggerModel>().MouseTriggerInfo.StartPoint.Y;
            }
            RadioButtonRefresh();
        }        
    }
}
