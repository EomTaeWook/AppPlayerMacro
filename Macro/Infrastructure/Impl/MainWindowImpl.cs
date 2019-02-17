﻿using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Manager;
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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using Utils;
using Utils.Extensions;
using Utils.Infrastructure;
using EventType = Macro.Models.EventType;
using InputManager = Macro.Infrastructure.Manager.InputManager;
using Message = Utils.Document.Message;
using Point = System.Windows.Point;
using Rect = Utils.Infrastructure.Rect;
using Version = Macro.Infrastructure.Version;

namespace Macro
{
    public partial class MainWindow : MetroWindow
    {
        private Random _random;
        private TaskQueue _taskQueue;
        private string _path;
        private KeyValuePair<string, Process>[] _processes;
        private KeyValuePair<string, Process>? _fixProcess;
        private IConfig _config;
        private Bitmap _bitmap;
        private List<CaptureView> _captureViews;
        public MainWindow()
        {
            _random = new Random();
            _taskQueue = new TaskQueue();
            _captureViews = new List<CaptureView>();

            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
        
        private void Init()
        {
            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor > 1)
            {
                NativeHelper.SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
            }
            else
            {
                this.MessageShow("Error", DocumentHelper.Get(Message.FailedOSVersion));
            }
            foreach(var item in DisplayHelper.MonitorInfo())
            {
                _captureViews.Add(new CaptureView(item));
            }
            _config = ObjectExtensions.GetInstance<IConfig>();

            Refresh();
            ProcessManager.AddJob(OnProcessCallback);
            _taskQueue.Enqueue(SaveLoad, _path);
        }

        private void Refresh()
        {
            _path = _config.SavePath;
            if (string.IsNullOrEmpty(_path))
                _path = ConstHelper.DefaultSavePath;
            else
                _path += @"\";
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
            _path = $"{_path}{ConstHelper.DefaultSaveFile}";

            _processes = Process.GetProcesses().Where(r=>r.MainWindowHandle != IntPtr.Zero)
                                                .Select(r => new KeyValuePair<string, Process>(r.ProcessName, r))
                                                .OrderBy(r=>r.Key).ToArray();
            comboProcess.ItemsSource = _processes;
            comboProcess.DisplayMemberPath = "Key";
            comboProcess.SelectedValuePath = "Value";

            var labels = ObjectExtensions.FindChildren<Label>(this);
            foreach (var label in labels)
            {
                BindingOperations.GetBindingExpressionBase(label, ContentProperty).UpdateTarget();
            }
            var buttons = ObjectExtensions.FindChildren<Button>(this);
            foreach (var button in buttons)
            {
                if (button.Equals(btnSetting) || button.Content == null || !(button.Content is string))
                    continue;

                BindingOperations.GetBindingExpressionBase(button, ContentProperty).UpdateTarget();
            }
            var checkBoxs = ObjectExtensions.FindChildren<CheckBox>(this);
            foreach (var checkBox in checkBoxs)
            {
                if (checkBox.Content is string || checkBox.Content == null)
                    continue;

                BindingOperations.GetBindingExpressionBase(checkBox, ContentProperty).UpdateTarget();
            }
            BindingOperations.GetBindingExpressionBase(this, TitleProperty).UpdateTarget();
            configView.Clear();
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
            if (model.EventType == EventType.Mouse && model.MouseTriggerInfo.MouseInfoEventType == MouseEventType.None)
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

            foreach(var item in _captureViews)
            {
                item.ShowActivate();
            }
        }
        private void Clear()
        {
            btnDelete.Visibility = Visibility.Collapsed;
            _bitmap = null;
            captureImage.Background = System.Windows.Media.Brushes.White;
            configView.Clear();
        }
        private Task Delete()
        {
            configView.CurrentRemove();
            if (File.Exists(_path))
            {
                File.Delete(_path);
                using (var fs = new FileStream(_path, FileMode.CreateNew))
                {
                    foreach (var data in this.configView.DataContext<Models.ViewModel.ConfigEventViewModel>().TriggerSaves)
                    {
                        var bytes = ObjectSerializer.SerializeObject(data);
                        fs.Write(bytes, 0, bytes.Count());
                    }
                    fs.Close();
                }
            }
            return Task.CompletedTask;
        }
        private Task Save()
        {
            if (File.Exists(_path))
                File.Delete(_path);
            using (var fs = new FileStream(_path, FileMode.OpenOrCreate))
            {
                var saves = (configView.DataContext as Models.ViewModel.ConfigEventViewModel).TriggerSaves;
                foreach (var data in saves)
                {
                    var bytes = ObjectSerializer.SerializeObject(data);
                    fs.Write(bytes, 0, bytes.Count());
                }
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
                    configView.BindingItems(models);
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
        private void VersionCheck()
        {
            if (!_config.VersionCheck)
                return;
            Version version = null;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(ConstHelper.VersionUrl);
                using (var response = request.GetResponse())
                {
                    using (var stream = new StreamReader(response.GetResponseStream()))
                    {
                        version = JsonHelper.DeserializeObject<Version>(stream.ReadToEnd());
                    }
                }
            }
            catch(Exception ex)
            {
                LogHelper.Warning(ex.Message);
            }
            
            if(version != null)
            {
                if(version.CompareTo(Version.CurrentVersion) > 0)
                {
                    if(this.MessageShow("Infomation", DocumentHelper.Get(Message.NewVersion), MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative) == MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
                    {
                        if(File.Exists("Patcher.exe"))
                        {
                            var param = $"{Version.CurrentVersion.Major}.{Version.CurrentVersion.Minor}.{Version.CurrentVersion.Build} " +
                                $"{version.Major}.{version.Minor}.{version.Build}";
                            Process.Start("Patcher.exe", param);
                            Application.Current.Shutdown();
                        }
                        else
                        {
                            Process.Start(ConstHelper.ReleaseUrl);
                        }
                    }
                }
            }
        }
        private void ImageTriggerProcess(IntPtr hWnd, Point location, EventTriggerModel model)
        {
            var currentPosition = new Rect();
            NativeHelper.GetWindowRect(hWnd, ref currentPosition);

            var targetFactorX = 1.0F;
            var targetFactorY = 1.0F;
            var factor = NativeHelper.GetSystemDpi();
            foreach (var monitor in DisplayHelper.MonitorInfo())
            {
                if (monitor.Rect.IsContain(currentPosition))
                {
                    targetFactorX = monitor.Dpi.X / (factor.X * targetFactorX);
                    targetFactorY = monitor.Dpi.Y / (factor.Y * targetFactorY);
                    break;
                }
            }
            var position = new Point()
            {
                X = (location.X + model.MouseTriggerInfo.StartPoint.X) * targetFactorX,
                Y = (location.Y + model.MouseTriggerInfo.StartPoint.Y) * targetFactorY
            };

            LogHelper.Debug($">>>>Image Location X : {position.X} Location Y : {position.Y}");
            NativeHelper.PostMessage(hWnd, WindowMessage.LButtonDown, 1, position.ToLParam());
            Task.Delay(100).Wait();
            NativeHelper.PostMessage(hWnd, WindowMessage.LButtonUp, 0, position.ToLParam());
        }
        private void MouseTriggerProcess(IntPtr hWnd, Point location, EventTriggerModel model)
        {
            var currentPosition = new Rect();
            NativeHelper.GetWindowRect(hWnd, ref currentPosition);

            var targetFactorX = 1.0F;
            var targetFactorY = 1.0F;

            foreach (var monitor in DisplayHelper.MonitorInfo())
            {
                if (monitor.Rect.IsContain(currentPosition))
                {
                    targetFactorX = monitor.Dpi.X / (model.MonitorInfo.Dpi.X * targetFactorX);
                    targetFactorY = monitor.Dpi.Y / (model.MonitorInfo.Dpi.Y * targetFactorY);
                    break;
                }
            }
            var mousePosition = new Point()
            {
                X = Math.Abs(model.ProcessInfo.Position.Left + (model.MouseTriggerInfo.StartPoint.X + location.X) * -1) * targetFactorX,
                Y = Math.Abs(model.ProcessInfo.Position.Top + (model.MouseTriggerInfo.StartPoint.Y + location.Y) * -1) * targetFactorY
            };

            if (model.MouseTriggerInfo.MouseInfoEventType == MouseEventType.LeftClick)
            {
                LogHelper.Debug($">>>>LMouse Save Position X : {model.MouseTriggerInfo.StartPoint.X} Save Position Y : {model.MouseTriggerInfo.StartPoint.Y} Target X : { mousePosition.X } Target Y : { mousePosition.Y }");
                NativeHelper.PostMessage(hWnd, WindowMessage.LButtonDown, 1, mousePosition.ToLParam());
                Task.Delay(100).Wait();
                NativeHelper.PostMessage(hWnd, WindowMessage.LButtonUp, 0, mousePosition.ToLParam());
            }
            else if (model.MouseTriggerInfo.MouseInfoEventType == MouseEventType.RightClick)
            {
                LogHelper.Debug($">>>>RMouse Save Position X : {model.MouseTriggerInfo.StartPoint.X} Save Position Y : {model.MouseTriggerInfo.StartPoint.Y} Target X : { mousePosition.X } Target Y : { mousePosition.Y }");
                NativeHelper.PostMessage(hWnd, WindowMessage.RButtonDown, 1, mousePosition.ToLParam());
                Task.Delay(100).Wait();
                NativeHelper.PostMessage(hWnd, WindowMessage.RButtonDown, 0, mousePosition.ToLParam());
            }
            else if (model.MouseTriggerInfo.MouseInfoEventType == MouseEventType.Drag)
            {
                LogHelper.Debug($">>>>Drag Mouse Save Position X : {model.MouseTriggerInfo.StartPoint.X} Save Position Y : {model.MouseTriggerInfo.StartPoint.Y} Target X : { mousePosition.X } Target Y : { mousePosition.Y }");
                NativeHelper.PostMessage(hWnd, WindowMessage.LButtonDown, 1, mousePosition.ToLParam());
                Task.Delay(100).Wait();
                for (int i = 0; i < model.MouseTriggerInfo.MiddlePoint.Count; ++i)
                {
                    mousePosition = new Point()
                    {
                        X = Math.Abs(model.ProcessInfo.Position.Left + model.MouseTriggerInfo.MiddlePoint[i].X * -1) * targetFactorX,
                        Y = Math.Abs(model.ProcessInfo.Position.Top + model.MouseTriggerInfo.MiddlePoint[i].Y * -1) * targetFactorY
                    };
                    NativeHelper.PostMessage(hWnd, WindowMessage.MouseMove, 1, mousePosition.ToLParam());
                    Task.Delay(100).Wait();
                }
                mousePosition = new Point()
                {
                    X = Math.Abs(model.ProcessInfo.Position.Left + model.MouseTriggerInfo.EndPoint.X * -1) * targetFactorX,
                    Y = Math.Abs(model.ProcessInfo.Position.Top + model.MouseTriggerInfo.EndPoint.Y * -1) * targetFactorY
                };
                NativeHelper.PostMessage(hWnd, WindowMessage.MouseMove, 1, mousePosition.ToLParam());
                Task.Delay(100).Wait();
                NativeHelper.PostMessage(hWnd, WindowMessage.LButtonUp, 0, mousePosition.ToLParam());
                LogHelper.Debug($">>>>Drag Mouse Save Position X : {model.MouseTriggerInfo.EndPoint.X} Save Position Y : {model.MouseTriggerInfo.EndPoint.Y} Target X : { mousePosition.X } Target Y : { mousePosition.Y }");
            }
        }
        private void KeyboardTriggerProcess(IntPtr hWnd, EventTriggerModel model)
        {
            var childs = NativeHelper.GetChildHandles(hWnd);
            var hWndActive = NativeHelper.GetForegroundWindow();

            Task.Delay(100).Wait();
            NativeHelper.SetForegroundWindow(hWnd);
            var inputs = model.KeyboardCmd.ToUpper().Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
            var modifiedKey = inputs.Where(r =>
            {
                if(Enum.TryParse($"{r}", out KeyCode keyCode))
                    return keyCode.IsExtendedKey();
                return false;
            }).Select(r =>
            {
                Enum.TryParse($"{r}", out KeyCode keyCode);
                return keyCode;
            }).ToArray();

            var command = new List<char>();
            foreach(var input in inputs)
            {
                if (Enum.TryParse(input, out KeyCode keyCode))
                {
                    if (!keyCode.IsExtendedKey())
                    {
                        for (int i = 0; i < input.Count(); i++)
                            command.Add(input[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < input.Count(); i++)
                        command.Add(input[i]);
                }
            }
            var keys = command.Where(r=>
            {
                if (Enum.TryParse($"KEY_{r}", out KeyCode keyCode))
                    return !keyCode.IsExtendedKey();
                return false;
            }).Select(r =>
            {
                Enum.TryParse($"KEY_{r}", out KeyCode keyCode);
                return keyCode;
            }).ToArray();

            ObjectExtensions.GetInstance<InputManager>().Keyboard.ModifiedKeyStroke(modifiedKey, keys);
            Task.Delay(100).Wait();
            LogHelper.Debug($">>>>Keyboard Event");
            //NativeHelper.SetForegroundWindow(hWndActive);
        }
        private Tuple<float, float> CalculateFactor(IntPtr hWnd, EventTriggerModel model, bool isDynamic)
        {
            var currentPosition = new Rect();
            NativeHelper.GetWindowRect(hWnd, ref currentPosition);
            var factor = NativeHelper.GetSystemDpi();
            var factorX = 1.0F;
            var factorY = 1.0F;
            if (isDynamic)
            {
                foreach (var monitor in DisplayHelper.MonitorInfo())
                {
                    if (monitor.Rect.IsContain(currentPosition))
                    {
                        factorX = monitor.Dpi.X * factorX / model.MonitorInfo.Dpi.X * (factor.X * factorX / monitor.Dpi.X);
                        factorY = monitor.Dpi.Y * factorY / model.MonitorInfo.Dpi.Y * (factor.X * factorY / monitor.Dpi.Y);
                        break;
                    }
                }
            }
            else
            {
                foreach (var monitor in DisplayHelper.MonitorInfo())
                {
                    if (monitor.Rect.IsContain(currentPosition))
                    {
                        factorX = monitor.Dpi.X * factorX / ConstHelper.DefaultDPI;
                        factorY = monitor.Dpi.Y * factorY / ConstHelper.DefaultDPI;
                        break;
                    }
                }
            }
            return Tuple.Create(factorX, factorY);
        }
        private async Task<bool> TriggerProcess(EventTriggerModel model, CancellationToken token)
        {
            var isExcute = false;
            KeyValuePair<string, Process>[] processes = null;
            
            Dispatcher.Invoke(() =>
            {
                if (_fixProcess.HasValue)
                    processes = new KeyValuePair<string, Process>[] { _fixProcess.Value };
                else
                    processes = _processes.Where(r => r.Key.Equals(model.ProcessInfo.ProcessName)).ToArray();
            });
            IntPtr hWnd = IntPtr.Zero;
            var applciationData = ObjectExtensions.GetInstance<ApplicationDataManager>().Find(model.ProcessInfo.ProcessName) ?? new ApplicationData();
            for (int i=0; i<processes.Length; ++i)
            {
                var factor = CalculateFactor(processes[i].Value.MainWindowHandle, model, applciationData.IsDynamic);

                if (string.IsNullOrEmpty(applciationData.HandleName))
                {
                    hWnd = processes[i].Value.MainWindowHandle;
                }
                else
                {
                    var item = NativeHelper.GetChildHandles(processes[i].Value.MainWindowHandle).Where(r => r.Item1.Equals(applciationData.HandleName)).FirstOrDefault();

                    if(item != null)
                        hWnd = item.Item2;
                    else
                        hWnd = processes[i].Value.MainWindowHandle;
                }
                
                if (model.RepeatInfo.RepeatType == RepeatType.Search && model.SubEventTriggers.Count > 0)
                {
                    var count = model.RepeatInfo.Count;
                    while(DisplayHelper.ProcessCapture(processes.ElementAt(i).Value, out Bitmap bmp, applciationData.IsDynamic) && count-- > 0)
                    {
                        var targetBmp = model.Image.Resize((int)Math.Truncate(model.Image.Width * factor.Item1), (int)Math.Truncate(model.Image.Height * factor.Item2));
                        var similarity = OpenCVHelper.Search(bmp, targetBmp, out Point location, _config.SearchResultDisplay);
                        LogHelper.Debug($"RepeatType[Search : {count}] : >>>> Similarity : {similarity} % max Loc : X : {location.X} Y: {location.Y}");
                        Dispatcher.Invoke(() =>
                        {
                            captureImage.Background = new ImageBrush(bmp.ToBitmapSource());
                        });

                        if (!await TokenCheckDelayAsync(model.AfterDelay, token) || similarity > _config.Similarity)
                            break;
                        for (int ii = 0; ii < model.SubEventTriggers.Count; ++ii)
                        {
                            await TriggerProcess(model.SubEventTriggers[ii], token);
                            if (token.IsCancellationRequested)
                                break;
                        }
                        factor = CalculateFactor(processes[i].Value.MainWindowHandle, model, applciationData.IsDynamic);
                    }
                }
                else
                {
                    if (DisplayHelper.ProcessCapture(processes.ElementAt(i).Value, out Bitmap bmp, applciationData.IsDynamic))
                    {
                        var targetBmp = model.Image.Resize((int)Math.Truncate(model.Image.Width * factor.Item1), (int)Math.Truncate(model.Image.Height * factor.Item2));
                        var similarity = OpenCVHelper.Search(bmp, targetBmp, out Point location, _config.SearchResultDisplay);
                        LogHelper.Debug($"Similarity : {similarity} % max Loc : X : {location.X} Y: {location.Y}");
                        Dispatcher.Invoke(() =>
                        {
                            captureImage.Background = new ImageBrush(bmp.ToBitmapSource());
                        });
                        if (similarity > _config.Similarity)
                        {
                            if (model.SubEventTriggers.Count > 0)
                            {
                                if (model.RepeatInfo.RepeatType == RepeatType.Count || model.RepeatInfo.RepeatType == RepeatType.Once)
                                {
                                    for (int ii = 0; ii < model.RepeatInfo.Count; ++ii)
                                    {
                                        if (!await TokenCheckDelayAsync(model.AfterDelay, token))
                                            break;
                                        for (int iii = 0; iii < model.SubEventTriggers.Count; ++iii)
                                        {
                                            await TriggerProcess(model.SubEventTriggers[iii], token);
                                            if (token.IsCancellationRequested)
                                                break;
                                        }
                                    }
                                }
                                else if (model.RepeatInfo.RepeatType == RepeatType.NoSearch)
                                {
                                    while (await TokenCheckDelayAsync(model.AfterDelay, token))
                                    {
                                        isExcute = false;
                                        for (int ii = 0; ii < model.SubEventTriggers.Count; ++ii)
                                        {
                                            var childExcute = await TriggerProcess(model.SubEventTriggers[ii], token);
                                            if (token.IsCancellationRequested)
                                                break;
                                            if (!isExcute && childExcute)
                                                isExcute = childExcute;
                                        }
                                        if (!isExcute)
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                isExcute = true;
                                if (model.EventType == EventType.Mouse)
                                {
                                    location.X = applciationData.OffsetX;
                                    location.Y = applciationData.OffsetY;
                                    MouseTriggerProcess(hWnd, location, model);
                                }
                                else if (model.EventType == EventType.Image)
                                {
                                    var percentage = _random.NextDouble();

                                    location.X = location.X + targetBmp.Width * percentage;
                                    location.Y = location.Y + targetBmp.Height * percentage;
                                    ImageTriggerProcess(hWnd, location, model);
                                }
                                else if(model.EventType == EventType.RelativeToImage)
                                {
                                    location.X = location.X + (targetBmp.Width / 2);
                                    location.Y = location.Y + (targetBmp.Height / 2);
                                    ImageTriggerProcess(hWnd, location, model);
                                }
                                else if (model.EventType == EventType.Keyboard)
                                {
                                    KeyboardTriggerProcess(processes.ElementAt(i).Value.MainWindowHandle, model);
                                }
                                if (!await TokenCheckDelayAsync(model.AfterDelay, token))
                                    break;
                            }
                        }
                    }
                }                
            }
            await TokenCheckDelayAsync(_config.ItemDelay, token);
            return isExcute;
        }
        private async Task<bool> TokenCheckDelayAsync(int millisecondsDelay, CancellationToken token)
        {
            try
            {
                if (millisecondsDelay > 0)
                    await Task.Delay(millisecondsDelay, token);
            }
            catch (TaskCanceledException ex)
            {
                LogHelper.Debug(ex.Message);
            }
            catch (AggregateException ex)
            {
                LogHelper.Debug(ex.Message);
            }
            return !token.IsCancellationRequested;
        }
        private async Task OnProcessCallback(CancellationToken token)
        {
            List<EventTriggerModel> saves = null;
            
            Dispatcher.Invoke(() => 
            {
                saves = configView.TriggerSaves;
            });
            if(saves != null)
            {
                foreach(var save in saves)
                {
                    await TriggerProcess(save, token);
                    if (token.IsCancellationRequested)
                        break;
                }
                await TokenCheckDelayAsync(_config.Period, token);
            }
        }
    }
}
