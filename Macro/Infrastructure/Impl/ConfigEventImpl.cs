using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Models;
using Macro.Models.ViewModel;
using Macro.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Utils;
using EventType = Macro.Models.EventType;

namespace Macro.View
{
    public partial class ConfigEventView : UserControl
    {
        public TreeGridViewItem CurrentTreeViewItem
        {
            get => this.DataContext<ConfigEventViewModel>().CurrentTreeViewItem;
            private set => this.DataContext<ConfigEventViewModel>().CurrentTreeViewItem = value;
        }
        public List<EventTriggerModel> TriggerSaves
        {
            get => this.DataContext<ConfigEventViewModel>().TriggerSaves.ToList();
        }
        public PointModel RelativePosition
        {
            get => this.DataContext<ConfigEventViewModel>().RelativePosition;
            private set => this.DataContext<ConfigEventViewModel>().RelativePosition = value;
        }

        private List<MousePositionView> _mousePointViews;
        private TreeGridViewItem _dummyTreeGridViewItem;
        private PointModel _dummyRelativePosition;
        private bool _isDrag;
        private ObservableCollection<KeyValuePair<RepeatType, string>> _repeatItems;
        public ConfigEventView()
        {
            _isDrag = false;
            _dummyTreeGridViewItem = new TreeGridViewItem()
            {
                DataContext = new EventTriggerModel()
            };
            _dummyRelativePosition = new PointModel();
            _repeatItems = new ObservableCollection<KeyValuePair<RepeatType, string>>();
            _mousePointViews = new List<MousePositionView>();
            DataContext = new ViewModelLocator().ConfigEventViewModel;
            CurrentTreeViewItem = _dummyTreeGridViewItem;
            RelativePosition = _dummyRelativePosition;
            
            InitializeComponent();

            Loaded += ConfigEventView_Loaded;
        }

        private void Init()
        {
            treeSaves.ItemsSource = this.DataContext<ConfigEventViewModel>().TriggerSaves;
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

        public void ShowMousePoisitionView()
        {
            foreach (var item in _mousePointViews)
                item.ShowActivate();
        }
        public void BindingItems(IEnumerable<EventTriggerModel> items)
        {
            foreach (var item in items)
                this.DataContext<ConfigEventViewModel>().TriggerSaves.Add(item);
        }
        public void CopyAndInsertCurrentItem()
        {
            if (CurrentTreeViewItem == _dummyTreeGridViewItem)
                return;
            Dispatcher.Invoke(() =>
            {
                var treeVIewItem = treeSaves.GetSelectItemFromObject<TreeGridViewItem>(CurrentTreeViewItem.DataContext<EventTriggerModel>());
                if (treeVIewItem != null)
                {
                    var item = new EventTriggerModel(treeVIewItem.DataContext<EventTriggerModel>());
                    this.DataContext<ConfigEventViewModel>().TriggerSaves.Add(item);
                }
                Clear();
            });
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
                    this.DataContext<ConfigEventViewModel>().TriggerSaves.Add(CurrentTreeViewItem.DataContext<EventTriggerModel>());
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
                if (CurrentTreeViewItem.ParentItem == null)
                {
                    this.DataContext<ConfigEventViewModel>().TriggerSaves.Remove(CurrentTreeViewItem.DataContext<EventTriggerModel>());
                }
                else
                {
                    CurrentTreeViewItem.ParentItem.DataContext<EventTriggerModel>().SubEventTriggers.Remove(CurrentTreeViewItem.DataContext<EventTriggerModel>());
                }
            });
        }
        public void Clear()
        {
            CurrentTreeViewItem.IsSelected = false;
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
            var parentItemContainer = CurrentTreeViewItem.ParentItem == null ? this.DataContext<ConfigEventViewModel>().TriggerSaves : CurrentTreeViewItem.ParentItem.DataContext<EventTriggerModel>().SubEventTriggers;

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
                this.DataContext<ConfigEventViewModel>().TriggerSaves.Add(item);
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

                lblWheelData.Visibility = Visibility.Collapsed;
                gridWheelData.Visibility = Visibility.Collapsed;
            }
            else if(CurrentTreeViewItem.DataContext<EventTriggerModel>().EventType == EventType.RelativeToImage)
            {
                gridRelative.Visibility = Visibility.Visible;
                txtKeyboardCmd.Visibility = Visibility.Collapsed;

                btnMouseCoordinate.Visibility = Visibility.Collapsed;
                btnMouseCoordinate.IsEnabled = false;
                //btnMouseWheel.Visibility = Visibility.Collapsed;
                //btnMouseWheel.IsEnabled = false;

                lblWheelData.Visibility = Visibility.Collapsed;
                gridWheelData.Visibility = Visibility.Collapsed;
            }
            else
            {
                gridRelative.Visibility = Visibility.Collapsed;
                txtKeyboardCmd.Visibility = Visibility.Collapsed;

                btnMouseCoordinate.Visibility = Visibility.Visible;
                btnMouseCoordinate.IsEnabled = false;
                //btnMouseWheel.Visibility = Visibility.Visible;
                //btnMouseWheel.IsEnabled = false;

                lblWheelData.Visibility = Visibility.Collapsed;
                gridWheelData.Visibility = Visibility.Collapsed;
            }
        }
    }
}
