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
            Dispatcher.Invoke(() =>
            {
                ((ConfigEventViewModel)DataContext).TriggerSaves.Add(model);
                if (Model != _dummy)
                {
                    Model = _dummy;
                }
            });
        }
        public void RemoveModel(EventTriggerModel model)
        {
            Dispatcher.Invoke(() =>
            {
                if (((ConfigEventViewModel)DataContext).TriggerSaves.Remove(model.Index))
                {
                    if (Model != _dummy)
                    {
                        Model = _dummy;
                    }
                }
            });
        }
        public void Clear()
        {
            if (Model != _dummy)
            {
                Model = _dummy;
            }
            grdSaves.SelectedItem = null;
        }
    }
}
