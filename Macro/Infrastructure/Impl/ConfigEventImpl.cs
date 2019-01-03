using Macro.Models;
using Macro.Models.ViewModel;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using Utils;
using Macro.Extensions;
using Unity;
using Macro.Models.Event;
using Utils.Infrastructure;

namespace Macro.View
{
    public partial class ConfigEventView : UserControl
    {
        public ConfigEventModel Model
        {
            get => ((ConfigEventViewModel)DataContext).ConfigData;
            private set => ((ConfigEventViewModel)DataContext).ConfigData = value;
        }

        private List<MousePositionView> _mousePointViews;
        private ConfigEventModel _dummy;
        public ConfigEventView()
        {
            _dummy = new ConfigEventModel();
            _mousePointViews = new List<MousePositionView>();
            DataContext = Singleton<UnityContainer>.Instance.Resolve<ConfigEventViewModel>();
            Model = _dummy;
            InitializeComponent();

            Loaded += ConfigEventView_Loaded;
        }

        private void Init()
        {
            grdSaves.ItemsSource = (DataContext as ConfigEventViewModel).ConfigSaves;
            foreach(var item in CaptureHelper.MonitorInfo())
            {
                _mousePointViews.Add(new MousePositionView(item));
                _mousePointViews.Last().DataBinding += ConfigEventView_DataBinding;
            }
        }

        private void ConfigEventView_DataBinding(object sender, MousePointArgs args)
        {
            if (Model == _dummy)
            {
                Model = new ConfigEventModel();
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
        public void InsertModel(ConfigEventModel model)
        {
            Dispatcher.Invoke(() =>
            {
                ((ConfigEventViewModel)DataContext).ConfigSaves.Add(model);
                if (Model != _dummy)
                {
                    Model = _dummy;
                }
            });
        }
        public void RemoveModel(ConfigEventModel model)
        {
            Dispatcher.Invoke(() =>
            {
                if (((ConfigEventViewModel)DataContext).ConfigSaves.Remove(model.Index))
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
