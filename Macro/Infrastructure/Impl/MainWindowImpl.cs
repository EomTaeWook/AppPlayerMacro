using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Impl;
using Macro.Infrastructure.Manager;
using Macro.Infrastructure.Serialize;
using Macro.Models;
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
using System.Windows.Threading;
using Utils;
using Utils.Extensions;
using Utils.Infrastructure;
using Message = Utils.Document.Message;
using Point = System.Windows.Point;
using Rect = Utils.Infrastructure.Rect;
using Version = Macro.Infrastructure.Version;

namespace Macro
{
    public partial class MainWindow : MetroWindow
    {
        private readonly TaskQueue _taskQueue;
        private KeyValuePair<string, Process>[] _processes;
        private KeyValuePair<string, Process>? _fixProcess;
        private IConfig _config;
        
        private CancellationTokenSource tokenSource = null;
        private string _savePath;
        private readonly Dictionary<object, SaveFileLoadModel> _viewMap;

        public MainWindow()
        {
            _taskQueue = new TaskQueue();
            _viewMap = new Dictionary<object, SaveFileLoadModel>();
            
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void Init()
        {
            if (Environment.OSVersion.Version >= new System.Version(6,1,0))
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
                Process.GetCurrentProcess().Kill();
            }
            _config = ObjectExtensions.GetInstance<IConfig>();

            Refresh();

            var _saveFileNames = new Tuple<string, string>[]
            {
                Tuple.Create(ConstHelper.DefaultSaveFileName, ConstHelper.DefaultSaveCacheFile),
                Tuple.Create(ConstHelper.DefaultGameSaveFileName, ConstHelper.DefaultGameCacheFile),
            };
            for (int i=0; i<tab_content.Items.Count; i++)
            {
                var view = (tab_content.Items[i] as MetroTabItem).Content;
                var model = new SaveFileLoadModel()
                {
                    View = view as BaseContentView,
                    CacheFilePath = Path.Combine(_savePath, _saveFileNames[i].Item2),
                    SaveFilePath = Path.Combine(_savePath, _saveFileNames[i].Item1),
                };
                _viewMap.Add(view, model);
                _taskQueue.Enqueue(SaveFileLoad, model);
            }
        }
        
        private void Refresh()
        {
            _savePath = _config.SavePath;
            if (string.IsNullOrEmpty(_config.SavePath))
                _savePath = ConstHelper.DefaultSavePath;
            else
                _savePath += @"\";
            if (!Directory.Exists(_savePath))
                Directory.CreateDirectory(_savePath);

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
        private void Clear()
        {
            for(int i=0; i< tab_content.Items.Count; i++)
            {
                if((tab_content.Items[i] as MetroTabItem).Content is BaseContentView view)
                {
                    view.Clear();
                }
            }
        }
        private bool Validate(EventTriggerModel model)
        {
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
                return true;
            }
            else
            {
                this.MessageShow("Error", DocumentHelper.Get(error));
                return false;
            }
            
        }
        private Task SaveFileLoad(object state)
        {
            if (state is SaveFileLoadModel model)
            {
                try
                {
                    var saveFiles = ObjectSerializer.DeserializeObject<EventTriggerModel>(File.ReadAllBytes(model.SaveFilePath));

                    if (ObjectExtensions.GetInstance<CacheDataManager>().CheckAndMakeCacheFile(saveFiles, model.CacheFilePath))
                    {
                        model.View.Save(saveFiles);
                    }
                    model.View.SaveDataBind(saveFiles);
                }
                catch (Exception ex)
                {
                    File.Delete(model.SaveFilePath);
                    LogHelper.Warning(ex);
                    Task.FromException(new FileLoadException(DocumentHelper.Get(Message.FailedLoadSaveFile)));
                }
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
        private async Task InvokeNextEventTriggerAsync(BaseContentView view, EventTriggerModel model, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            var processConfigModel = new ProcessConfigModel()
            {
                ItemDelay = _config.ItemDelay,
                SearchImageResultDisplay = _config.SearchResultDisplay,
                Processes = new List<Process>(),
                Token = token,
                Similarity = _config.Similarity,
            };

            Dispatcher.Invoke(() =>
            {
                if (_fixProcess.HasValue)
                {
                    processConfigModel.Processes.Add(_fixProcess.Value.Value);
                }
                else
                {
                    processConfigModel.Processes.AddRange(_processes.Where(r => r.Key.Equals(model.ProcessInfo.ProcessName)).Select(r => r.Value));
                }
            });

            var nextItem = await view.InvokeNextEventTriggerAsync(model, processConfigModel);
            if(nextItem!= null)
            {
                await _taskQueue.Enqueue(async () => await InvokeNextEventTriggerAsync(view, nextItem, token));
            }
        }
        private async Task ProcessStartAsync(object state)
        {
            //if (state is CancellationToken token)
            //{
            //    if (token.IsCancellationRequested == true)
            //        return;

            //    List<EventTriggerModel> models = new List<EventTriggerModel>();
            //    BaseContentView view = null;
            //    var selectView = _viewMap[tab_content.SelectedContent];
            //    if (selectView != null)
            //    {
            //        models = selectView.View.GetEnumerator().ToList();
            //        view = selectView.View;
            //    }
            //    if (view == null)
            //        return;

            //    foreach (var iter in models)
            //    {
            //        await _taskQueue.Enqueue(() =>
            //        {
            //            return Task.CompletedTask;
            //            //await InvokeNextEventTriggerAsync(view, iter, token);
            //        });

            //        if (token.IsCancellationRequested)
            //        {
            //            break;
            //        }
            //    }

            //    await TaskHelper.TokenCheckDelayAsync(_config.Period, token);

            //    await _taskQueue.Enqueue(ProcessStartAsync, token);
            //}

            //List<EventTriggerModel> saves = null;
            //if(state is CancellationToken token)
            //{
            //    if (token.IsCancellationRequested)
            //        return;
            //    Dispatcher.Invoke(() =>
            //    {
            //        //saves = configView.TriggerSaves;
            //    });

            //    if (saves != null)
            //    {
            //        foreach (var save in saves)
            //        {
            //            await _taskQueue.Enqueue(async () =>
            //            {
            //                var job = new JobModel()
            //                {
            //                    Model = save,
            //                    Token = token
            //                };
            //                await InvokeNextEventTriggerAsync(job);
            //            });
            //            if (token.IsCancellationRequested)
            //                break;
            //        }
            //        await TokenCheckDelayAsync(_config.Period, token);
            //    }
            //    await _taskQueue.Enqueue(ProcessStartAsync, token);
            //}
        }
        
        
        //private async Task<bool> TriggerProcess(EventTriggerModel model, CancellationToken token)
        //{
        //    var isExcute = false;
        //    KeyValuePair<string, Process>[] processes = null;
        //    Dispatcher.Invoke(() =>
        //    {
        //        if (_fixProcess.HasValue)
        //            processes = new KeyValuePair<string, Process>[] { _fixProcess.Value };
        //        else
        //            processes = _processes.Where(r => r.Key.Equals(model.ProcessInfo.ProcessName)).ToArray();
        //    });
        //    IntPtr hWnd = IntPtr.Zero;
        //    var applciationData = ObjectExtensions.GetInstance<ApplicationDataManager>().Find(model.ProcessInfo.ProcessName) ?? new ApplicationDataModel();
        //    for (int i = 0; i < processes.Length; ++i)
        //    {
        //        var factor = CalculateFactor(processes[i].Value.MainWindowHandle, model, applciationData.IsDynamic);

        //        if (string.IsNullOrEmpty(applciationData.HandleName))
        //        {
        //            hWnd = processes[i].Value.MainWindowHandle;
        //        }
        //        else
        //        {
        //            var item = NativeHelper.GetChildHandles(processes[i].Value.MainWindowHandle).Where(r => r.Item1.Equals(applciationData.HandleName)).FirstOrDefault();

        //            if (item != null)
        //                hWnd = item.Item2;
        //            else
        //                hWnd = processes[i].Value.MainWindowHandle;
        //        }

        //        if (model.RepeatInfo.RepeatType == RepeatType.Search && model.SubEventTriggers.Count > 0)
        //        {
        //            var count = model.RepeatInfo.Count;
        //            while (DisplayHelper.ProcessCapture(processes.ElementAt(i).Value, out Bitmap bmp, applciationData.IsDynamic) && count-- > 0)
        //            {
        //                var targetBmp = model.Image.Resize((int)Math.Truncate(model.Image.Width * factor.Item1.Item1), (int)Math.Truncate(model.Image.Height * factor.Item1.Item1));
        //                var similarity = OpenCVHelper.Search(bmp, targetBmp, out Point location, _config.SearchResultDisplay);
        //                LogHelper.Debug($"RepeatType[Search : {count}] : >>>> Similarity : {similarity} % max Loc : X : {location.X} Y: {location.Y}");
        //                Dispatcher.Invoke(() =>
        //                {
        //                    //captureImage.Background = new ImageBrush(bmp.ToBitmapSource());
        //                });

        //                if (!await TaskHelper.TokenCheckDelayAsync(model.AfterDelay, token) || similarity > _config.Similarity)
        //                    break;
        //                for (int ii = 0; ii < model.SubEventTriggers.Count; ++ii)
        //                {
        //                    await TriggerProcess(model.SubEventTriggers[ii], token);
        //                    if (token.IsCancellationRequested)
        //                        break;
        //                }
        //                factor = CalculateFactor(processes[i].Value.MainWindowHandle, model, applciationData.IsDynamic);
        //            }
        //        }
        //        else
        //        {
        //            if (DisplayHelper.ProcessCapture(processes.ElementAt(i).Value, out Bitmap bmp, applciationData.IsDynamic))
        //            {
        //                var targetBmp = model.Image.Resize((int)Math.Truncate(model.Image.Width * factor.Item1.Item1), (int)Math.Truncate(model.Image.Height * factor.Item1.Item2));
        //                var similarity = OpenCVHelper.Search(bmp, targetBmp, out Point location, _config.SearchResultDisplay);
        //                LogHelper.Debug($"Similarity : {similarity} % max Loc : X : {location.X} Y: {location.Y}");
        //                Dispatcher.Invoke(() =>
        //                {
        //                    //captureImage.Background = new ImageBrush(bmp.ToBitmapSource());
        //                });
        //                if (similarity > _config.Similarity)
        //                {
        //                    if (model.SubEventTriggers.Count > 0)
        //                    {
        //                        if (model.RepeatInfo.RepeatType == RepeatType.Count || model.RepeatInfo.RepeatType == RepeatType.Once)
        //                        {
        //                            for (int ii = 0; ii < model.RepeatInfo.Count; ++ii)
        //                            {
        //                                if (!await TaskHelper.TokenCheckDelayAsync(model.AfterDelay, token))
        //                                    break;
        //                                for (int iii = 0; iii < model.SubEventTriggers.Count; ++iii)
        //                                {
        //                                    await TriggerProcess(model.SubEventTriggers[iii], token);
        //                                    if (token.IsCancellationRequested)
        //                                        break;
        //                                }
        //                            }
        //                        }
        //                        else if (model.RepeatInfo.RepeatType == RepeatType.NoSearch)
        //                        {
        //                            while (await TaskHelper.TokenCheckDelayAsync(model.AfterDelay, token))
        //                            {
        //                                isExcute = false;
        //                                for (int ii = 0; ii < model.SubEventTriggers.Count; ++ii)
        //                                {
        //                                    var childExcute = await TriggerProcess(model.SubEventTriggers[ii], token);
        //                                    if (token.IsCancellationRequested)
        //                                        break;
        //                                    if (!isExcute && childExcute)
        //                                        isExcute = childExcute;
        //                                }
        //                                if (!isExcute)
        //                                    break;
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        isExcute = true;
        //                        if (model.EventType == EventType.Mouse)
        //                        {
        //                            location.X = applciationData.OffsetX;
        //                            location.Y = applciationData.OffsetY;
        //                            MouseTriggerProcess(hWnd, location, model, factor.Item2);
        //                        }
        //                        else if (model.EventType == EventType.Image)
        //                        {
        //                            var percentage = _random.NextDouble();

        //                            location.X = ((location.X + applciationData.OffsetX) / factor.Item2.Item1) + (targetBmp.Width / factor.Item2.Item1 * percentage);
        //                            location.Y = ((location.Y + applciationData.OffsetY) / factor.Item2.Item2) + (targetBmp.Height / factor.Item2.Item1 * percentage);
        //                            ImageTriggerProcess(hWnd, location, model);
        //                        }
        //                        else if (model.EventType == EventType.RelativeToImage)
        //                        {
        //                            location.X = ((location.X + applciationData.OffsetX) / factor.Item2.Item1) + (targetBmp.Width / factor.Item2.Item1 / 2);
        //                            location.Y = ((location.Y + applciationData.OffsetY) / factor.Item2.Item2) + (targetBmp.Height / factor.Item2.Item2 / 2);
        //                            ImageTriggerProcess(hWnd, location, model);
        //                        }
        //                        else if (model.EventType == EventType.Keyboard)
        //                        {
        //                            KeyboardTriggerProcess(processes.ElementAt(i).Value.MainWindowHandle, model);
        //                        }
        //                        if (!await TaskHelper.TokenCheckDelayAsync(model.AfterDelay, token))
        //                            break;

        //                        if (model.EventToNext > 0 && model.TriggerIndex != model.EventToNext)
        //                        {
        //                            EventTriggerModel nextModel = null;
        //                            Dispatcher.Invoke(() =>
        //                            {
        //                                nextModel = ObjectExtensions.GetInstance<CacheDataManager>().GetEventTriggerModel(model.EventToNext);
        //                            });

        //                            if (nextModel != null)
        //                            {
        //                                LogHelper.Debug($">>>>Next Move Event : CurrentIndex [ {model.TriggerIndex} ] NextIndex [ {nextModel.TriggerIndex} ] ");
        //                                var job = new JobModel()
        //                                {
        //                                    Model = nextModel,
        //                                    Token = token
        //                                };
        //                                await _taskQueue.Enqueue(InvokeNextEventTriggerAsync, job);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    await TaskHelper.TokenCheckDelayAsync(_config.ItemDelay, token);
        //    return isExcute;
        //}
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

                        if (model.EventType == EventType.Mouse)
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
        private async Task InvokeNextEventTriggerAsync(object state)
        {
            if (state is JobModel job)
            {
                if (job.Token.IsCancellationRequested)
                    return;
                //await TriggerProcess(job.Model, job.Token);
            }
        }
    }
}
