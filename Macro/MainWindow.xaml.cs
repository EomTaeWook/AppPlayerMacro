using Macro.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Utils;
using Utils.Document;
using Image = Macro.Extensions.Image;
using Macro.Extensions;
using Unity;
using MahApps.Metro.Controls;
using Macro.View;

namespace Macro
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private List<Process> _processes;
        private IConfig _config;
        public MainWindow()
        {
            InitializeComponent();
            _config = Singleton<UnityContainer>.Instance.Resolve<IConfig>();
            this.Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }
        private void Init()
        {
            _processes = Process.GetProcesses().ToList();
            combo_process.ItemsSource = _processes.OrderBy(r => r.ProcessName).Select(r => r.ProcessName).ToList();

            btnCapture.Click += Button_Click;
            btnRefresh.Click += Button_Click;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.Equals(btnCapture))
            {
                Capture();
                Application.Current.MainWindow.Activate();
            }
            else if(btn.Equals(btnRefresh))
            {
                _processes = Process.GetProcesses().ToList();
                combo_process.ItemsSource = _processes.OrderBy(r => r.ProcessName).Select(r => r.ProcessName).ToList();
            }
        }
        private void Capture()
        {
            var capture = new CaptureView();
            this.WindowState = WindowState.Minimized;
            capture.ShowDialog();
            if(capture.CaptureImage != null)
            {
                captureImage.Background = new ImageBrush(Image.ToBitmapSource(capture.CaptureImage));
            }
            this.WindowState = WindowState.Normal;
        }
    }
}
