using Macro.Extensions;
using Macro.Models;
using Macro.Models.ViewModel;
using System.Collections.Generic;
using System.Linq;
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
            grdSaves.ItemsSource = (DataContext as ConfigEventViewModel).TriggerSaves;
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

            Dispatcher.Invoke(() =>
            {
                ((ConfigEventViewModel)DataContext).TriggerSaves.Remove(model);
                Clear();
            });
        }
        public void Clear()
        {
            if (Model != _dummy)
                Model = _dummy;

            grdSaves.SelectedItem = null;
            RadioButtonRefresh();

        }
        private void RadioButtonRefresh()
        {
            if (Model.EventType == EventType.Mouse)
            {
                btnMouseCoordinate.Visibility = System.Windows.Visibility.Visible;
                txtKeyboardCmd.Visibility = System.Windows.Visibility.Collapsed;
                btnMouseCoordinate.IsEnabled = true;
            }
            else if(Model.EventType == EventType.Keyboard)
            {
                btnMouseCoordinate.Visibility = System.Windows.Visibility.Collapsed;
                txtKeyboardCmd.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                btnMouseCoordinate.Visibility = System.Windows.Visibility.Visible;
                txtKeyboardCmd.Visibility = System.Windows.Visibility.Collapsed;
                btnMouseCoordinate.IsEnabled = false;
            }
        }
    }
}
