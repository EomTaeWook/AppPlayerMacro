using KosherUtils.Framework;
using KosherUtils.Log;
using KosherUtils.Log.Config;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Patcher.Extensions;
using Patcher.Infrastructure;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Utils;
using Utils.Document;
using ConstHelper = Patcher.Infrastructure.ConstHelper;
using Version = Utils.Models.Version;

namespace Patcher
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        private FileManager _fileManager = new FileManager();
        public App()
        {
            LogBuilder.AddLogConfig(new FileLogConfig()
            {
                ArchiveFileName = @"./archive/log.{#}.txt",
                ArchiveRollingType = FileRollingType.Day,
                AutoFlush = true,
                FileName = @"./logs/LogFile.txt",
                KeepOpenFile = true,
                LogFormat = @"${date} | ${level} | ${message}",
                MaxArchiveFile = 7,
                MinLogLevel = LogLevel.Info
            });
            LogBuilder.AddLogConfig(new ConsoleLogConfig()
            {
                LogFormat = @"${date} | ${level} | ${message}",
                MinLogLevel = LogLevel.Info
            });

            LogBuilder.Build();
        }
        
        private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            if (Current == null)
            {
                return;
            }
            LogHelper.Fatal(e.Exception);

            var result = (Current.MainWindow as MetroWindow).ShowMessageDialog("", $"{e.Exception.Message}", MessageDialogStyle.Affirmative);

            if(result == MessageDialogResult.Affirmative)
            {
                Current.Shutdown();
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (Current == null)
            {
                return;

            }
            var ex = e.ExceptionObject as Exception;
            if(ex != null)
            {
                LogHelper.Fatal(ex);

                var result = (Current.MainWindow as MetroWindow).ShowMessageDialog("", $"{ex.Message}", MessageDialogStyle.Affirmative);

                if (result == MessageDialogResult.Affirmative)
                {
                    Current.Shutdown();
                }
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (Current == null)
            {
                return;
            }

            LogHelper.Fatal(e.Exception);

            var result = (Current.MainWindow as MetroWindow).ShowMessageDialog("", $"{e.Exception.Message}", MessageDialogStyle.Affirmative);

            if (result == MessageDialogResult.Affirmative)
            {
                Current.Shutdown();
            }
        }
        
        private void AllFileBackup()
        {
            _fileManager.Move(ConstHelper.TempBackupPath, new DirectoryInfo(Environment.CurrentDirectory), true);
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            var processes = Process.GetProcessesByName("Macro");
            foreach (var process in processes)
            {
                process.Kill();
            }
            AllFileBackup();
#if !DEBUG
            if(VersionValidate(e.Args) == false)
            {
                Current.Shutdown();
            }
            var currentVersion = Version.MakeVersion(e.Args[0]);
            var patchVersion = Version.MakeVersion(e.Args[1]);
#else
            var currentVersion = Version.MakeVersion("2.4.0");
            var patchVersion = Version.MakeVersion("9.6.0");
#endif
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;

            if (currentVersion >= patchVersion)
            {
                Current.Shutdown();
            }

            ServiceProviderManager.AddService("CurrentVersion", currentVersion);
            ServiceProviderManager.AddService("PatchVersion", patchVersion);
            DependenciesResolved();
            InitTemplate();
            
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }

        private bool VersionValidate(string [] args)
        {
            if (args.Count() != 2)
            {
                return false;
            }
            var currentVersion = args[0].Split('.');

            var nextVersion = args[1].Split('.');

            if (currentVersion.Count() != nextVersion.Count() || currentVersion.Count() != 3 || nextVersion.Count() != 3)
            {
                return false;
            }
               
            return true;
        }

        private void DependenciesResolved()
        {
            var path = Environment.CurrentDirectory + $@"\{Utils.ConstHelper.DefaultConfigFile}";

            if (File.Exists(path))
            {
                var config = JsonHelper.DeserializeObject<dynamic>(File.ReadAllText(path));

                if (Enum.TryParse(config["Language"].ToString(), true, result: out Language @enum))
                {
                    ServiceProviderManager.AddRefService("Language", @enum);
                }
            }
            else
            {
                ServiceProviderManager.AddRefService("Language", Language.Kor);
            }

            ServiceProviderManager.AddService("PatchUrl", ConstHelper.PatchV3Url);
            ServiceProviderManager.AddService("Label", Singleton<DocumentTemplate<Label>>.Instance);
            ServiceProviderManager.AddService("Message", Singleton<DocumentTemplate<Message>>.Instance);

        }
        private void InitTemplate()
        {
            var path = Utils.ConstHelper.DefaultDatasFilePath;
#if DEBUG
            path = $@"..\..\..\..\Datas\";
#endif
            ServiceProviderManager.GetService<DocumentTemplate<Label>>("Label").Init(path);
            ServiceProviderManager.GetService<DocumentTemplate<Message>>("Message").Init(path);
            
        }
    }
}
