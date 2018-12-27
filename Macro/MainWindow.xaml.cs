using Macro.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Utils;
using Unity;
using MahApps.Metro.Controls;
using System.Drawing;
using Macro.View;
using Macro.Extensions;
using Utils.Document;
using System.IO;
using System.Text;

namespace Macro
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private List<Process> _processes;
        private IConfig _config;
        private Bitmap _bitmap;
        public MainWindow()
        {
            InitializeComponent();
            _config = Singleton<UnityContainer>.Instance.Resolve<IConfig>();
            this.Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            NativeHelper.SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
            Init();
        }
        private void Init()
        {
            _processes = Process.GetProcesses().ToList();
            combo_process.ItemsSource = _processes.OrderBy(r => r.ProcessName).Select(r => r.ProcessName).ToList();

            btnCapture.Click += Button_Click;
            btnRefresh.Click += Button_Click;
            btnSave.Click += Button_Click;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.Equals(btnCapture))
            {
                Capture();
                Application.Current.MainWindow.Activate();
            }
            else if (btn.Equals(btnRefresh))
            {
                _processes = Process.GetProcesses().ToList();
                combo_process.ItemsSource = _processes.OrderBy(r => r.ProcessName).Select(r => r.ProcessName).ToList();
            }
            else if (btn.Equals(btnSave))
            {
                var model = configControl.Model;
                model.Image = _bitmap;
                model.ProcessName = combo_process.SelectedValue as string;
                if(TryModelValidate(model, out Message error))
                {
                    Save(model);
                }
                else
                {
                    this.MessageShow("Error", DocumentHelper.Get(error));
                }
            }
        }
        private bool TryModelValidate(ConfigEventModel model, out Message message)
        {
            message = Message.Success;
            model.KeyBoardCmd = model.KeyBoardCmd.Replace(" ", "");
            if (model.Image == null)
            {
                message = Message.FailedImageValidate;
                return false;
            }
            if (model.EventType == EventType.Mouse && model.MousePoint == null)
            {
                message = Message.FailedMouseCoordinatesValidate;
                return false;
            }

            if (string.IsNullOrEmpty(model.KeyBoardCmd) && model.EventType == EventType.Keyboard)
            {
                message = Message.FailedKeyboardCommandValidate;
                return false;
            }
            if (string.IsNullOrEmpty(model.ProcessName))
            {
                message = Message.FailedProcessValidate;
                return false;
            }
            return true;
        }
        private void Capture()
        {
            var capture = new CaptureView();
            this.WindowState = WindowState.Minimized;
            capture.Show();
            if (capture.CaptureImage != null)
            {
                _bitmap = capture.CaptureImage;
                captureImage.Background = new ImageBrush(_bitmap.ToBitmapSource());
            }
            this.WindowState = WindowState.Normal;
        }
        private void Save(ConfigEventModel model)
        {
            string path = _config.SavePath;
            if (string.IsNullOrEmpty(path))
                path = ConstHelper.DefaultSavePath;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            File.AppendAllText($@"{path}config.save", Encoding.UTF8.GetString(model.SerializeObject())+@"\r\n");
        }
    }
}
