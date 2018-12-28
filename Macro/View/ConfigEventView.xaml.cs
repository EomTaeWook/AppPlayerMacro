using Macro.Extensions;
using Macro.Models;
using MahApps.Metro.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Utils.Document;

namespace Macro.View
{
    /// <summary>
    /// ConfigEventView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ConfigEventView : UserControl
    {
        public ObservableCollection<ConfigEventModel> SaveConfigs { get; private set; }
        public ConfigEventView()
        {
            InitializeComponent();
            Model = new ConfigEventModel();
            SaveConfigs = new ObservableCollection<ConfigEventModel>();

            this.Loaded += ConfigEventView_Loaded;
        }
        private void ConfigEventView_Loaded(object sender, RoutedEventArgs e)
        {
            EventInit();
            Init();
        }
        private void Init()
        {

        }
        private void EventInit()
        {
            var radioButtons = this.FindChildren<RadioButton>();
            foreach(var button in radioButtons)
            {
                button.Click += RadioButton_Click;
            }
            btnMouseCoordinate.Click += Button_Click;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(sender.Equals(btnMouseCoordinate))
            {
                var mousePosition = new MousePositionView();
                mousePosition.ShowDialog();
                Model.MousePoint = mousePosition.MousePoint;
                Desc(Model);
            }
            //else if (sender.Equals(btnSave))
            //{
            //    if (TryModelValidate(_dummy, out Message message))
            //    {
            //        Save(this, _dummy);
            //        _saves.Add(new ConfigEventModel(_dummy));
            //        _dummy = null;
            //        _dummy = new ConfigEventModel();
            //        Refresh();
            //    }
            //    else
            //    {
            //        this.MessageShow("Error", DocumentHelper.Get(message));
            //    }
            //}
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if(sender.Equals(rbMouse))
            {
                btnMouseCoordinate.Visibility = Visibility.Visible;
                txtKeyboardCmd.Visibility = Visibility.Collapsed;

                Model.EventType = EventType.Mouse;
            }
            else if(sender.Equals(rbKeyboard))
            {
                btnMouseCoordinate.Visibility = Visibility.Collapsed;
                txtKeyboardCmd.Visibility = Visibility.Visible;

                Model.EventType = EventType.Keyboard;
            }
            Desc(Model);
        }
        private void Desc(ConfigEventModel model)
        {
            if(model.EventType == EventType.Mouse)
            {
                if(model.MousePoint != null)
                {
                    lblDesc.Content = $"X : {model.MousePoint?.X} Y : {model.MousePoint?.Y}";
                }
            }
            else if(model.EventType == EventType.Keyboard)
            {
                lblDesc.Content = $"{model.KeyBoardCmd}";
            }
        }
        public void InsertModel(ConfigEventModel model)
        {
            SaveConfigs.Add(model);
            Model = null;
            Model = new ConfigEventModel();
        }
        public ConfigEventModel Model { get; private set; }
    }
}
