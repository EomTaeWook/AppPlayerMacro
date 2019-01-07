using Macro.Extensions;
using Macro.Models;
using Macro.Models.Event;
using Macro.Models.ViewModel;
using System.Collections.Generic;
using System.Windows.Controls;
using Unity;
using Utils;
using System.Linq;

namespace Macro.View
{
    public partial class ConfigEventView : UserControl
    {
        public EventTriggerModel Model
        {
            get => ((ConfigEventViewModel)DataContext).Trigger;
            private set => ((ConfigEventViewModel)DataContext).Trigger = value;
        }

        private List<MousePositionView> _mousePointViews;
        private EventTriggerModel _dummy;
        public ConfigEventView()
        {
            _dummy = new EventTriggerModel();
            _mousePointViews = new List<MousePositionView>();
            DataContext = Singleton<UnityContainer>.Instance.Resolve<ConfigEventViewModel>();
            Model = _dummy;
            InitializeComponent();

            Loaded += ConfigEventView_Loaded;
        }

        private void Init()
        {
            grdSaves.ItemsSource = (DataContext as ConfigEventViewModel).TriggerSaves;
            foreach (var item in CaptureHelper.MonitorInfo())
            {
                _mousePointViews.Add(new MousePositionView(item));
                _mousePointViews.Last().DataBinding += ConfigEventView_DataBinding;
            }
        }

        private void ConfigEventView_DataBinding(object sender, MousePointArgs args)
        {
            if (Model == _dummy)
            {
                Model = new EventTriggerModel();
            }
            Model.MonitorInfo = args.MonitorInfo;
            Model.MousePoint = args.MousePoint;
            foreach (var item in _mousePointViews)
            {
                item.Hide();
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
