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
using Macro.Infrastructure;
using System.Threading.Tasks;

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
        private TaskQueue _taskQueue;
        private int _index;

        public MainWindow()
        {
            InitializeComponent();
            _index = 0;
            _taskQueue = new TaskQueue();
            _config = Singleton<UnityContainer>.Instance.Resolve<IConfig>();
            this.Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //window7 not support
            //NativeHelper.SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE);
            Init();
            SaveLoad();
        }
        private void Init()
        {
            _processes = Process.GetProcesses().ToList();
            combo_process.ItemsSource = _processes.OrderBy(r => r.ProcessName).Select(r => r.ProcessName).ToList();
            Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            ScaleTransform dpiTransform = new ScaleTransform(1 / m.M11, 1 / m.M22);
            if (dpiTransform.CanFreeze)
                dpiTransform.Freeze();
            this.LayoutTransform = dpiTransform;

            btnCapture.Click += Button_Click;
            btnRefresh.Click += Button_Click;
            btnSave.Click += Button_Click;
            btnDelete.Click += Button_Click;
            configControl.SelectData += ConfigControl_SelectData;
        }

        private void ConfigControl_SelectData(ConfigEventModel model)
        {
            if(model == null)
            {
                Clear();
            }
            else
            {
                btnDelete.Visibility = Visibility.Visible;
                _bitmap = model.Image;
                captureImage.Background = new ImageBrush(_bitmap.ToBitmapSource());
            }
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
                if (TryModelValidate(model, out Message error))
                {
                    _taskQueue.Enqueue(Save, model).ContinueWith((task) =>
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Clear();
                        });                        
                    }).Finally(r => ((ConfigEventView)r).InsertModel(model), configControl);
                }
                else
                {
                    this.MessageShow("Error", DocumentHelper.Get(error));
                }
            }
            else if(btn.Equals(btnDelete))
            {
                var model = configControl.Model;
                _taskQueue.Enqueue((o) =>
                {
                    configControl.RemoveModel(model);
                    return Task.CompletedTask;
                }, configControl)
                .ContinueWith((task) =>
                {
                    if (task.IsCompleted)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Delete(model);
                            Clear();
                        });
                    }
                });
            }
        }
        private bool TryModelValidate(ConfigEventModel model, out Message message)
        {
            message = Message.Success;
            model.KeyboardCmd = model.KeyboardCmd.Replace(" ", "");
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

            if (string.IsNullOrEmpty(model.KeyboardCmd) && model.EventType == EventType.Keyboard)
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
            capture.ShowDialog();
            if (capture.CaptureImage != null)
            {
                _bitmap = capture.CaptureImage;
                captureImage.Background = new ImageBrush(_bitmap.ToBitmapSource());
            }
            this.WindowState = WindowState.Normal;
        }
        private void Clear()
        {
            btnDelete.Visibility = Visibility.Collapsed;
            _bitmap = null;
            captureImage.Background = System.Windows.Media.Brushes.White;
        }
        private Task Delete(object m)
        {
            var model = m as ConfigEventModel;
            var path = _config.SavePath;
            if (string.IsNullOrEmpty(path))
                path = ConstHelper.DefaultSavePath;
            path = $@"{path}config.save";
            if (File.Exists(path))
            {
                File.Delete(path);
                using (var fs = new FileStream(path, FileMode.CreateNew))
                {
                    foreach(var data in (configControl.DataContext as Models.ViewModel.ConfigEventViewModel).ConfigSaves)
                    {
                        var bytes = ObjectExtensions.SerializeObject(data);
                        fs.Write(bytes, 0, bytes.Count());
                    }
                    fs.Close();
                }
            }
            return Task.CompletedTask;
        }
        private Task Save(object m)
        {
            var model = m as ConfigEventModel;
            model.Index = _index++;
            var path = _config.SavePath;
            if (string.IsNullOrEmpty(path))
                path = ConstHelper.DefaultSavePath;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            using (var fs = new FileStream($@"{path}config.save", FileMode.Append))
            {
                var bytes = ObjectExtensions.SerializeObject(model);
                fs.Write(bytes, 0, bytes.Count());
                fs.Close();
            }
            return Task.CompletedTask;
        }
        private void SaveLoad()
        {
            var path = _config.SavePath;
            if (string.IsNullOrEmpty(path))
                path = ConstHelper.DefaultSavePath;
            if(File.Exists($@"{path}config.save"))
            {
                var models = ObjectExtensions.DeserializeObject(File.ReadAllBytes($@"{path}config.save"));
                _index = models.LastOrDefault()?.Index ?? 0;
                foreach (var model in models)
                {
                    configControl.InsertModel(model);
                }
            }
        }
    }
}
