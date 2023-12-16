using Dignus.Coroutine;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Patcher.Extensions;
using Patcher.Infrastructure;
using Patcher.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Utils;
using Utils.Document;
using ConstHelper = Patcher.Infrastructure.ConstHelper;
using Version = Utils.Models.Version;

namespace Patcher
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private static HttpClientHandler handler = new HttpClientHandler
        {
            UseCookies = false,
        };
        private static HttpClient httpClient = new HttpClient(handler);

        private CoroutineHandler _coroutineHandler = new CoroutineHandler();

        private SortedList<int, PatchInfoModel> _patchDatas = new SortedList<int, PatchInfoModel>();

        private DocumentTemplate<Label> _labelTemplate;

        private DocumentTemplate<Message> _messageTemplate;

        private Language _language;

        private FileManager _fileManager = new FileManager();

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
        }
        private void ProcessDownloadFiles()
        {
            _coroutineHandler.Start(DownloadFiles(), RestoreNotPatchFiles);
        }
        private void RestoreNotPatchFiles()
        {
            _fileManager.Move(Environment.CurrentDirectory,
                            new DirectoryInfo(ConstHelper.TempBackupPath));

            ProcessRunMacro();
        }
        private void ProcessRunMacro()
        {
            var directoryInfo = new DirectoryInfo(ConstHelper.TempBackupPath);
            directoryInfo.Delete(true);

#if !DEBUG
            Process.Start("Macro");
#else
            Process.Start(@"..\..\..\..\Release\exe\Macro.exe");
#endif

            Application.Current.Shutdown();
        }

        private async Task RequestPatchListAsync()
        {
            lblState.Content = _labelTemplate.Get(Label.SearchPatchList, _language);

            var currentVersion = ServiceProviderManager.GetService<Version>("CurrentVersion");

#if !DEBUG
            var response = await httpClient.GetAsync(ServiceProviderManager.GetService<string>("PatchUrl"));

            var readStream = await response.Content.ReadAsStreamAsync();
#else
            var readStream = new FileStream(@"..\..\..\..\Datas\PatchListV3.json", FileMode.Open);
#endif
            using (var reader = new StreamReader(readStream))
            {
                var json = await reader.ReadToEndAsync();
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
            ProcessDownloadFiles();
        }

        private IEnumerator PatchingFiles(Dictionary<string, string> patchFileList)
        {
            var buffer = new byte[4096];
            var index = 0;
            foreach (var kv in patchFileList)
            {
                var fileInfo = new FileInfo(kv.Key);
                if (fileInfo.Name.Equals(AppDomain.CurrentDomain.FriendlyName) == true)
                {
                    continue;
                }
                Dispatcher.Invoke(() =>
                {
                    lblState.Content = $"{_labelTemplate.Get(Label.Patching, _language)} : {kv.Key}";
                    lblCount.Content = $"({++index}/{patchFileList.Keys.Count})";
                    progress.Value = 0;
                });

                fileInfo.Directory.Create();
                fileInfo.Delete();
                using (var rs = new FileStream($"{ConstHelper.TempPath}{kv.Key}", FileMode.OpenOrCreate))
                {
                    var totalSize = rs.Length;

                    using (var ws = fileInfo.OpenWrite())
                    {
                        var read = 0;
                        var current = 0;
                        while ((read = rs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ws.Write(buffer, 0, read);
                            current += read;
                            Dispatcher.Invoke(() =>
                            {
                                progress.Value = 100 / (totalSize / current);
                            });
                            ws.Flush();
                            yield return null;
                        }
                        ws.Close();
                    }
                }
                yield return null;
            }
        }
        private IEnumerator DownloadFiles()
        {
            var buffer = new byte[4096];
            var totalCount = _patchDatas.Values.Sum(r => r.GetFileList().Count);
            var index = 0;
            foreach (var patchData in _patchDatas)
            {
                var fileList = patchData.Value.GetFileList();

                foreach (var kv in fileList)
                {
                    Dispatcher.Invoke(() =>
                    {
                        lblState.Content = $"{_labelTemplate.Get(Label.Download, _language)} : {kv.Key}";
                        lblCount.Content = $"({++index}/{totalCount})";
                        progress.Value = 0;
                    });

#if !DEBUG
                    var response = httpClient.GetAsync(kv.Value).GetAwaiter().GetResult();

                    var totalSize = response.Content.Headers.ContentLength == null ? 0L : (long)response.Content.Headers.ContentLength;

                    using (var stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                    {
                        var fileInfo = new FileInfo($"{ConstHelper.TempPath}{kv.Key}");
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
#endif
                }

                yield return PatchingFiles(fileList);

                yield return null;
            }
        }

        private async Task UpdateTime(long dateTimeTicks)
        {
            var currentTime = DateTime.Now.Ticks;
            _coroutineHandler.UpdateCoroutines((float)TimeSpan.FromTicks(DateTime.Now.Ticks - dateTimeTicks).TotalSeconds);
            await Task.Delay(50);
            _ = UpdateTime(currentTime);
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            if (this.ShowMessageDialog("", _messageTemplate.Get(Message.CancelPatch, _language), MessageDialogStyle.AffirmativeAndNegative)
                == MessageDialogResult.Affirmative)
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
            if (sender.Equals(btnCancel))
            {
                if (this.ShowMessageDialog("", _messageTemplate.Get(Message.CancelPatch, _language), MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                {
                    btnCancel.IsEnabled = false;

                    this._coroutineHandler.StopAll();

                    this._coroutineHandler.Start(Rollback(), () =>
                    {
                        Application.Current.Shutdown();
                    });
                }
            }
        }
        private IEnumerator Rollback()
        {
            var buffer = new byte[4096];
            foreach (var patchData in _patchDatas)
            {
                foreach (var item in patchData.Value.GetFileList())
                {
                    Dispatcher.Invoke(() =>
                    {
                        lblState.Content = $"{_labelTemplate.Get(Label.Rollback, _language)} : {item.Key}";
                    });

                    var rollbackFileInfo = new FileInfo($"{ConstHelper.TempBackupPath}{item.Key}");

                    if (rollbackFileInfo.Exists == false)
                    {
                        continue;
                    }

                    var fileInfo = new FileInfo($"{item.Key}");

                    if (fileInfo.Exists == true)
                    {
                        fileInfo.Delete();
                    }

                    using (var rs = rollbackFileInfo.OpenRead())
                    {
                        var read = 0;
                        var totalSize = rollbackFileInfo.Length;
                        var current = 0;
                        while ((read = rs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            using (var ws = fileInfo.OpenWrite())
                            {
                                ws.Write(buffer, 0, read);
                                current += read;

                                Dispatcher.Invoke(() =>
                                {
                                    progress.Value = 100.0 / (totalSize / current);
                                });
                                yield return null;
                            }
                        }
                    }

                    rollbackFileInfo.Delete();
                }
                yield return null;
            }
        }
    }
}
