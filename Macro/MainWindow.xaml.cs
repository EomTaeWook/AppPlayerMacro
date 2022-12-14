using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Controller;
using Macro.Infrastructure.Manager;
using Macro.Models;
using Macro.View;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Utils;
using Utils.Document;
using Utils.Extensions;
using Utils.Infrastructure;
using Label = System.Windows.Controls.Label;
using Rect = Utils.Infrastructure.Rect;

namespace Macro
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private KeyValuePair<string, Process>[] _processes;
        private KeyValuePair<string, Process>? _fixProcess;
        private Config _config;
        private ContentView _contentView;
        private ContentController _contentController = new ContentController();
        public MainWindow()
        {
            InitializeComponent();
            _config = ServiceProviderManager.Instance.GetService<Config>();
            Loaded += MainWindow_Loaded;
        }
        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitEvent();
            Init();
            VersionCheck();
            this.ads.ShowAd(728, 90, "5ybbzi0gxwn0");
            ApplicationManager.Instance.Init();
        }
        private void InitEvent()
        {
            btnRefresh.Click += Button_Click;
            btnStart.Click += Button_Click;
            btnStop.Click += Button_Click;
            btnSetting.Click += Button_Click;
            btnGithub.Click += Button_Click;
            checkFix.Checked += CheckFix_Checked;
            checkFix.Unchecked += CheckFix_Checked;
            comboProcess.SelectionChanged += ComboProcess_SelectionChanged;

            KeyDown += MainWindow_KeyDown;

            NotifyHelper.ConfigChanged += NotifyHelper_ConfigChanged;
            NotifyHelper.TreeItemOrderChanged += NotifyHelper_TreeItemOrderChanged;
            NotifyHelper.SelectTreeViewChanged += NotifyHelper_SelectTreeViewChanged;
            NotifyHelper.EventTriggerOrderChanged += NotifyHelper_EventTriggerOrderChanged;
            NotifyHelper.SaveEventTriggerModel += NotifyHelper_SaveEventTriggerModel;
            NotifyHelper.DeleteEventTriggerModel += NotifyHelper_DeleteEventTriggerModelAsync;
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Escape)
            {
                Button_Click(btnStop, null);
            }
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
                ApplicationManager.ShowMessageDialog("Error", DocumentHelper.Get(Message.FailedOSVersion));
                Process.GetCurrentProcess().Kill();
            }
            Refresh();
            LoadDatas(GetSaveFilePath());
        }
        private string GetSaveFilePath()
        {
            if(string.IsNullOrEmpty(_config.SavePath) == true)
            {
                return $"{ConstHelper.DefaultSavePath}{ConstHelper.DefaultSaveFileName}";
            }
            else
            {
                return $"{_config.SavePath}\\{ConstHelper.DefaultSaveFileName}";
            }
        }

        private void Refresh()
        {
            FileInfo fileInfo = new FileInfo(GetSaveFilePath());
            if (fileInfo.Directory.Exists == false)
            {
                fileInfo.Directory.Create();
            }

            _processes = Process.GetProcesses().Where(r => r.MainWindowHandle != IntPtr.Zero)
                                                .Select(r => new KeyValuePair<string, Process>($"{r.ProcessName}", r))
                                                .OrderBy(r => r.Key).ToArray();
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
                {
                    continue;
                }
                    

                BindingOperations.GetBindingExpressionBase(button, ContentProperty).UpdateTarget();
            }
            var checkBoxs = ObjectExtensions.FindChildren<CheckBox>(this);
            foreach (var checkBox in checkBoxs)
            {
                if (checkBox.Content == null || !(checkBox.Content is string))
                {
                    continue;
                }

                BindingOperations.GetBindingExpressionBase(checkBox, ContentProperty).UpdateTarget();
            }
            foreach (var tab in tab_content.Items)
            {
                var tablItem = tab as TabItem;

                BindingOperations.GetBindingExpressionBase(tablItem, HeaderedContentControl.HeaderProperty).UpdateTarget();
            }
            BindingOperations.GetBindingExpressionBase(this, TitleProperty).UpdateTarget();

            Clear();

            foreach (var tab in tab_content.Items)
            {
                var tabItem = tab as TabItem;
                var tabView = tabItem.Content as ContentView;
                var key = tabView.Tag.ToString();
                if (key.Equals(_config.InitialTab.ToString()))
                {
                    tabItem.IsSelected = true;
                    _contentView = tabView;
                    break;
                }
            }

            _contentController.SetContentView(_contentView);
        }
        private void SettingProcessMonitorInfo(EventTriggerModel model, Process process)
        {
            if(process == null)
            {
                model.ProcessInfo = new ProcessInfo()
                {
                    ProcessName = string.Empty
                };
                return;
            }

            var rect = new Rect();
            NativeHelper.GetWindowRect(process.MainWindowHandle, ref rect);
            model.ProcessInfo = new ProcessInfo()
            {
                ProcessName = process.ProcessName,
                Position = rect
            };

            if (model.EventType != EventType.Mouse)
            {
                foreach (var monitor in DisplayHelper.MonitorInfo())
                {
                    if (monitor.Rect.IsContain(rect))
                    {
                        if(model.EventType != EventType.Mouse)
                        {
                            if (model.MonitorInfo != null)
                            {
                                model.Image = model.Image.Resize((int)(model.Image.Width * (monitor.Dpi.X * 1.0F / model.MonitorInfo.Dpi.X)), (int)(model.Image.Height * (monitor.Dpi.Y * 1.0F / model.MonitorInfo.Dpi.Y)));
                            }
                            model.MonitorInfo = monitor;
                            
                        }
                        if (model.RoiData != null)
                        {
                            model.RoiData.RoiRect.Left -= rect.Left - monitor.Rect.Left;
                            model.RoiData.RoiRect.Top -= rect.Top - monitor.Rect.Top;
                            model.RoiData.RoiRect.Right -= rect.Left - monitor.Rect.Left;
                            model.RoiData.RoiRect.Bottom -= rect.Top - monitor.Rect.Top;
                        }
                        break;
                    }
                }
            }    
        }
        public void Clear()
        {
            Dispatcher.Invoke(() =>
            {
                for (int i = 0; i < tab_content.Items.Count; i++)
                {
                    if ((tab_content.Items[i] as MetroTabItem).Content is ContentView view)
                    {
                        view.Clear();
                    }
                }
            });
        }
        
        private async void NotifyHelper_DeleteEventTriggerModelAsync(DeleteEventTriggerModelArgs obj)
        {
            _contentView.eventSettingView.RemoveCurrentItem();

            if (File.Exists(GetSaveFilePath()))
            {
                File.Delete(GetSaveFilePath());
            }

            var triggers = _contentView.eventSettingView.GetDataContext().TriggerSaves;

            await FileManager.Instance.Save(GetSaveFilePath(), triggers);

            Dispatcher.Invoke(() =>
            {
                Clear();
            });
        }
        
        private void NotifyHelper_SaveEventTriggerModel(SaveEventTriggerModelArgs obj)
        {
            var process = comboProcess.SelectedValue as Process;

            SettingProcessMonitorInfo(obj.CurrentEventTriggerModel, process);

            if (_contentController.Validate(obj.CurrentEventTriggerModel, out Message error))
            {
                Dispatcher.Invoke(async () =>
                {
                    CacheDataManager.Instance.MakeIndexTriggerModel(obj.CurrentEventTriggerModel);

                    _contentView.eventSettingView.InsertCurrentItem();
                    var triggers = _contentView.eventSettingView.GetDataContext().TriggerSaves;

                    await FileManager.Instance.Save(GetSaveFilePath(), triggers);

                    Clear();
                });
            }
            else
            {
                ApplicationManager.ShowMessageDialog("Error", DocumentHelper.Get(error));
            }
        }
        private async Task Save()
        {
            var triggers = _contentView.eventSettingView.GetDataContext().TriggerSaves;

            await FileManager.Instance.Save(GetSaveFilePath(), triggers);
        }
        private void NotifyHelper_TreeItemOrderChanged(EventTriggerOrderChangedEventArgs e)
        {
            if (File.Exists(GetSaveFilePath()))
            {
                File.Delete(GetSaveFilePath());
            }
            _ = Save();

            Clear();
        }
        private void NotifyHelper_EventTriggerOrderChanged(EventTriggerOrderChangedEventArgs obj)
        {
            obj.SelectedTreeViewItem.IsSelected = true;

            _ = Save();
        }

        private void ComboProcess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckFix_Checked(checkFix, null);

            if (comboProcess.SelectedItem is KeyValuePair<string, Process> item)
            {
                NotifyHelper.InvokeNotify(NotifyEventType.ComboProcessChanged, new ComboProcessChangedEventArgs()
                {
                    Process = item.Value,
                });
            }
        }
        private void CheckFix_Checked(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(checkFix))
            {
                if(checkFix.IsChecked.HasValue == false)
                {
                    _contentController.SetFixProcess(null);
                    _fixProcess = null;
                    return;
                }

                if (checkFix.IsChecked.Value)
                {
                    if (comboProcess.SelectedItem is KeyValuePair<string, Process> item)
                    {
                        _fixProcess = new KeyValuePair<string, Process>(item.Key, item.Value);
                        _contentController.SetFixProcess(item.Value);

                    }
                }
                else
                {
                    _contentController.SetFixProcess(null);
                    _fixProcess = null;
                }
            }
        }



        private void NotifyHelper_SelectTreeViewChanged(SelctTreeViewItemChangedEventArgs e)
        {
            if(e.TreeViewItem == null)
            {
                return;
            }

            var model = e.TreeViewItem.DataContext<EventTriggerModel>();

            if (model == null)
            {
                Clear();
                return;
            }

            if (_fixProcess == null)
            {
                var pair = comboProcess.Items.Cast<KeyValuePair<string, Process>>().Where(r => r.Key == model.ProcessInfo.ProcessName).FirstOrDefault();
                comboProcess.SelectedValue = pair.Value;
            }
            else
            {
                comboProcess.SelectedValue = _fixProcess.Value.Value;
            }
        }

        public async void LoadDatas(string path)
        {
            var loadDatas = await FileManager.Instance.Load<EventTriggerModel>(path);
            if (loadDatas == null)
            {
                loadDatas = new List<EventTriggerModel>();
            }
            CacheDataManager.Instance.InitMaxIndex(loadDatas);

            _contentView.SaveDataBind(loadDatas);
        }

        private void NotifyHelper_ConfigChanged(ConfigEventArgs e)
        {
            ApplicationManager.ShowProgressbar();
            _config = e.Config;
            Refresh();
            LoadDatas(GetSaveFilePath());
            ApplicationManager.HideProgressbar();
        }
        private void VersionCheck()
        {
            if (_config.VersionCheck == false)
            {
                return;
            }
            var latestNote = ApplicationManager.Instance.GetLatestVersion();
            if(latestNote == null)
            {
                return;
            }
            if (latestNote.Version > VersionNote.CurrentVersion)
            {
                if (ApplicationManager.ShowMessageDialog("Infomation",
                                                        $"{DocumentHelper.Get(Message.NewVersion)}{Environment.NewLine}{Environment.NewLine}" +
                                                        $"{DocumentHelper.Get(Message.PatchContent, latestNote.Desc)}",
                                                        MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative) == MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
                {
                    Process.Start(ConstHelper.ReleaseUrl);

                    //if (File.Exists("Patcher.exe"))
                    //{
                    //    Process.Start("Patcher.exe", $"{VersionNote.CurrentVersion.ToVersionString()} {latestNote.Version.ToVersionString()}");
                    //    Application.Current.Shutdown();
                    //}
                    //else
                    //{
                    //    Process.Start(ConstHelper.ReleaseUrl);
                    //}
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.Equals(btnRefresh))
            {
                Refresh();
            }
            else if (btn.Equals(btnStart))
            {
                ApplicationManager.ShowProgressbar();

                var buttons = this.FindChildren<Button>();
                foreach (var button in buttons)
                {
                    if (button.Equals(btnStart) || button.Equals(btnStop))
                    {
                        continue;
                    }
                        
                    button.IsEnabled = false;
                }
                btnStop.Visibility = Visibility.Visible;
                btnStart.Visibility = Visibility.Collapsed;
                tab_content.IsEnabled = false;
                Clear();

                _contentController.Start();

                ApplicationManager.HideProgressbar();
            }
            else if (btn.Equals(btnStop))
            {
                ApplicationManager.ShowProgressbar();
                _contentController.Stop();
                tab_content.IsEnabled = true;
                Dispatcher.Invoke(() =>
                {
                    var buttons = this.FindChildren<Button>();
                    foreach (var button in buttons)
                    {
                        if (button.Equals(btnStart) || button.Equals(btnStop))
                        {
                            continue;
                        }
                        button.IsEnabled = true;
                    }
                    btnStart.Visibility = Visibility.Visible;
                    btnStop.Visibility = Visibility.Collapsed;
                });
                ApplicationManager.HideProgressbar();
            }
            else if (btn.Equals(btnSetting))
            {
                UIManager.Instance.AddPopup<SettingView>();
            }
            else if (btn.Equals(btnGithub))
            {
                Process.Start(ConstHelper.HelpUrl);
            }
        }
    }
}
