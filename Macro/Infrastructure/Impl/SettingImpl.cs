using Macro.Infrastructure;
using Macro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Utils.Document;
using Macro.Extensions;
using System.Threading.Tasks;
using Utils;

namespace Macro.View
{
    public partial class SettingView : UserControl
    {
        public IConfig Config { get; private set; }

        private Config _dummy;
        private TaskQueue _taskQueue;
        public SettingView()
        {
            _taskQueue = new TaskQueue();
            _dummy = new Config();
            Config = ObjectExtensions.GetInstance<IConfig>();

            InitializeComponent();
            Loaded += SettingView_Loaded;
        }
        private void Init()
        {
            var source = new List<string>();
            var languages = Enum.GetValues(typeof(Language)).Cast<Language>();
            foreach(var language in languages)
            {
                if (language == Utils.Document.Language.Max)
                    continue;
                source.Add(language.ToString());
            }
            comboLanguage.ItemsSource = source;
            comboLanguage.SelectedValue = Config.Language.ToString();

            txtSavePath.Text = Config.SavePath;
            numPeriod.Value = Config.Period;
            numDelay.Value = Config.ProcessDelay;
            numSimilarity.Value = Config.Similarity;
        }
        private bool TryModelValidate(Config model, out Message message)
        {
            message = Message.Success;

            if (model.Period <= ConstHelper.MinPeriod)
            {
                message = Message.FailedPeriodValidate;
                return false;
            }
            if (model.ProcessDelay <= ConstHelper.MinProcessDelay)
            {
                message = Message.FailedProcessDelayValidate;
                return false;
            }
            if (model.Similarity <= ConstHelper.MinSimilarity)
            {
                message = Message.FailedSimilarityValidate;
                return false;
            }
            return true;
        }
        private Task Save()
        {
            var file = Environment.CurrentDirectory + $"config.json";
            

            return Task.CompletedTask;
        }
    }
}
