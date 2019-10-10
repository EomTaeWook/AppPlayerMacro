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
            {
                _savePath = ConstHelper.DefaultSavePath;
            }
            else
            {
                _savePath += @"\";
            }   
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }  

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

            Clear();
        }
        private bool TryModelValidate(GameEventTriggerModel model, out Message message)
        {
            message = Message.Success;
            model.KeyboardCmd = model.KeyboardCmd.Replace(" ", "");
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
            Dispatcher.Invoke(() =>
            {
                for (int i = 0; i < tab_content.Items.Count; i++)
                {
                    if ((tab_content.Items[i] as MetroTabItem).Content is BaseContentView view)
                    {
                        view.Clear();
                    }
                }
            });
            
        }
        private bool Validate(GameEventTriggerModel model)
        {
            var process = comboProcess.SelectedValue as Process;

            model.ProcessInfo = new ProcessInfo()
            {
                ProcessName = process?.ProcessName,
                Position = new Rect()
            };

            if (TryModelValidate(model, out Message error))
            {
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
            //if (state is SaveFileLoadModel model)
            //{
            //    try
            //    {
            //        var saveFiles = ObjectSerializer.DeserializeObject<EventTriggerModel>(File.ReadAllBytes(model.SaveFilePath));

            //        if (ObjectExtensions.GetInstance<CacheDataManager>().CheckAndMakeCacheFile(saveFiles, model.CacheFilePath))
            //        {
            //            model.View.Save(saveFiles);
            //        }
            //        model.View.SaveDataBind(saveFiles);
            //    }
            //    catch (Exception ex)
            //    {
            //        File.Delete(model.SaveFilePath);
            //        LogHelper.Warning(ex);
            //        Task.FromException(new FileLoadException(DocumentHelper.Get(Message.FailedLoadSaveFile)));
            //    }
            //}
            return Task.CompletedTask;
        }
        private void SettingProcessMonitorInfo(IBaseEventTriggerModel model, Process process)
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
        private async Task InvokeNextEventTriggerAsync(BaseContentView view, IBaseEventTriggerModel model, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            var processConfigModel = new ProcessConfigModel()
            {
                ItemDelay = _config.ItemDelay,
                SearchImageResultDisplay = _config.SearchImageResultDisplay,
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
            if (state is CancellationToken token)
            {
                if (token.IsCancellationRequested == true)
                    return;

                List<IBaseEventTriggerModel> models = new List<IBaseEventTriggerModel>();
                BaseContentView view = null;
                Dispatcher.Invoke(() => {
                    var selectView = _viewMap[tab_content.SelectedContent];
                    if (selectView != null)
                    {
                        models = selectView.View.GetEnumerator().ToList();
                        view = selectView.View;
                    }
                });

                if (view == null)
                    return;

                foreach (var iter in models)
                {
                    await _taskQueue.Enqueue(async () =>
                    {
                        await InvokeNextEventTriggerAsync(view, iter, token);
                    });

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                }

                await TaskHelper.TokenCheckDelayAsync(_config.Period, token);

                await _taskQueue.Enqueue(ProcessStartAsync, token);
            }

        }        
    }
}
