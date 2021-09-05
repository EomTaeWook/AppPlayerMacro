using KosherUtils.Log;
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
    public partial class GameEventConfigView : BaseEventConfigView<GameEventConfigViewModel>
    {
        public TreeGridViewItem CurrentTreeViewItem
        {
            get => _contextViewModel.CurrentTreeViewItem;
            protected set => _contextViewModel.CurrentTreeViewItem = value;
        }
        public PointModel RelativePosition
        {
            get => _contextViewModel.RelativePosition;
            protected set => _contextViewModel.RelativePosition = value;
        }
        public List<GameEventTriggerModel> TriggerSaves
        {
            get => _contextViewModel.TriggerSaves.ToList();
        }

        private TreeGridViewItem _dummyTreeGridViewItem;
        private PointModel _dummyRelativePosition;
        private ValueConditionModel _dummyHpCondition;
        private ValueConditionModel _dummyMpCondition;
        private ObservableCollection<KeyValuePair<ConditionType, string>> _conditionItems;
        private GameEventConfigViewModel _contextViewModel;
        public GameEventConfigView()
        {
            _dummyTreeGridViewItem = new TreeGridViewItem()
            {
                DataContext = new GameEventTriggerModel()
            };
            _dummyRelativePosition = new PointModel();

            _dummyHpCondition = new ValueConditionModel();

            _dummyMpCondition = new ValueConditionModel();

            _conditionItems = new ObservableCollection<KeyValuePair<ConditionType, string>>();

            InitializeComponent();

            InitEvent();

            Init();

            if (InitDataContext() == false)
            {
                Log.Error(new Exception("Failed Init DataContext"));
            }
        }
        private bool InitDataContext()
        {
            var resource = this.TryFindResource("Locator");
            if (resource == null)
            {
                return false;
            }

            _contextViewModel = (resource as ViewModelLocator).GameEventConfigViewModel;

            _contextViewModel.HpCondition = _dummyHpCondition;

            _contextViewModel.MpCondition = _dummyMpCondition;

            _contextViewModel.CurrentTreeViewItem = _dummyTreeGridViewItem;

            _contextViewModel.RelativePosition = _dummyRelativePosition;

            DataContext = _contextViewModel;

            return true;
        }
        private void Init()
        {
            foreach (var type in Enum.GetValues(typeof(ConditionType)))
            {
                if ((ConditionType)type == ConditionType.Max)
                {
                    continue;
                }
                if (Enum.TryParse(type.ToString(), out Utils.Document.Label label))
                {
                    _conditionItems.Add(new KeyValuePair<ConditionType, string>((ConditionType)type, DocumentHelper.Get(label)));
                }
            }

            foreach (var item in _mousePointViews)
            {
                item.SettingViewMode(MousePointViewMode.Game);
            }

            comboHpCondition.ItemsSource = _conditionItems;
            comboHpCondition.DisplayMemberPath = "Value";
            comboHpCondition.SelectedValuePath = "Key";

            comboMpCondition.ItemsSource = _conditionItems;
            comboMpCondition.DisplayMemberPath = "Value";
            comboMpCondition.SelectedValuePath = "Key";

            _dummyHpCondition.ConditionType = ConditionType.Below;

            _dummyMpCondition.ConditionType = ConditionType.Below;
        }
        public void Clear()
        {
            CurrentTreeViewItem.IsSelected = false;
            _dummyTreeGridViewItem.DataContext<GameEventTriggerModel>().Clear();

            if (CurrentTreeViewItem != _dummyTreeGridViewItem)
            {
                CurrentTreeViewItem = _dummyTreeGridViewItem;
            }
            if (RelativePosition != _dummyRelativePosition)
            {
                RelativePosition = _dummyRelativePosition;
            }
            RadioButtonRefresh();
            btnTreeItemUp.Visibility = btnTreeItemDown.Visibility = Visibility.Hidden;
            lblRepeatSubItems.Visibility = Visibility.Collapsed;
            gridRepeat.Visibility = Visibility.Collapsed;
            checkImageSearchRequired.Visibility = lblImageSearchRequired.Visibility = Visibility.Collapsed;
            checkImageSearchRequired.IsChecked = true;
        }
        public GameEventTriggerModel CopyCurrentItem()
        {
            if (CurrentTreeViewItem == _dummyTreeGridViewItem)
                return null;

            Dispatcher.Invoke(() =>
            {
                var selectModel = CurrentTreeViewItem.DataContext<GameEventTriggerModel>();
                CurrentTreeViewItem = new TreeGridViewItem()
                {
                    DataContext = new GameEventTriggerModel(selectModel)
                };
            });
            return CurrentTreeViewItem.DataContext<GameEventTriggerModel>();
        }
        public void InsertCurrentItem()
        {
            if (CurrentTreeViewItem == _dummyTreeGridViewItem)
            {
                return;
            }

            Dispatcher.Invoke(() =>
            {
                var treeVIewItem = treeSaves.GetSelectItemFromObject<TreeGridViewItem>(CurrentTreeViewItem.DataContext<GameEventTriggerModel>());

                if (treeVIewItem == null)
                {
                    var model = CurrentTreeViewItem.DataContext<GameEventTriggerModel>();
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
        public void RemoveCurrentItem()
        {
            if (CurrentTreeViewItem == _dummyTreeGridViewItem)
            {
                return;
            }

            Dispatcher.Invoke(() =>
            {
                var model = CurrentTreeViewItem.DataContext<GameEventTriggerModel>();

                if (CurrentTreeViewItem.ParentItem == null)
                {
                    this.DataContext<GameEventConfigViewModel>().TriggerSaves.Remove(CurrentTreeViewItem.DataContext<GameEventTriggerModel>());
                }
                else
                {
                    CurrentTreeViewItem.ParentItem.DataContext<GameEventTriggerModel>().SubEventTriggers.Remove(CurrentTreeViewItem.DataContext<GameEventTriggerModel>());
                }
                NotifyHelper.InvokeNotify(NotifyEventType.EventTriggerRemoved, new EventTriggerEventArgs()
                {
                    Index = model.TriggerIndex,
                    TriggerModel = model
                });
            });
        }
        private void ItemContainerPositionChange(TreeGridViewItem target)
        {
            var parentItemContainer = CurrentTreeViewItem.ParentItem == null ? this.DataContext<GameEventConfigViewModel>().TriggerSaves : CurrentTreeViewItem.ParentItem.DataContext<GameEventTriggerModel>().SubEventTriggers;

            if (target != null)
            {
                if (target == CurrentTreeViewItem)
                    return;
                var item = CurrentTreeViewItem.DataContext<GameEventTriggerModel>();
                var targetItem = target.DataContext<GameEventTriggerModel>();

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
                else if (target.ParentItem == CurrentTreeViewItem)
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
                var item = CurrentTreeViewItem.DataContext<GameEventTriggerModel>();
                parentItemContainer.Remove(item);
                this.DataContext<GameEventConfigViewModel>().TriggerSaves.Add(item);
            }
        }
        private void RadioButtonRefresh()
        {
            if (CurrentTreeViewItem.DataContext<GameEventTriggerModel>().EventType == EventType.Mouse)
            {
                txtKeyboardCmd.Visibility = Visibility.Collapsed;
                gridRelative.Visibility = Visibility.Collapsed;

                btnMouseCoordinate.Visibility = Visibility.Visible;
                btnMouseCoordinate.IsEnabled = true;

                checkImageSearchRequired.Visibility = lblImageSearchRequired.Visibility = Visibility.Visible;

                checkSameImageDrag.Visibility = Visibility.Collapsed;
                checkSameImageDrag.IsChecked = false;
                numMaxSameImageCount.Visibility = Visibility.Collapsed;

                //btnMouseWheel.Visibility = Visibility.Visible;
                //btnMouseWheel.IsEnabled = false;
            }
            else if (CurrentTreeViewItem.DataContext<GameEventTriggerModel>().EventType == EventType.Keyboard)
            {
                txtKeyboardCmd.Visibility = Visibility.Visible;
                gridRelative.Visibility = Visibility.Collapsed;

                btnMouseCoordinate.Visibility = Visibility.Collapsed;
                btnMouseCoordinate.IsEnabled = false;

                checkImageSearchRequired.Visibility = lblImageSearchRequired.Visibility = Visibility.Visible;

                checkSameImageDrag.Visibility = Visibility.Collapsed;
                checkSameImageDrag.IsChecked = false;
                numMaxSameImageCount.Visibility = Visibility.Collapsed;

                //btnMouseWheel.Visibility = Visibility.Collapsed;
                //btnMouseWheel.IsEnabled = false;

                //lblWheelData.Visibility = Visibility.Collapsed;
                //gridWheelData.Visibility = Visibility.Collapsed;
            }
            else if (CurrentTreeViewItem.DataContext<GameEventTriggerModel>().EventType == EventType.RelativeToImage)
            {
                gridRelative.Visibility = Visibility.Visible;
                txtKeyboardCmd.Visibility = Visibility.Collapsed;

                btnMouseCoordinate.Visibility = Visibility.Collapsed;
                btnMouseCoordinate.IsEnabled = false;

                checkSameImageDrag.Visibility = Visibility.Collapsed;
                checkSameImageDrag.IsChecked = false;
                numMaxSameImageCount.Visibility = Visibility.Collapsed;
                //btnMouseWheel.Visibility = Visibility.Collapsed;
                //btnMouseWheel.IsEnabled = false;

                //lblWheelData.Visibility = Visibility.Collapsed;
                //gridWheelData.Visibility = Visibility.Collapsed;
            }
            else if (CurrentTreeViewItem.DataContext<GameEventTriggerModel>().EventType == EventType.Image)
            {
                checkSameImageDrag.Visibility = Visibility.Visible;
                checkSameImageDrag.IsChecked = false;
                numMaxSameImageCount.Visibility = Visibility.Collapsed;

                btnMouseCoordinate.Visibility = Visibility.Collapsed;
                btnMouseCoordinate.IsEnabled = false;

                txtKeyboardCmd.Visibility = Visibility.Collapsed;
                gridRelative.Visibility = Visibility.Collapsed;
            }
            else
            {
                gridRelative.Visibility = Visibility.Collapsed;
                txtKeyboardCmd.Visibility = Visibility.Collapsed;

                btnMouseCoordinate.Visibility = Visibility.Visible;
                btnMouseCoordinate.IsEnabled = false;

                checkImageSearchRequired.Visibility = lblImageSearchRequired.Visibility = Visibility.Collapsed;
                checkImageSearchRequired.IsChecked = true;

                checkSameImageDrag.Visibility = Visibility.Collapsed;
                checkSameImageDrag.IsChecked = false;
                numMaxSameImageCount.Visibility = Visibility.Collapsed;

                //btnMouseWheel.Visibility = Visibility.Visible;
                //btnMouseWheel.IsEnabled = false;

                //lblWheelData.Visibility = Visibility.Collapsed;
                //gridWheelData.Visibility = Visibility.Collapsed;
            }
        }

    }
}
