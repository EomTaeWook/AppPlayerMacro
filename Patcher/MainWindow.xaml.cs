using KosherUtils.Log;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Patcher.Extensions;
using Patcher.Infrastructure;
using Patcher.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Utils;
using ConstHelper = Patcher.Infrastructure.ConstHelper;

namespace Patcher
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly List<Tuple<string, string>> _patchList;
        private readonly CancellationTokenSource _cts;
        public MainWindow()
        {
            _patchList = new List<Tuple<string, string>>();
            _cts = new CancellationTokenSource();

            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitEvent();
            Init();
            Topmost = true;
            if (ObjectCache.GetValue("Version").ToString().Equals("1"))
                CheckPatchList();

            Task.Run(async () =>
            {
                await RunPatch(_cts.Token);
                Dispatcher.Invoke(() =>
                {
                    Application.Current.Shutdown();
                });
            });
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            if (this.MessageShow("", ObjectCache.GetValue("CancelPatch").ToString(), MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings()
                    {
                        DialogTitleFontSize = 0.1F,
                        MaximumBodyHeight = 500,
                    }) == MessageDialogResult.Affirmative)
            {
                btnCancel.IsEnabled = false;
                _cts.Cancel();
            }
        }
        private void Init()
        {
            btnCancel.Content = ObjectCache.GetValue("Cancel");
        }
        private void InitEvent()
        {
            btnCancel.Click += Button_Click;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(sender.Equals(btnCancel))
            {
                if (this.MessageShow("", ObjectCache.GetValue("CancelPatch").ToString(), MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings()
                    {
                        DialogTitleFontSize = 0.1F,
                        MaximumBodyHeight = 500,
                    }) == MessageDialogResult.Affirmative)
                {
                    btnCancel.IsEnabled = false;
                    _cts.Cancel();
                    Rollback().Wait();
                    Application.Current.Shutdown();
                }
            }
        }
        
        private Task RunPatch(CancellationToken token)
        {
            var tcs = new TaskCompletionSource<Task>();
            Dispatcher.InvokeAsync(() =>
            {
                Backup().ContinueWith(task =>
                {
                    return DownloadFiles(token);
                }).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                        return Patching(token);
                    return task;
                }).ContinueWith(async task =>
                {
                    if (task.Result.Status != TaskStatus.RanToCompletion)
                        await Rollback();
                    tcs.SetResult(task.Result);
                });
            }, System.Windows.Threading.DispatcherPriority.Input);

            return tcs.Task;
        }
        private void CheckPatchList()
        {
            try
            {
                lblState.Content = ObjectCache.GetValue("SearchPatchList");
                var request = (HttpWebRequest)WebRequest.Create(ObjectCache.GetValue("PatchUrl").ToString());
                using (var response = request.GetResponse())
                {
                    using (var stream = new StreamReader(response.GetResponseStream()))
                    {
                        var json = stream.ReadToEnd();
                        var patchModels = JsonHelper.DeserializeObject<Dictionary<string, PatchInfoModel>>(json);
                        var version = ObjectCache.GetValue("PathchVersion") as Infrastructure.Version;
                        if (patchModels.ContainsKey(version.ToVersionString()))
                        {
                            var patchModel = patchModels[version.ToVersionString()];
                            ObjectCache.SetValue("PatchMethod", patchModel.Method);
                            if (patchModel.Method == PatchMethod.Exe)
                            {
                                _patchList.AddRange(patchModel.List.Where(r => r.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Count() == 2)
                                .Select(r =>
                                {
                                    var split = r.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    return new Tuple<string, string>(split[0], split[1]);
                                }));
                            }
                            else
                            {
                                Process.Start(Utils.ConstHelper.ReleaseUrl);
                                Application.Current.Shutdown();
                            }
                        }
                    }
                }
                lblCount.Content = $"(0/{_patchList.Count})";
            }
            catch(Exception ex)
            {
                Log.Warning(ex);
            }
        }
        private Task DownloadFiles(CancellationToken token)
        {
            for (int i = 0; i < _patchList.Count; ++i)
            {
                try
                {
                    if (token.IsCancellationRequested)
                        return Task.FromCanceled(token);
                    Dispatcher.Invoke(() =>
                    {
                        lblState.Content = $"{ObjectCache.GetValue("Download")} : {_patchList[i].Item1}" ;
                        lblCount.Content = $"({i + 1}/{_patchList.Count})";
                        progress.Value = 0;
                    });

                    var request = (HttpWebRequest)WebRequest.Create(_patchList[i].Item2);
                    request.ContentType = "application/octet-stream";
                    request.ContentLength = 0;
                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        var total = response.ContentLength;
                        using (var stream = response.GetResponseStream())
                        {
                            if (_patchList[i].Item1.Contains(@"\"))
                            {
                                var index = _patchList[i].Item1.LastIndexOf(@"\");
                                Directory.CreateDirectory($@"{ConstHelper.TempPath}\{_patchList[i].Item1.Substring(0, index)}");
                            }
                            using (var fs = new FileStream($@"{ConstHelper.TempPath}\{_patchList[i].Item1}", FileMode.Create, FileAccess.Write))
                            {
                                var buffer = new byte[4096];
                                var read = 0;
                                var current = 0L;
                                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        fs.Write(buffer, 0, read);
                                        current += read;
                                        progress.Value = 100 - total * 1.0 / current;
                                    });
                                }
                                fs.Flush();
                                Dispatcher.Invoke(() =>
                                {
                                    progress.Value = 100D;
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex);
                    i--;
                }
            }
            return Task.CompletedTask;
        }
        private Task Backup()
        {
            for(int i=0; i<_patchList.Count; ++i)
            {
                try
                {
                    if (_patchList[i].Item1.Contains(@"\"))
                    {
                        var index = _patchList[i].Item1.LastIndexOf(@"\");
                        Directory.CreateDirectory($@"{ConstHelper.TempBackupPath}{_patchList[i].Item1.Substring(0, index)}");
                    }
                    if (File.Exists(_patchList[i].Item1))
                    {
                        if(File.Exists($"{ConstHelper.TempBackupPath}{_patchList[i].Item1}"))
                        {
                            File.Delete($"{ConstHelper.TempBackupPath}{_patchList[i].Item1}");
                        }
                        File.Move(_patchList[i].Item1, $"{ConstHelper.TempBackupPath}{_patchList[i].Item1}");
                    }
                }
                catch(Exception ex)
                {
                    Log.Warning(ex);
                }
            }
            return Task.CompletedTask;
        }
        private Task Patching(CancellationToken token)
        {
            for(int i=0; i< _patchList.Count; ++i)
            {
                try
                {
                    if (token.IsCancellationRequested)
                        return Task.FromCanceled(token);

                    if (_patchList[i].Item1.Equals(AppDomain.CurrentDomain.FriendlyName))
                    {
                        ObjectCache.SetValue("Patcher", _patchList[i].Item1);
                        continue;
                    }
                    Dispatcher.Invoke(() =>
                    {
                        lblState.Content = $"{ObjectCache.GetValue("Patching")} : {_patchList[i].Item1}";
                        lblCount.Content = $"({i + 1}/{_patchList.Count})";
                        progress.Value = 0;
                    });

                    if (_patchList[i].Item1.Contains(@"\"))
                    {
                        var index = _patchList[i].Item1.LastIndexOf(@"\");
                        Directory.CreateDirectory($"{_patchList[i].Item1.Substring(0, index)}");
                    }
                    if (File.Exists(_patchList[i].Item1))
                        File.Delete(_patchList[i].Item1);

                    if (File.Exists($@"{ConstHelper.TempPath}\{_patchList[i].Item1}"))
                        File.Move($@"{ConstHelper.TempPath}\{_patchList[i].Item1}", _patchList[i].Item1);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex);
                    MessageBox.Show(ObjectCache.GetValue("FailedPatchUpdate").ToString(), ObjectCache.GetValue("FailedPatch").ToString());
                    return Task.FromException(ex);
                }
            }
            return Task.CompletedTask;
        }
        private Task Rollback()
        {
            for (int i = 0; i < _patchList.Count; ++i)
            {
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        lblState.Content = $"{ObjectCache.GetValue("Rollback")} : {_patchList[i].Item1}";
                        lblCount.Content = $"({i + 1}/{_patchList.Count})";
                        progress.Value = 0;
                    });
                    if (_patchList[i].Item1.Contains(@"\"))
                    {
                        var index = _patchList[i].Item1.LastIndexOf(@"\");
                        Directory.CreateDirectory($"{_patchList[i].Item1.Substring(0, index)}");
                    }
                    if (File.Exists(_patchList[i].Item1))
                        File.Delete(_patchList[i].Item1);

                    if (File.Exists($@"{ConstHelper.TempBackupPath}{_patchList[i].Item1}"))
                        File.Move($"{ConstHelper.TempBackupPath}{_patchList[i].Item1}", _patchList[i].Item1);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex);
                }
            }
            return Task.CompletedTask;
        } 
    }
}
