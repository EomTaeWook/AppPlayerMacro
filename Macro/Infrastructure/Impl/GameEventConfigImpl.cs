using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Impl;
using Macro.Models;
using Macro.Models.ViewModel;
using Macro.UI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Macro.View
{
    public partial class GameEventConfigView : BaseEventConfigView<GameEventConfigViewModel>
    {
        public TreeGridViewItem CurrentTreeViewItem
        {
            get => this.DataContext<GameEventConfigViewModel>().CurrentTreeViewItem;
            protected set => this.DataContext<GameEventConfigViewModel>().CurrentTreeViewItem = value;
        }
        public PointModel RelativePosition
        {
            get => this.DataContext<GameEventConfigViewModel>().RelativePosition;
            protected set => this.DataContext<GameEventConfigViewModel>().RelativePosition = value;
        }

        private readonly TreeGridViewItem _dummyTreeGridViewItem;
        private readonly PointModel _dummyRelativePosition;
        private readonly ObservableCollection<KeyValuePair<ConditionType, string>> _conditionItems;

        public GameEventConfigView()
        {
            _dummyTreeGridViewItem = new TreeGridViewItem()
            {
                DataContext = new GameEventConfigViewModel()
            };
            _dummyRelativePosition = new PointModel();

            _conditionItems = new ObservableCollection<KeyValuePair<ConditionType, string>>();

            DataContext = new ViewModelLocator().GameEventConfigViewModel;

            CurrentTreeViewItem = _dummyTreeGridViewItem;

            RelativePosition = _dummyRelativePosition;

            InitializeComponent();

            //InitEvent();

            //Init();
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
