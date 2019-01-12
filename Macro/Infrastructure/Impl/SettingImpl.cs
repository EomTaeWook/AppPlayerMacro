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

namespace Macro.View
{
    public partial class SettingView : UserControl
    {
        public IConfig Config { get; private set; }

        private TaskQueue _taskQueue;
        public SettingView()
        {
            _taskQueue = new TaskQueue();          

            InitializeComponent();
            Loaded += SettingView_Loaded;
        }
        private void Init()
        {
            var source = new List<string>();
            var languages = Enum.GetValues(typeof(Language)).Cast<Language>().Where(r=> r != Utils.Document.Language.Max);
            comboLanguage.ItemsSource = languages;

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
            if (model.ProcessDelay < ConstHelper.MinProcessDelay)
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
            NotifyHelper.InvokeNotify(Infrastructure.EventType.ConfigChanged, new ConfigEventArgs() { Config = state as Config });
            return Task.CompletedTask;
        }
    }
}
