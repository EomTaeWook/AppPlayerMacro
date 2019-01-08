using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Serialize;
using Macro.Models;
using Macro.View;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Utils;
using Utils.Infrastructure;
using Message = Utils.Document.Message;
using Rect = Utils.Infrastructure.Rect;

namespace Macro
{
    public partial class MainWindow : MetroWindow
    {
        private TaskQueue _taskQueue;
        private string _path;
        private int _index;
        private IEnumerable<KeyValuePair<string, Process>> _processes;
        private IConfig _config;
        private Bitmap _bitmap;
        private List<CaptureView> _captureViews;

        public MainWindow()
        {
            _index = 0;
            _taskQueue = new TaskQueue();
            _config = ObjectExtensions.GetInstance<IConfig>();
            ProcessManager.AddJob(OnProcessCallback);
            _captureViews = new List<CaptureView>();

            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void Init()
        {
            foreach (var item in CaptureHelper.MonitorInfo())
            {
                _captureViews.Add(new CaptureView(item));
                _captureViews.Last().DataBinding += CaptureView_DataBinding;
            }
            Refresh();

            _path = _config.SavePath;
            if (string.IsNullOrEmpty(_path))
                _path = ConstHelper.DefaultSavePath;
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
            _path = $"{_path}{ConstHelper.DefaultSaveFile}";
            _taskQueue.Enqueue(SaveLoad, _path);
            //window7 not support
            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor > 1)
            {
                NativeHelper.SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
            }
            else
            {
                this.MessageShow("Error", DocumentHelper.Get(Message.FailedOSVersion));
            }
        }
        private void Refresh()
        {
            _processes = Process.GetProcesses().Where(r=>r.MainWindowHandle != IntPtr.Zero).Select(r => new KeyValuePair<string, Process>(r.ProcessName, r));
            combo_process.ItemsSource = _processes.OrderBy(r => r.Key);
            combo_process.DisplayMemberPath = "Key";
            combo_process.SelectedValuePath = "Value";

        }
        private void CaptureView_DataBinding(object sender, Models.Event.CaptureArgs args)
        {
            foreach(var item in _captureViews)
            {
                item.Hide();
            }
            if(args.CaptureImage != null)
            {
                var capture = args.CaptureImage;
                captureImage.Background = new ImageBrush(capture.ToBitmapSource());
                var factor = NativeHelper.GetSystemDpi();
                _bitmap = new Bitmap(capture, (int)Math.Truncate(capture.Width * factor.X / ConstHelper.DefaultDPI), (int)Math.Truncate(capture.Height * factor.Y / ConstHelper.DefaultDPI));
            }
            WindowState = WindowState.Normal;
        }

        private bool TryModelValidate(EventTriggerModel model, out Message message)
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
            if (string.IsNullOrEmpty(model.ProcessInfo.ProcessName))
            {
                message = Message.FailedProcessValidate;
                return false;
            }
            return true;
        }
        private void Capture()
        {
            Clear();
            WindowState = WindowState.Minimized;
            foreach (var item in _captureViews)
                item.ShowActivate();
        }
        private void Clear()
        {
            btnDelete.Visibility = Visibility.Collapsed;
            _bitmap = null;
            captureImage.Background = System.Windows.Media.Brushes.White;
            configControl.Clear();
        }
        private Task Delete(object m)
        {
            var model = m as EventTriggerModel;
            if (File.Exists(_path))
            {
                File.Delete(_path);
                using (var fs = new FileStream(_path, FileMode.CreateNew))
                {
                    foreach (var data in (configControl.DataContext as Models.ViewModel.ConfigEventViewModel).TriggerSaves)
                    {
                        var bytes = ObjectSerializer.SerializeObject(data);
                        fs.Write(bytes, 0, bytes.Count());
                    }
                    fs.Close();
                }
            }
            return Task.CompletedTask;
        }
        private Task Save(object m)
        {
            var model = m as EventTriggerModel;
            model.Index = _index++;
            using (var fs = new FileStream(_path, FileMode.Append))
            {
                var bytes = ObjectSerializer.SerializeObject(model);
                fs.Write(bytes, 0, bytes.Count());
                fs.Close();
            }
            return Task.CompletedTask;
        }
        private Task SaveLoad(object state)
        {
            var task = new TaskCompletionSource<Task>();
            Dispatcher.Invoke(() => 
            {
                try
                {
                    var models = ObjectSerializer.DeserializeObject<EventTriggerModel>(File.ReadAllBytes(_path));
                    _index = models.LastOrDefault()?.Index ?? 0;
                    foreach (var model in models)
                    {
                        configControl.InsertModel(model);
                    }
                    task.SetResult(Task.CompletedTask);
                }
                catch (Exception ex)
                {
                    File.Delete(_path);
                    LogHelper.Warning(ex.Message);
                    Task.FromException(new FileLoadException(DocumentHelper.Get(Message.FailedLoadSaveFile)));
                }
            }, DispatcherPriority.Send);
            return task.Task;
        }
        private Task OnProcessCallback()
        {
            var task = new TaskCompletionSource<Task>();
            Dispatcher.InvokeAsync(() =>
            {
                var saves = (configControl.DataContext as Models.ViewModel.ConfigEventViewModel).TriggerSaves;
                foreach (var save in saves)
                {
                    var processes = _processes.Where(r => r.Key.Equals(save.ProcessInfo.ProcessName)).ToList();
                    foreach (var process in processes)
                    {
                        if (CaptureHelper.ProcessCapture(process.Value, out Bitmap bmp))
                        {
                            captureImage.Background = new ImageBrush(bmp.ToBitmapSource());

                            var similarity = OpenCVHelper.Search(bmp, save.Image);
                            LogHelper.Debug($"similarity : {similarity}");
                            if (similarity >= _config.Similarity)
                            {
                                var hWndActive = NativeHelper.GetForegroundWindow();

                                if (save.EventType == EventType.Mouse)
                                {
                                    var currentMousePoint = NativeHelper.GetCursorPosition();
                                    LogHelper.Debug($"current X : {currentMousePoint.X} current Y : {currentMousePoint.Y}");
                                    Task.Delay(100);

                                    NativeHelper.SetForegroundWindow(process.Value.MainWindowHandle);

                                    var currentPosition = new Rect();
                                    NativeHelper.GetWindowRect(process.Value.MainWindowHandle, ref currentPosition);

                                    var movePositionX = save.ProcessInfo.Position.Left - currentPosition.Left;
                                    var widthRatio = currentPosition.Width * 1.0 / save.ProcessInfo.Position.Width;
                                    var widthPosition = (int)Math.Truncate((currentPosition.Width - save.ProcessInfo.Position.Width) * widthRatio);
                                    movePositionX += widthPosition;

                                    var movePositionY = currentPosition.Top - save.ProcessInfo.Position.Top;
                                    var heightRatio = currentPosition.Height * 1.0 / save.ProcessInfo.Position.Height;
                                    var heightPosition = (int)Math.Truncate((currentPosition.Height - save.ProcessInfo.Position.Height) * heightRatio);

                                    var positionX = (int)(Math.Abs(save.MonitorInfo.Rect.Left - save.MousePoint.Value.X + movePositionX) * (65535 / SystemParameters.VirtualScreenWidth));
                                    var positionY = (int)(Math.Abs(save.MonitorInfo.Rect.Top - save.MousePoint.Value.Y - movePositionY) * (65535 / SystemParameters.VirtualScreenHeight));
                                    ObjectExtensions.GetInstance<InputManager>().Mouse.MoveMouseToVirtualDesktop(positionX, positionY);
                                    ObjectExtensions.GetInstance<InputManager>().Mouse.LeftButtonClick();

                                    //NativeHelper.SetWindowPos(process.Value.MainWindowHandle, currentPosition);

                                    positionX = (int)(Math.Abs(save.MonitorInfo.Rect.Left - currentMousePoint.X) * (65535 / SystemParameters.VirtualScreenWidth));
                                    positionY = (int)(Math.Abs(save.MonitorInfo.Rect.Top + currentMousePoint.Y) * (65535 / SystemParameters.VirtualScreenHeight));
                                    ObjectExtensions.GetInstance<InputManager>().Mouse.MoveMouseToVirtualDesktop(positionX, positionY);
                                }
                                else if(save.EventType == EventType.Keyboard)
                                {
                                    var commands = save.KeyboardCmd.Split('+');
                                    var modifiedKey = commands.Where(r =>
                                    {
                                        var keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), $"{r}", true);
                                        return keyCode.IsExtendedKey();
                                    }).Select(r =>
                                    {
                                        var keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), $"{r}", true);
                                        return keyCode;
                                    });

                                    var keys = commands.Where(r =>
                                    {
                                        var keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), r, true);
                                        return !keyCode.IsExtendedKey();
                                    }).Select(r =>
                                    {
                                        var keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), r, true);
                                        return keyCode;
                                    });
                                    ObjectExtensions.GetInstance<InputManager>().Keyboard.ModifiedKeyStroke(modifiedKey, keys);
                                }
                                NativeHelper.SetForegroundWindow(hWndActive);
                            }
                        }
                    }
                    Task.Delay(500);
                }
                task.SetResult(Task.CompletedTask);
            });
            return task.Task;
        }
    }
}
