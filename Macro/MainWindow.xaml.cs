using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Utils;
using Image = Macro.Extensions.Image;

namespace Macro
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Process> _processes;
        public MainWindow()
        {
            InitializeComponent();
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
            var item = combo_process.SelectedValue;
            if (item == null)
                return;

            var btn = sender as Button;
            if (btn.Equals(btnCapture) && TryCapture(item))
            {
                Application.Current.MainWindow.Activate();
            }
            else if(btn.Equals(btnRefresh))
                _processes = Process.GetProcesses().ToList();
        }
        private bool TryCapture(object item)
        {
            var process = _processes.Where(r => r.ProcessName == item.ToString()).FirstOrDefault();
            if (process == null)
                return false;

            var imageBrush = new ImageBrush(Image.ToBitmapSource(CaptureHelper.Capture(process)));
            captureZone.Background = imageBrush;
            return true;
        }
    }
}
