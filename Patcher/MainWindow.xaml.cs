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
using System.Net.Http;
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

        private static HttpClient httpClient = new HttpClient();

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
            {
                Task.Run(async ()=> 
                {
                    await CheckPatchList();
                    await RunPatch();
                });
            }
            else
            {
                Application.Current.Shutdown();
            }
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
                    Rollback();
                    Application.Current.Shutdown();
                }
            }
        }
        
        private async Task RunPatch()
        {
            Backup();
            await DownloadFiles(_cts.Token);
            await Patching(_cts.Token);
            await Dispatcher.InvokeAsync(() =>
            {
                if (this.MessageShow("", ObjectCache.GetValue("CompletePatch").ToString(), MessageDialogStyle.Affirmative,
                    new MetroDialogSettings()
                    {
                        DialogTitleFontSize = 0.1F,
                        MaximumBodyHeight = 500,
                    }) == MessageDialogResult.Affirmative)
                {
                    Application.Current.Shutdown();
                }
            });
        }
        private async Task CheckPatchList()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    lblState.Content = ObjectCache.GetValue("SearchPatchList");
                });
                
                var response = await httpClient.GetAsync(ObjectCache.GetValue("PatchUrl").ToString());
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var json = reader.ReadToEnd();
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
                await Task.Delay(1);
                Dispatcher.Invoke(() =>
                {
                    lblCount.Content = $"(0/{_patchList.Count})";
                });
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex);
            }
            return;
        }
        private async Task DownloadFiles(CancellationToken token)
        {
            for (int i = 0; i < _patchList.Count; ++i)
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        Rollback();
                        return;
                    }

                    Dispatcher.Invoke(() =>
                    {
                        lblState.Content = $"{ObjectCache.GetValue("Download")} : {_patchList[i].Item1}";
                        lblCount.Content = $"({i + 1}/{_patchList.Count})";
                        progress.Value = 0;
                    });

                    var response = await httpClient.GetAsync(_patchList[i].Item2);

                    var totalSize = response.Content.Headers.ContentLength == null ? 0L : (long)response.Content.Headers.ContentLength;

                    using (var stream = await response.Content.ReadAsStreamAsync())
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
                                fs.Write(buffer, 0, read);
                                current += read;
                                await Task.Delay(10);
                                await Dispatcher.InvokeAsync(() =>
                                {
                                    progress.Value = 100 / (totalSize / current);
                                }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            }
                            fs.Flush();
                        }
                    }
                }
                catch (Exception ex)
                {
                    //재시도
                    LogHelper.Error(ex);
                    i--;
                }
            }
        }
        private void Backup()
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
                    LogHelper.Error(ex);
                }
            }
        }
        private async Task Patching(CancellationToken token)
        {
            var copy = new List<Tuple<string, string>>();

            for(int i=0; i< _patchList.Count; ++i)
            {
                if (_patchList[i].Item1.Equals(AppDomain.CurrentDomain.FriendlyName))
                {
                    ObjectCache.SetValue("Patcher", _patchList[i].Item1);
                    continue;
                }
                copy.Add(_patchList[i]);
            }

            for (int i=0; i< copy.Count; ++i)
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        Rollback();
                        return;
                    }
                    await Dispatcher.InvokeAsync(() =>
                    {
                        lblState.Content = $"{ObjectCache.GetValue("Patching")} : {copy[i].Item1}";
                        lblCount.Content = $"({i + 1}/{copy.Count})";
                        progress.Value = 0;
                    });

                    if (_patchList[i].Item1.Contains(@"\"))
                    {
                        var index = copy[i].Item1.LastIndexOf(@"\");
                        Directory.CreateDirectory($"{copy[i].Item1.Substring(0, index)}");
                    }
                    if (File.Exists(copy[i].Item1))
                    {
                        File.Delete(copy[i].Item1);
                    }
                    if (File.Exists($@"{ConstHelper.TempPath}\{copy[i].Item1}"))
                    {
                        var totalSize = new FileInfo($@"{ConstHelper.TempPath}\{copy[i].Item1}").Length;
                        byte[] buffer = new byte[4096];
                        using (var inStream = new FileStream($@"{ConstHelper.TempPath}\{copy[i].Item1}", FileMode.Open))
                        {
                            using (var outStream = new FileStream(copy[i].Item1, FileMode.OpenOrCreate))
                            {
                                var read = 0;
                                var current = 0;
                                while((read = inStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    outStream.Write(buffer, 0, read);

                                    current += read;
                                    await Task.Delay(10);
                                    await Dispatcher.InvokeAsync(() =>
                                    {
                                        progress.Value = 100 / (totalSize / current);
                                    }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                                }
                                outStream.Flush();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    MessageBox.Show(ObjectCache.GetValue("FailedPatchUpdate").ToString(), ObjectCache.GetValue("FailedPatch").ToString());
                }
            }
        }
        private void Rollback()
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
                    {
                        File.Delete(_patchList[i].Item1);
                    }
                        

                    if (File.Exists($@"{ConstHelper.TempBackupPath}{_patchList[i].Item1}"))
                    {
                        File.Move($"{ConstHelper.TempBackupPath}{_patchList[i].Item1}", _patchList[i].Item1);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }
        } 
    }
}
