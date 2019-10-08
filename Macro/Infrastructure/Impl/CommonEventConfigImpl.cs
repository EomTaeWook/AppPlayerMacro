using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Impl;
using Macro.Models;
using Macro.Models.ViewModel;
using Macro.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Utils;

namespace Macro.View
{
    public partial class CommonEventConfigView : BaseEventConfigView<CommonEventConfigViewModel>
    {
        public TreeGridViewItem CurrentTreeViewItem
        {
            get => this.DataContext<CommonEventConfigViewModel>().CurrentTreeViewItem;
            private set => this.DataContext<CommonEventConfigViewModel>().CurrentTreeViewItem = value;
        }
        public List<EventTriggerModel> TriggerSaves
        {
            get => this.DataContext<CommonEventConfigViewModel>().TriggerSaves.ToList();
        }
        public PointModel RelativePosition
        {
            get => this.DataContext<CommonEventConfigViewModel>().RelativePosition;
            private set => this.DataContext<CommonEventConfigViewModel>().RelativePosition = value;
        }

        private readonly TreeGridViewItem _dummyTreeGridViewItem;
        private readonly PointModel _dummyRelativePosition;

        public CommonEventConfigView()
        {
            _isDrag = false;
            _dummyTreeGridViewItem = new TreeGridViewItem()
            {
                DataContext = new EventTriggerModel()
            };
            _dummyRelativePosition = new PointModel();

            DataContext = new ViewModelLocator().CommonEventConfigViewModel;
            CurrentTreeViewItem = _dummyTreeGridViewItem;
            RelativePosition = _dummyRelativePosition;
            
            InitializeComponent();

            InitEvent();
            Init();
        }

        private void Init()
        {
            treeSaves.ItemsSource = this.DataContext<CommonEventConfigViewModel>().TriggerSaves;
            foreach (var item in DisplayHelper.MonitorInfo())
            {
                _mousePointViews.Add(new MousePositionView(item));
            }
            foreach(var type in Enum.GetValues(typeof(RepeatType)))
            {
                if(Enum.TryParse($"Repeat{type.ToString()}", out Utils.Document.Label label))
                {
                    _repeatItems.Add(new KeyValuePair<RepeatType, string>((RepeatType)type, DocumentHelper.Get(label)));
                }
            }
            comboRepeatSubItem.ItemsSource = _repeatItems;
            comboRepeatSubItem.DisplayMemberPath = "Value";
            comboRepeatSubItem.SelectedValuePath = "Key";
        }

        public void BindingItems(IEnumerable<EventTriggerModel> items)
        {
            foreach (var item in items)
                this.DataContext<CommonEventConfigViewModel>().TriggerSaves.Add(item);
        }
        public TreeGridViewItem CopyCurrentItem()
        {
            if (CurrentTreeViewItem == _dummyTreeGridViewItem)
                return null;
            Dispatcher.Invoke(() =>
            {
                var treeVIewItem = treeSaves.GetSelectItemFromObject<TreeGridViewItem>(CurrentTreeViewItem.DataContext<EventTriggerModel>());
                if (treeVIewItem != null)
                {
                    CurrentTreeViewItem = new TreeGridViewItem()
                    {
                        DataContext = new EventTriggerModel(treeVIewItem.DataContext<EventTriggerModel>())
                    };
                }
            });
            return CurrentTreeViewItem;
        }
        public void InsertCurrentItem()
        {
            if (CurrentTreeViewItem == _dummyTreeGridViewItem)
                return;

            Dispatcher.Invoke(() =>
            {
                var treeVIewItem = treeSaves.GetSelectItemFromObject<TreeGridViewItem>(CurrentTreeViewItem.DataContext<EventTriggerModel>());

                if (treeVIewItem == null)
                {
                    var model = CurrentTreeViewItem.DataContext<EventTriggerModel>();
                    this.DataContext<CommonEventConfigViewModel>().TriggerSaves.Add(model);
                    NotifyHelper.InvokeNotify(NotifyEventType.EventTriggerInserted, new EventTriggerEventArgs()
                    {
                        Index = model.TriggerIndex,
                        TriggerModel = model
                    });
                }
                Clear();
            });
        }
        public void CurrentRemove()
        {
            if (CurrentTreeViewItem == _dummyTreeGridViewItem)
                return;
            Dispatcher.Invoke(() =>
            {
                var model = CurrentTreeViewItem.DataContext<EventTriggerModel>();

                if (CurrentTreeViewItem.ParentItem == null)
                {
                    this.DataContext<CommonEventConfigViewModel>().TriggerSaves.Remove(CurrentTreeViewItem.DataContext<EventTriggerModel>());
                }
                else
                {
                    CurrentTreeViewItem.ParentItem.DataContext<EventTriggerModel>().SubEventTriggers.Remove(CurrentTreeViewItem.DataContext<EventTriggerModel>());
                }
                NotifyHelper.InvokeNotify(NotifyEventType.EventTriggerRemoved, new EventTriggerEventArgs()
                {
                    Index = model.TriggerIndex,
                    TriggerModel = model
                });
            });
        }
        public void Clear()
        {
            CurrentTreeViewItem.IsSelected = false;
            _dummyTreeGridViewItem.DataContext<EventTriggerModel>().Clear();

            if (CurrentTreeViewItem != _dummyTreeGridViewItem)
            {
                CurrentTreeViewItem = _dummyTreeGridViewItem;
            }
            if(RelativePosition != _dummyRelativePosition)
            {
                RelativePosition = _dummyRelativePosition;
            }
            RadioButtonRefresh();
            btnTreeItemUp.Visibility = btnTreeItemDown.Visibility = Visibility.Hidden;
            lblRepeatSubItems.Visibility = Visibility.Collapsed;
            gridRepeat.Visibility = Visibility.Collapsed;
        }
        
        private void ItemContainerPositionChange(TreeGridViewItem target)
        {
            var parentItemContainer = CurrentTreeViewItem.ParentItem == null ? this.DataContext<CommonEventConfigViewModel>().TriggerSaves : CurrentTreeViewItem.ParentItem.DataContext<EventTriggerModel>().SubEventTriggers;

            if (target != null)
            {
                if (target == CurrentTreeViewItem)
                    return;
                var item = CurrentTreeViewItem.DataContext<EventTriggerModel>();
                var targetItem = target.DataContext<EventTriggerModel>();

                if (target.ParentItem == null && CurrentTreeViewItem.ParentItem == null)
                {
                    parentItemContainer.Remove(item);
                    targetItem.SubEventTriggers.Add(item);
                }
                else if (target.ParentItem != CurrentTreeViewItem)
                {
                    parentItemContainer.Remove(item);
                    targetItem.SubEventTriggers.Add(item);
                }
                else if(target.ParentItem == CurrentTreeViewItem)
                {
                    parentItemContainer.Remove(item);
                    item.SubEventTriggers.Remove(targetItem);
                    var targetSubItem = targetItem.SubEventTriggers;
                    targetItem.SubEventTriggers = item.SubEventTriggers;
                    item.SubEventTriggers = targetSubItem;
                    targetItem.SubEventTriggers.Add(item);
                    parentItemContainer.Add(targetItem);
                    CurrentTreeViewItem = _dummyTreeGridViewItem;
                }
            }
            else
            {
                var item = CurrentTreeViewItem.DataContext<EventTriggerModel>();
                parentItemContainer.Remove(item);
                this.DataContext<CommonEventConfigViewModel>().TriggerSaves.Add(item);
            }
        }
        private void RadioButtonRefresh()
        {
            if (CurrentTreeViewItem.DataContext<EventTriggerModel>().EventType == EventType.Mouse)
            {
                txtKeyboardCmd.Visibility = Visibility.Collapsed;
                gridRelative.Visibility = Visibility.Collapsed;

                btnMouseCoordinate.Visibility = Visibility.Visible;
                btnMouseCoordinate.IsEnabled = true;

                //btnMouseWheel.Visibility = Visibility.Visible;
                //btnMouseWheel.IsEnabled = false;
            }
            else if(CurrentTreeViewItem.DataContext<EventTriggerModel>().EventType == EventType.Keyboard)
            {
                txtKeyboardCmd.Visibility = Visibility.Visible;
                gridRelative.Visibility = Visibility.Collapsed;

                btnMouseCoordinate.Visibility = Visibility.Collapsed;
                btnMouseCoordinate.IsEnabled = false;
                //btnMouseWheel.Visibility = Visibility.Collapsed;
                //btnMouseWheel.IsEnabled = false;

                //lblWheelData.Visibility = Visibility.Collapsed;
                //gridWheelData.Visibility = Visibility.Collapsed;
            }
            else if(CurrentTreeViewItem.DataContext<EventTriggerModel>().EventType == EventType.RelativeToImage)
            {
                gridRelative.Visibility = Visibility.Visible;
                txtKeyboardCmd.Visibility = Visibility.Collapsed;

                btnMouseCoordinate.Visibility = Visibility.Collapsed;
                btnMouseCoordinate.IsEnabled = false;
                //btnMouseWheel.Visibility = Visibility.Collapsed;
                //btnMouseWheel.IsEnabled = false;

                //lblWheelData.Visibility = Visibility.Collapsed;
                //gridWheelData.Visibility = Visibility.Collapsed;
            }
            else
            {
                gridRelative.Visibility = Visibility.Collapsed;
                txtKeyboardCmd.Visibility = Visibility.Collapsed;

                btnMouseCoordinate.Visibility = Visibility.Visible;
                btnMouseCoordinate.IsEnabled = false;
                //btnMouseWheel.Visibility = Visibility.Visible;
                //btnMouseWheel.IsEnabled = false;

                //lblWheelData.Visibility = Visibility.Collapsed;
                //gridWheelData.Visibility = Visibility.Collapsed;
            }
        }
    }
}
