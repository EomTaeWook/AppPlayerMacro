using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Models;
using Macro.Models.ViewModel;
using Macro.UI;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Utils;

namespace Macro.View
{
    public partial class ConfigEventView : UserControl
    {
        public EventTriggerModel Model
        {
            get => ((ConfigEventViewModel)DataContext).Trigger;
            private set => ((ConfigEventViewModel)DataContext).Trigger = value;
        }
        public List<EventTriggerModel> TriggerSaves
        {
            get => ((ConfigEventViewModel)DataContext).TriggerSaves.ToList();
        }

        private List<MousePositionView> _mousePointViews;
        private EventTriggerModel _dummy;
        private bool _isDrag;
        public ConfigEventView()
        {
            _isDrag = false;
            _dummy = new EventTriggerModel();
            _mousePointViews = new List<MousePositionView>();
            DataContext = new ViewModelLocator().ConfigEventViewModel;
            Model = _dummy;
            InitializeComponent();

            Loaded += ConfigEventView_Loaded;
        }

        private void Init()
        {
            treeSaves.ItemsSource = (DataContext as ConfigEventViewModel).TriggerSaves;
            foreach (var item in DisplayHelper.MonitorInfo())
            {
                _mousePointViews.Add(new MousePositionView(item));
            }
        }

        public void ShowMousePoisitionView()
        {
            foreach (var item in _mousePointViews)
                item.ShowActivate();
        }
        public void InsertModel(EventTriggerModel model)
        {
            if (model == null)
                return;

            Dispatcher.Invoke(() =>
            {
                var saves = ((ConfigEventViewModel)DataContext).TriggerSaves;

                if(!saves.Contains(model))
                    ((ConfigEventViewModel)DataContext).TriggerSaves.Add(model);

                Clear();
            });
        }
        public void RemoveModel(EventTriggerModel model)
        {
            if (model == null)
                return;
            var viewModel = DataContext as ConfigEventViewModel;
            if (model != viewModel.SelectTreeItem.DataContext)
                return;
            Dispatcher.Invoke(() =>
            {
                if(viewModel.SelectTreeItem.ParentItem == null)
                {
                    viewModel.TriggerSaves.Remove(model);
                }
                else
                {
                    (viewModel.SelectTreeItem.ParentItem.DataContext as EventTriggerModel).SubEventTriggers.Remove(model);
                }
                Clear();
            });
        }
        public void Clear()
        {
            if (Model != _dummy)
                Model = _dummy;
            (DataContext as ConfigEventViewModel).SelectTreeItem = null;
            RadioButtonRefresh();

        }
        private void ItemContainerPositionChange(Point point)
        {
            var targetRow = treeSaves.TryFindFromPoint<TreeGridViewItem>(point);
            var dragItem = (DataContext as ConfigEventViewModel).SelectTreeItem;

            var parentItemContainer = dragItem.ParentItem == null ? (DataContext as ConfigEventViewModel).TriggerSaves : (dragItem.ParentItem.DataContext as EventTriggerModel).SubEventTriggers;

            if (targetRow != null)
            {
                if (targetRow == dragItem)
                    return;
                var item = dragItem.DataContext as EventTriggerModel;
                var targetItem = targetRow.DataContext as EventTriggerModel;

                if (targetRow.ParentItem == null && dragItem.ParentItem == null)
                {
                    parentItemContainer.Remove(item);
                    (targetRow.DataContext as EventTriggerModel).SubEventTriggers.Add(item);
                }
                else if (targetRow.ParentItem != dragItem.ParentItem && targetRow.ParentItem != dragItem)
                {
                    parentItemContainer.Remove(item);
                    targetItem.SubEventTriggers.Add(item);
                }
                else if (targetRow.ParentItem == dragItem.ParentItem)
                {
                    var targetParent = (targetRow.ParentItem.DataContext as EventTriggerModel);
                    var targetIndex = targetParent.SubEventTriggers.IndexOf(targetRow.DataContext as EventTriggerModel);

                    targetParent.SubEventTriggers.Remove(item);
                    targetParent.SubEventTriggers.Insert(targetIndex, item);
                }
                else if(targetRow.ParentItem == dragItem)
                {
                    parentItemContainer.Remove(item);
                    item.SubEventTriggers.Remove(targetItem);
                    var targetSubItem = targetItem.SubEventTriggers;
                    targetItem.SubEventTriggers = item.SubEventTriggers;
                    item.SubEventTriggers = targetSubItem;
                    targetItem.SubEventTriggers.Add(item);
                    parentItemContainer.Add(targetItem);
                }
                NotifyHelper.InvokeNotify(Infrastructure.EventType.EventTriggerOrderChanged, new EventTriggerOrderChangedEventArgs()
                {
                    TriggerModel1 = item,
                    TriggerModel2 = targetItem
                });
            }
            else
            {
                var item = dragItem.DataContext as EventTriggerModel;
                parentItemContainer.Remove(item);
                (DataContext as ConfigEventViewModel).TriggerSaves.Add(item);

                NotifyHelper.InvokeNotify(Infrastructure.EventType.EventTriggerOrderChanged, new EventTriggerOrderChangedEventArgs()
                {
                    TriggerModel1 = item,
                });
            }
            
        }
        private void RadioButtonRefresh()
        {
            if (Model.EventType == Models.EventType.Mouse)
            {
                btnMouseCoordinate.Visibility = Visibility.Visible;
                txtKeyboardCmd.Visibility = Visibility.Collapsed;
                btnMouseCoordinate.IsEnabled = true;
            }
            else if(Model.EventType == Models.EventType.Keyboard)
            {
                btnMouseCoordinate.Visibility = Visibility.Collapsed;
                txtKeyboardCmd.Visibility = Visibility.Visible;
            }
            else
            {
                btnMouseCoordinate.Visibility = Visibility.Visible;
                txtKeyboardCmd.Visibility = Visibility.Collapsed;
                btnMouseCoordinate.IsEnabled = false;
            }
        }
    }
}
