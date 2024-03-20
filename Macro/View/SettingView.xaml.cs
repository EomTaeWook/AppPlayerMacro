﻿using DataContainer.Generated;
using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Manager;
using Macro.Models;
using Macro.Models.ViewModel;
using Macro.UI;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TemplateContainers;
using Utils;
using Utils.Models;

namespace Macro.View
{
    /// <summary>
    /// SettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingView : UIItem
    {
        public Config Config { get; private set; }

        public SettingView()
        {
            InitializeComponent();
            Loaded += SettingView_Loaded;
        }

        private void Init()
        {
            var languages = Enum.GetValues(typeof(LanguageType)).Cast<LanguageType>().Where(r => r != LanguageType.Max);
            comboLanguage.ItemsSource = languages;


            DataContext = ServiceDispatcher.Resolve<SettingViewModel>();
        }
        private void SettingView_Loaded(object sender, RoutedEventArgs e)
        {
            EventInit();
            Init();
        }
        private void EventInit()
        {
            btnSave.Click += Button_Click;
            btnClose.Click += Button_Click;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.Equals(btnSave))
            {
                var model = (DataContext as SettingViewModel).Config;
                if (TryModelValidate(model, out MessageTemplate template))
                {
                    Save(model);
                }
                else
                {
                    ApplicationManager.ShowMessageDialog("Error", template.GetString());
                }
                UIManager.Instance.ClosePopup(this);
            }
            else if (btn.Equals(btnClose))
            {
                UIManager.Instance.ClosePopup(this);
            }


        }
        private void Save(Config model)
        {
            var path = Environment.CurrentDirectory + $@"\{ConstHelper.DefaultConfigFile}";
            var fileManager = ServiceDispatcher.Resolve<FileService>();
            var saved = fileManager.SaveJson(path, model);

            if (saved == true)
            {
                NotifyHelper.InvokeNotify(NotifyEventType.ConfigChanged, new ConfigEventArgs() { Config = model });
            }
        }
        private bool TryModelValidate(Config model, out MessageTemplate messageTemplate)
        {
            messageTemplate = TemplateContainer<MessageTemplate>.Find(1000);

            if (model.Period < ConstHelper.MinPeriod)
            {
                messageTemplate = TemplateContainer<MessageTemplate>.Find(1008);
                return false;
            }
            if (model.ItemDelay < ConstHelper.MinItemDelay)
            {
                messageTemplate = TemplateContainer<MessageTemplate>.Find(1009);
                return false;
            }
            if (model.Similarity < ConstHelper.MinSimilarity)
            {
                messageTemplate = TemplateContainer<MessageTemplate>.Find(1010);
                return false;
            }
            return true;
        }
    }
}
