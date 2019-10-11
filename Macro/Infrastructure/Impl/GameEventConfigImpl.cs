using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Impl;
using Macro.Models;
using Macro.Models.ViewModel;
using Macro.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

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

        private readonly TreeGridViewItem _dummyTreeGridViewItem;
        private readonly PointModel _dummyRelativePosition;
        private readonly ValueConditionModel _dummyHpCondition;
        private readonly ValueConditionModel _dummyMpCondition;
        private readonly ObservableCollection<KeyValuePair<ConditionType, string>> _conditionItems;
        private readonly GameEventConfigViewModel _contextViewModel;
        public GameEventConfigView()
        {
            _contextViewModel = new ViewModelLocator().GameEventConfigViewModel;

            _dummyTreeGridViewItem = new TreeGridViewItem()
            {
                DataContext = new GameEventTriggerModel()
            };
            _dummyRelativePosition = new PointModel();

            _dummyHpCondition = new ValueConditionModel();

            _dummyMpCondition = new ValueConditionModel();

            _conditionItems = new ObservableCollection<KeyValuePair<ConditionType, string>>();

            _contextViewModel.HpCondition = _dummyHpCondition;

            _contextViewModel.MpCondition = _dummyMpCondition;

            _contextViewModel.CurrentTreeViewItem = _dummyTreeGridViewItem;

            _contextViewModel.RelativePosition = _dummyRelativePosition;

            InitializeComponent();

            InitEvent();

            Init();
        }
        private void Init()
        {
            DataContext = _contextViewModel;

            treeSaves.ItemsSource = this.DataContext<GameEventConfigViewModel>().TriggerSaves;

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
        }
        public TreeGridViewItem CopyCurrentItem()
        {
            if (CurrentTreeViewItem == _dummyTreeGridViewItem)
                return null;
            Dispatcher.Invoke(() =>
            {
                var treeVIewItem = treeSaves.GetSelectItemFromObject<TreeGridViewItem>(CurrentTreeViewItem.DataContext<GameEventTriggerModel>());
                if (treeVIewItem != null)
                {
                    CurrentTreeViewItem = new TreeGridViewItem()
                    {
                        DataContext = new GameEventTriggerModel(treeVIewItem.DataContext<GameEventTriggerModel>())
                    };
                }
            });
            return CurrentTreeViewItem;
        }

        private void RadioButtonRefresh()
        {
            if (CurrentTreeViewItem.DataContext<GameEventTriggerModel>().EventType == EventType.Mouse)
            {
                txtKeyboardCmd.Visibility = Visibility.Collapsed;
                gridRelative.Visibility = Visibility.Collapsed;

                btnMouseCoordinate.Visibility = Visibility.Visible;
                btnMouseCoordinate.IsEnabled = true;

                //btnMouseWheel.Visibility = Visibility.Visible;
                //btnMouseWheel.IsEnabled = false;
            }
            else if (CurrentTreeViewItem.DataContext<GameEventTriggerModel>().EventType == EventType.Keyboard)
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
            else if (CurrentTreeViewItem.DataContext<GameEventTriggerModel>().EventType == EventType.RelativeToImage)
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
