using Macro.Infrastructure;
using Macro.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utils;
using Utils.Document;
using Utils.Infrastructure;

namespace Macro.View
{
    public partial class SettingView : UserControl
    {
        public IConfig Config { get; private set; }

        private readonly TaskQueue _taskQueue;
        public SettingView()
        {
            _taskQueue = new TaskQueue();          

            InitializeComponent();
            Loaded += SettingView_Loaded;
        }
        private void Init()
        {
            var languages = Enum.GetValues(typeof(Language)).Cast<Language>().Where(r=> r != Utils.Document.Language.Max);
            comboLanguage.ItemsSource = languages;

            comboInitialTab.ItemsSource = Enum.GetValues(typeof(InitialTab)).Cast<InitialTab>().Where(r => r != InitialTab.Max);

            DataContext = new ViewModelLocator().SettingViewModel;
        }
        private bool TryModelValidate(Config model, out Message message)
        {
            message = Message.Success;

            if (model.Period < ConstHelper.MinPeriod)
            {
                message = Message.FailedPeriodValidate;
                return false;
            }
            if (model.ItemDelay < ConstHelper.MinItemDelay)
            {
                message = Message.FailedProcessDelayValidate;
                return false;
            }
            if (model.Similarity < ConstHelper.MinSimilarity)
            {
                message = Message.FailedSimilarityValidate;
                return false;
            }
            return true;
        }
        private Task Save(object state)
        {
            var path = Environment.CurrentDirectory + $@"\{ConstHelper.DefaultConfigFile}";
            File.WriteAllText(path, JsonHelper.SerializeObject(state, true));
            NotifyHelper.InvokeNotify(NotifyEventType.ConfigChanged, new ConfigEventArgs() { Config = state as Config });
            return Task.CompletedTask;
        }
    }
}
