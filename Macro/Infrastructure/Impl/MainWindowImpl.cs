using Macro.Extensions;
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
        private readonly Random _random;
        private readonly TaskQueue _taskQueue;
        private string _path;
        private KeyValuePair<string, Process>[] _processes;
        private KeyValuePair<string, Process>? _fixProcess;
        private IConfig _config;
        private Bitmap _bitmap;
        private readonly List<CaptureView> _captureViews;
        private CancellationTokenSource tokenSource = null;

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
            if (Environment.OSVersion.Version >= new System.Version(6, 1, 0))
            {
                if (Environment.OSVersion.Version >= new System.Version(10, 0, 15063))
                {
                    NativeHelper.SetProcessDpiAwarenessContext(PROCESS_DPI_AWARENESS.PROCESS_DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
                }
                else
                {
                    NativeHelper.SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
                }                
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
            
            _taskQueue.Enqueue(SaveFileLoad, _config.SavePath);
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
                if (checkBox.Content == null || !(checkBox.Content is string))
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
            btnAddSameContent.Visibility = Visibility.Collapsed;
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
        private Task SaveFile()
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
        private void Save()
        {
            var model = configView.CurrentTreeViewItem.DataContext<EventTriggerModel>();
            model.Image = _bitmap;

            if (model.EventType == EventType.RelativeToImage)
            {
                model.MouseTriggerInfo.StartPoint = new Point(configView.RelativePosition.X, configView.RelativePosition.Y);
            }

            var process = comboProcess.SelectedValue as Process;

            model.ProcessInfo = new ProcessInfo()
            {
                ProcessName = process?.ProcessName,
                Position = new Rect()
            };

            if (TryModelValidate(model, out Message error))
            {
                var rect = new Rect();

                NativeHelper.GetWindowRect(process.MainWindowHandle, ref rect);
                model.ProcessInfo.Position = rect;
                if (model.EventType != EventType.Mouse)
                {
                    foreach (var monitor in DisplayHelper.MonitorInfo())
                    {
                        if (monitor.Rect.IsContain(rect))
                        {
                            if (model.MonitorInfo != null)
                                model.Image = model.Image.Resize((int)(model.Image.Width * (monitor.Dpi.X * 1.0F / model.MonitorInfo.Dpi.X)), (int)(model.Image.Height * (monitor.Dpi.Y * 1.0F / model.MonitorInfo.Dpi.Y)));

                            model.MonitorInfo = monitor;
                            break;
                        }
                    }
                }
                ObjectExtensions.GetInstance<CacheDataManager>().MakeIndexTriggerModel(model);

                configView.InsertCurrentItem();

                _taskQueue.Enqueue(SaveFile).ContinueWith(task =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        Clear();
                    });
                });
            }
            else
            {
                this.MessageShow("Error", DocumentHelper.Get(error));
            }
        }
        private Task SaveFileLoad(object state)
        {
            if(state is string path)
            {
                var task = new TaskCompletionSource<Task>();
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        var models = ObjectSerializer.DeserializeObject<EventTriggerModel>(File.ReadAllBytes($@"{path}\{ConstHelper.DefaultSaveFile}"));
                        configView.BindingItems(models);
                        if (ObjectExtensions.GetInstance<CacheDataManager>().CheckAndMakeCacheFile(configView.TriggerSaves, path))
                        {
                            _taskQueue.Enqueue(SaveFile);
                        }
                        task.SetResult(Task.CompletedTask);
                    }
                    catch (Exception ex)
                    {
                        File.Delete($@"{path}\{ConstHelper.DefaultSaveFile}");
                        LogHelper.Warning(ex);
                        Task.FromException(new FileLoadException(DocumentHelper.Get(Message.FailedLoadSaveFile)));
                    }
                }, DispatcherPriority.Send);
                return task.Task;
            }
            return Task.CompletedTask;
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
                LogHelper.Warning(ex);
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
            var position = new Point()
            {
                X = location.X + model.MouseTriggerInfo.StartPoint.X,
                Y = location.Y + model.MouseTriggerInfo.StartPoint.Y
            };

            LogHelper.Debug($">>>>Image Location X : {position.X} Location Y : {position.Y}");
            NativeHelper.PostMessage(hWnd, WindowMessage.LButtonDown, 1, position.ToLParam());
            Task.Delay(100).Wait();
            NativeHelper.PostMessage(hWnd, WindowMessage.LButtonUp, 0, position.ToLParam());
        }
        private void MouseTriggerProcess(IntPtr hWnd, Point location, EventTriggerModel model, Tuple<float, float> factor)
        {
            var mousePosition = new Point()
            {
                X = Math.Abs(model.ProcessInfo.Position.Left + (model.MouseTriggerInfo.StartPoint.X + location.X) * -1) * factor.Item1,
                Y = Math.Abs(model.ProcessInfo.Position.Top + (model.MouseTriggerInfo.StartPoint.Y + location.Y) * -1) * factor.Item2
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
                        X = Math.Abs(model.ProcessInfo.Position.Left + model.MouseTriggerInfo.MiddlePoint[i].X * -1) * factor.Item1,
                        Y = Math.Abs(model.ProcessInfo.Position.Top + model.MouseTriggerInfo.MiddlePoint[i].Y * -1) * factor.Item2
                    };
                    NativeHelper.PostMessage(hWnd, WindowMessage.MouseMove, 1, mousePosition.ToLParam());
                    Task.Delay(100).Wait();
                }
                mousePosition = new Point()
                {
                    X = Math.Abs(model.ProcessInfo.Position.Left + model.MouseTriggerInfo.EndPoint.X * -1) * factor.Item1,
                    Y = Math.Abs(model.ProcessInfo.Position.Top + model.MouseTriggerInfo.EndPoint.Y * -1) * factor.Item2
                };
                NativeHelper.PostMessage(hWnd, WindowMessage.MouseMove, 1, mousePosition.ToLParam());
                Task.Delay(100).Wait();
                NativeHelper.PostMessage(hWnd, WindowMessage.LButtonUp, 0, mousePosition.ToLParam());
                LogHelper.Debug($">>>>Drag Mouse Save Position X : {model.MouseTriggerInfo.EndPoint.X} Save Position Y : {model.MouseTriggerInfo.EndPoint.Y} Target X : { mousePosition.X } Target Y : { mousePosition.Y }");
            }
            else if(model.MouseTriggerInfo.MouseInfoEventType == MouseEventType.Wheel)
            {
                LogHelper.Debug($">>>>Wheel Save Position X : {model.MouseTriggerInfo.StartPoint.X} Save Position Y : {model.MouseTriggerInfo.StartPoint.Y} Target X : { mousePosition.X } Target Y : { mousePosition.Y }");
                //NativeHelper.PostMessage(hWnd, WindowMessage.LButtonDown, 1, mousePosition.ToLParam());
                //Task.Delay(100).Wait();
                //NativeHelper.PostMessage(hWnd, WindowMessage.LButtonUp, 0, mousePosition.ToLParam());
                //NativeHelper.PostMessage(hWnd, WindowMessage.MouseWheel, ObjectExtensions.MakeWParam((uint)WindowMessage.MKControl, (uint)(model.MouseTriggerInfo.WheelData * -1)), 0);
                //var hwnd = NativeHelper.FindWindowEx(NativeHelper.FindWindow(null, "Test.txt - 메모장"), IntPtr.Zero, "Edit", null);
                //var p = new System.Drawing.Point(0, 0);
                NativeHelper.PostMessage(hWnd, WindowMessage.MouseWheel, ObjectExtensions.MakeWParam(0, model.MouseTriggerInfo.WheelData * ConstHelper.WheelDelta), mousePosition.ToLParam());
            }
        }
        private void KeyboardTriggerProcess(IntPtr hWnd, EventTriggerModel model)
        {
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
            NativeHelper.SetForegroundWindow(hWndActive);
        }
        private Tuple<Tuple<float, float>, Tuple<float, float>> CalculateFactor(IntPtr hWnd, EventTriggerModel model, bool isDynamic)
        {
            var currentPosition = new Rect();
            NativeHelper.GetWindowRect(hWnd, ref currentPosition);
            var factor = NativeHelper.GetSystemDPI();
            var factorX = 1.0F;
            var factorY = 1.0F;
            var positionFactorX = 1.0F;
            var positionFactorY = 1.0F;
            if (isDynamic)
            {
                foreach (var monitor in DisplayHelper.MonitorInfo())
                {
                    if (monitor.Rect.IsContain(currentPosition))
                    {
                        factorX = factor.X * factorX / model.MonitorInfo.Dpi.X;
                        factorY = factor.Y * factorY / model.MonitorInfo.Dpi.Y;

                        if(model.EventType == EventType.Mouse)
                        {
                            positionFactorX = positionFactorX * monitor.Dpi.X / model.MonitorInfo.Dpi.X;
                            positionFactorY = positionFactorY * monitor.Dpi.Y / model.MonitorInfo.Dpi.Y;
                        }
                        else
                        {
                            positionFactorX = positionFactorX * factor.X / monitor.Dpi.X;
                            positionFactorY = positionFactorY * factor.Y / monitor.Dpi.Y;
                        }
                        break;
                    }
                }
            }
            return Tuple.Create(Tuple.Create(factorX, factorY), Tuple.Create(positionFactorX, positionFactorY));
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
                        var targetBmp = model.Image.Resize((int)Math.Truncate(model.Image.Width * factor.Item1.Item1), (int)Math.Truncate(model.Image.Height * factor.Item1.Item1));
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
                        var targetBmp = model.Image.Resize((int)Math.Truncate(model.Image.Width * factor.Item1.Item1), (int)Math.Truncate(model.Image.Height * factor.Item1.Item2));
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
                                    MouseTriggerProcess(hWnd, location, model, factor.Item2);
                                }
                                else if (model.EventType == EventType.Image)
                                {
                                    var percentage = _random.NextDouble();

                                    location.X = ((location.X + applciationData.OffsetX) / factor.Item2.Item1) + (targetBmp.Width / factor.Item2.Item1 * percentage);
                                    location.Y = ((location.Y + applciationData.OffsetY) / factor.Item2.Item2) + (targetBmp.Height/ factor.Item2.Item2 * percentage);
                                    ImageTriggerProcess(hWnd, location, model);
                                }
                                else if(model.EventType == EventType.RelativeToImage)
                                {
                                    location.X = ((location.X + applciationData.OffsetX) / factor.Item2.Item1) + (targetBmp.Width / factor.Item2.Item1 / 2);
                                    location.Y = ((location.Y + applciationData.OffsetY) / factor.Item2.Item2) + (targetBmp.Height / factor.Item2.Item2 / 2);
                                    ImageTriggerProcess(hWnd, location, model);
                                }
                                else if (model.EventType == EventType.Keyboard)
                                {
                                    KeyboardTriggerProcess(processes.ElementAt(i).Value.MainWindowHandle, model);
                                }
                                if (!await TokenCheckDelayAsync(model.AfterDelay, token))
                                    break;

                                if (model.EventToNext > 0 && model.TriggerIndex != model.EventToNext)
                                {
                                    EventTriggerModel nextModel = null;
                                    Dispatcher.Invoke(() =>
                                    {
                                        nextModel = ObjectExtensions.GetInstance<CacheDataManager>().GetEventTriggerModel(model.EventToNext);
                                    });

                                    if (nextModel != null)
                                    {
                                        LogHelper.Debug($">>>>Next Move Event : CurrentIndex [ {model.TriggerIndex} ] NextIndex [ {nextModel.TriggerIndex} ] ");
                                        var job = new JobModel()
                                        {
                                            Model = nextModel,
                                            Token = token
                                        };
                                        await _taskQueue.Enqueue(InvokeNextEventTriggerAsync, job);
                                    }
                                }
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

        private async Task InvokeNextEventTriggerAsync(object state)
        {
            if (state is JobModel job)
            {
                if (job.Token.IsCancellationRequested)
                    return;
                await TriggerProcess(job.Model, job.Token);
            }
        }
        private async Task ProcessStartAsync(object state)
        {
            List<EventTriggerModel> saves = null;
            if(state is CancellationToken token)
            {
                if (token.IsCancellationRequested)
                    return;
                Dispatcher.Invoke(() =>
                {
                    saves = configView.TriggerSaves;
                });
                if (saves != null)
                {
                    foreach (var save in saves)
                    {
                        await _taskQueue.Enqueue(async () =>
                        {
                            var job = new JobModel()
                            {
                                Model = save,
                                Token = token
                            };
                            await InvokeNextEventTriggerAsync(job);
                        });
                        if (token.IsCancellationRequested)
                            break;
                    }
                    await TokenCheckDelayAsync(_config.Period, token);
                }
                await _taskQueue.Enqueue(ProcessStartAsync, token);
            }
        }
    }
}
