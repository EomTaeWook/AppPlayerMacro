using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Controller;
using Macro.Infrastructure.Manager;
using Macro.Models;
using Macro.Models.ViewModel;
using MahApps.Metro.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Utils;
using Utils.Document;

namespace Macro.View
{
    /// <summary>
    /// SettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingView : UserControl
    {
        public Config Config { get; private set; }

        public SettingView()
        {
            InitializeComponent();
            Loaded += SettingView_Loaded;
        }
        
        private void Init()
        {
            var languages = Enum.GetValues(typeof(Language)).Cast<Language>().Where(r => r != Utils.Document.Language.Max);
            comboLanguage.ItemsSource = languages;
            comboInitialTab.ItemsSource = Enum.GetValues(typeof(InitialTab)).Cast<InitialTab>().Where(r => r != InitialTab.Max);

            DataContext = new ViewModelLocator().SettingViewModel;
        }
        private void SettingView_Loaded(object sender, RoutedEventArgs e)
        {
            EventInit();
            Init();
        }
        private void EventInit()
        {
            btnSave.Click += Button_Click;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if(btn.Equals(btnSave))
            {
                var model = (DataContext as SettingViewModel).Config;
                if (TryModelValidate(model, out Message error))
                {
                    Save(model);
                }
                else
                {
                    ApplicationManager.MessageShow("Error", DocumentHelper.Get(error));
                }
            }
        }
        private void Save(Config model)
        {
            var path = Environment.CurrentDirectory + $@"\{ConstHelper.DefaultConfigFile}";
            var saved = FileManager.Instance.SaveJson(path, model);

            if(saved == true)
            {
                NotifyHelper.InvokeNotify(NotifyEventType.ConfigChanged, new ConfigEventArgs() { Config = model });
            }
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
    }
}
