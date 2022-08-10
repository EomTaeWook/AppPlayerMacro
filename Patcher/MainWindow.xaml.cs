using KosherUtils.Coroutine;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Patcher.Extensions;
using Patcher.Infrastructure;
using Patcher.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Utils;
using Utils.Document;
using Version = Utils.Infrastructure.Version;

namespace Patcher
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private static HttpClient httpClient = new HttpClient();

        private bool _isRunning = false;

        private CoroutineWoker _coroutineWoker = new CoroutineWoker();

        private SortedList<int, PatchInfoModel> _patchDatas = new SortedList<int, PatchInfoModel>();

        private DocumentTemplate<Label> _labelTemplate;

        private DocumentTemplate<Message> _messageTemplate;

        private Language _language;

        public MainWindow()
        {
            _labelTemplate = ServiceProviderManager.GetService<DocumentTemplate<Label>>("Label");
            _messageTemplate = ServiceProviderManager.GetService<DocumentTemplate<Message>>("Message");
            _language = ServiceProviderManager.GetRefService<Language>("Language");

            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitEvent();
            Init();
            Topmost = true;

            _ = RequestPatchListAsync();


            //if (ObjectCache.GetValue("Version").ToString().Equals("1"))
            //{

            //    Task.Run(async ()=> 
            //    {
            //        await RunPatch();
            //    });
            //}
            //else
            //{
            //    Application.Current.Shutdown();
            //}
        }

        private async Task RequestPatchListAsync()
        {
            lblState.Content = _labelTemplate.Get(Label.SearchPatchList, _language);

            var currentVersion = ServiceProviderManager.GetService<Version>("CurrentVersion");

            var response = await httpClient.GetAsync(ServiceProviderManager.GetService<string>("PatchUrl"));

            using (var reader = new StreamReader(await response.Content.ReadAsStreamAsync()))
            {
                var json = reader.ReadToEnd();
                var patchModel = JsonHelper.DeserializeObject<PatchListModel>(json);
                var newVersion = Version.MakeVersion(patchModel.CurrentVersion.Version);

                foreach (var item in patchModel.OldVersion)
                {
                    var version = item.GetVersion();
                    if (version < currentVersion)
                    {
                        continue;
                    }
                    _patchDatas.Add(version.GetVersionNumber(), item);
                }
                _patchDatas.Add(newVersion.GetVersionNumber(), patchModel.CurrentVersion);
            }
            _coroutineWoker.Start(DownLoadFiles());

        }
        private IEnumerator DownLoadFiles()
        {
            foreach(var patchData in _patchDatas)
            {
                var fileList = patchData.Value.GetFileList();
                var index = 0;
                foreach(var kv in fileList)
                {
                    Dispatcher.Invoke(() =>
                    {
                        lblState.Content = $"{_labelTemplate.Get(Label.Download, _language)} : {kv.Key}";
                        lblCount.Content = $"({index++}/{fileList.Count})";
                        progress.Value = 0;
                    });

                    var response =  httpClient.GetAsync(kv.Value).GetAwaiter().GetResult();

                    var totalSize = response.Content.Headers.ContentLength == null ? 0L : (long)response.Content.Headers.ContentLength;

                    var buffer = new byte[4096];

                    using (var stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                    {
                        var fileInfo = new FileInfo($"{Infrastructure.ConstHelper.TempPath}{kv.Key}");
                        fileInfo.Directory.Create();
                        using (var fileStream = fileInfo.Open(FileMode.Create))
                        {
                            var read = 0;
                            var current = 0L;
                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                fileStream.Write(buffer, 0, read);
                                current += read;
                                yield return null;
                                Dispatcher.Invoke(() =>
                                {
                                    progress.Value = 100.0 / (totalSize / current);
                                });
                            }
                            fileStream.Flush();
                            fileStream.Close();
                        }
                    }
                }
                yield return null;
            }
        }
 
        private async Task UpdateTime(long dateTimeTicks)
        {
            var currentTime = DateTime.Now.Ticks;
            _coroutineWoker.WorksUpdate((float)TimeSpan.FromTicks(DateTime.Now.Ticks - dateTimeTicks).TotalSeconds);
            await Task.Delay(10);
            await UpdateTime(currentTime);
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            if (this.ShowMessageDialog("", _messageTemplate.Get(Message.CancelPatch, _language), MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings()
                    {
                        DialogTitleFontSize = 0.1F,
                        MaximumBodyHeight = 500,
                    }) == MessageDialogResult.Affirmative)
            {
                btnCancel.IsEnabled = false;
            }
        }
        private void Init()
        {
            btnCancel.Content = _labelTemplate.Get(Label.Cancel, _language);

            _ = UpdateTime(DateTime.Now.Ticks);
        }
        private void InitEvent()
        {
            btnCancel.Click += Button_Click;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(sender.Equals(btnCancel))
            {
                if (this.ShowMessageDialog("", _messageTemplate.Get(Message.CancelPatch, _language), MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings()
                    {
                        DialogTitleFontSize = 0.1F,
                        MaximumBodyHeight = 500,
                    }) == MessageDialogResult.Affirmative)
                {
                    btnCancel.IsEnabled = false;
                    Rollback();
                    Application.Current.Shutdown();
                }
            }
        }
        
        private async Task RunPatch()
        {
            Backup();
            //await DownloadFiles(_cts.Token);
            //await Patching(_cts.Token);
            await Dispatcher.InvokeAsync(() =>
            {
                if (this.ShowMessageDialog("", _messageTemplate.Get(Message.CompletePatch, _language), MessageDialogStyle.Affirmative,
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
        
        
        private void Backup()
        {
            //for(int i=0; i<_patchList.Count; ++i)
            //{
            //    try
            //    {
            //        if (_patchList[i].Item1.Contains(@"\"))
            //        {
            //            var index = _patchList[i].Item1.LastIndexOf(@"\");
            //            Directory.CreateDirectory($@"{ConstHelper.TempBackupPath}{_patchList[i].Item1.Substring(0, index)}");
            //        }
            //        if (File.Exists(_patchList[i].Item1))
            //        {
            //            if(File.Exists($"{ConstHelper.TempBackupPath}{_patchList[i].Item1}"))
            //            {
            //                File.Delete($"{ConstHelper.TempBackupPath}{_patchList[i].Item1}");
            //            }
            //            File.Move(_patchList[i].Item1, $"{ConstHelper.TempBackupPath}{_patchList[i].Item1}");
            //        }
            //    }
            //    catch(Exception ex)
            //    {
            //        LogHelper.Error(ex);
            //    }
            //}
        }
        private async Task Patching(CancellationToken token)
        {
            //var copy = new List<Tuple<string, string>>();

            //for(int i=0; i< _patchList.Count; ++i)
            //{
            //    if (_patchList[i].Item1.Equals(AppDomain.CurrentDomain.FriendlyName))
            //    {
            //        ObjectCache.SetValue("Patcher", _patchList[i].Item1);
            //        continue;
            //    }
            //    copy.Add(_patchList[i]);
            //}

            //for (int i=0; i< copy.Count; ++i)
            //{
            //    try
            //    {
            //        if (token.IsCancellationRequested)
            //        {
            //            Rollback();
            //            return;
            //        }
            //        await Dispatcher.InvokeAsync(() =>
            //        {
            //            lblState.Content = $"{ObjectCache.GetValue("Patching")} : {copy[i].Item1}";
            //            lblCount.Content = $"({i + 1}/{copy.Count})";
            //            progress.Value = 0;
            //        });

            //        if (_patchList[i].Item1.Contains(@"\"))
            //        {
            //            var index = copy[i].Item1.LastIndexOf(@"\");
            //            Directory.CreateDirectory($"{copy[i].Item1.Substring(0, index)}");
            //        }
            //        if (File.Exists(copy[i].Item1))
            //        {
            //            File.Delete(copy[i].Item1);
            //        }
            //        if (File.Exists($@"{ConstHelper.TempPath}\{copy[i].Item1}"))
            //        {
            //            var totalSize = new FileInfo($@"{ConstHelper.TempPath}\{copy[i].Item1}").Length;
            //            byte[] buffer = new byte[4096];
            //            using (var inStream = new FileStream($@"{ConstHelper.TempPath}\{copy[i].Item1}", FileMode.Open))
            //            {
            //                using (var outStream = new FileStream(copy[i].Item1, FileMode.OpenOrCreate))
            //                {
            //                    var read = 0;
            //                    var current = 0;
            //                    while((read = inStream.Read(buffer, 0, buffer.Length)) > 0)
            //                    {
            //                        outStream.Write(buffer, 0, read);

            //                        current += read;
            //                        await Task.Delay(10);
            //                        await Dispatcher.InvokeAsync(() =>
            //                        {
            //                            progress.Value = 100 / (totalSize / current);
            //                        }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            //                    }
            //                    outStream.Flush();
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        LogHelper.Error(ex);
            //        MessageBox.Show(ObjectCache.GetValue("FailedPatchUpdate").ToString(), ObjectCache.GetValue("FailedPatch").ToString());
            //    }
            //}
        }
        private void Rollback()
        {
            //for (int i = 0; i < _patchList.Count; ++i)
            //{
            //    try
            //    {
            //        Dispatcher.Invoke(() =>
            //        {
            //            lblState.Content = $"{ObjectCache.GetValue("Rollback")} : {_patchList[i].Item1}";
            //            lblCount.Content = $"({i + 1}/{_patchList.Count})";
            //            progress.Value = 0;
            //        });
            //        if (_patchList[i].Item1.Contains(@"\"))
            //        {
            //            var index = _patchList[i].Item1.LastIndexOf(@"\");
            //            Directory.CreateDirectory($"{_patchList[i].Item1.Substring(0, index)}");
            //        }
            //        if (File.Exists(_patchList[i].Item1))
            //        {
            //            File.Delete(_patchList[i].Item1);
            //        }

            //        if (File.Exists($@"{ConstHelper.TempBackupPath}{_patchList[i].Item1}"))
            //        {
            //            File.Move($"{ConstHelper.TempBackupPath}{_patchList[i].Item1}", _patchList[i].Item1);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        LogHelper.Error(ex);
            //    }
            //}
        } 
    }
}
