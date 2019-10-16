using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Impl;
using Macro.Models;
using Macro.Models.ViewModel;
using Macro.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Utils;

namespace Macro.View
{
    public partial class CommonEventConfigView : BaseEventConfigView<CommonEventConfigViewModel>
    {
        public TreeGridViewItem CurrentTreeViewItem
        {
            get => _contextViewModel.CurrentTreeViewItem;
            private set => _contextViewModel.CurrentTreeViewItem = value;
        }
        public List<EventTriggerModel> TriggerSaves
        {
            get => _contextViewModel.TriggerSaves.ToList();
        }
        public PointModel RelativePosition
        {
            get => _contextViewModel.RelativePosition;
            private set => _contextViewModel.RelativePosition = value;
        }

        private readonly TreeGridViewItem _dummyTreeGridViewItem;

        private readonly PointModel _dummyRelativePosition;

        private readonly CommonEventConfigViewModel _contextViewModel;

        public CommonEventConfigView()
        {
            _contextViewModel = new ViewModelLocator().CommonEventConfigViewModel;
            _isDrag = false;
            _dummyTreeGridViewItem = new TreeGridViewItem()
            {
                DataContext = new EventTriggerModel()
            };
            _dummyRelativePosition = new PointModel();

            _contextViewModel.CurrentTreeViewItem = _dummyTreeGridViewItem;

            _contextViewModel.RelativePosition = _dummyRelativePosition;

            DataContext = _contextViewModel;

            InitializeComponent();

            InitEvent();

            Init();
        }

        private void Init()
        {
            treeSaves.ItemsSource = _contextViewModel.TriggerSaves;

            foreach (var type in Enum.GetValues(typeof(RepeatType)))
            {
                if(Enum.TryParse($"Repeat{type.ToString()}", out Utils.Document.Label label))
                {
                    _repeatItems.Add(new KeyValuePair<RepeatType, string>((RepeatType)type, DocumentHelper.Get(label)));
                }
            }

            foreach (var item in _mousePointViews)
            {
                item.SettingViewMode(MousePointViewMode.Common);
            }

            comboRepeatSubItem.ItemsSource = _repeatItems;
            comboRepeatSubItem.DisplayMemberPath = "Value";
            comboRepeatSubItem.SelectedValuePath = "Key";
        }

        public void BindingItems(IEnumerable<EventTriggerModel> items)
        {
            foreach (var item in items)
            {
                _contextViewModel.TriggerSaves.Add(item);
            }
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
            {
                return;
            }

            Dispatcher.Invoke(() =>
            {
                var treeVIewItem = treeSaves.GetSelectItemFromObject<TreeGridViewItem>(CurrentTreeViewItem.DataContext<EventTriggerModel>());

                if (treeVIewItem == null)
                {
                    var model = CurrentTreeViewItem.DataContext<EventTriggerModel>();
                    _contextViewModel.TriggerSaves.Add(model);
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
            {
                return;
            }

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
