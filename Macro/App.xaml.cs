using KosherUtils.Framework;
using KosherUtils.Log;
using Macro.Infrastructure;
using Macro.Infrastructure.Interface;
using Macro.Infrastructure.Manager;
using Macro.Models;
using Macro.Models.ViewModel;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Utils;
using Utils.Document;

namespace Macro
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += (s, ex) =>
            {
                ex.Handled = true;
                LogHelper.Fatal(ex.Exception);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                var exception = ex.ExceptionObject as Exception;
                LogHelper.Fatal(exception);
            };
            AppDomain.CurrentDomain.FirstChanceException += (s, ex) =>
            {
#if DEBUG
                LogHelper.Debug(ex.Exception.Message, 0, ex.Exception.TargetSite.DeclaringType.FullName);
#else
                LogHelper.Warning(ex.Exception);
#endif
            };

            var exeList = e.Args.Where(r => Path.GetExtension(r).Equals(".exe")).ToArray();
            if(exeList.Count() > 0)
            {
                for (int i = 0; i < exeList.Length; ++i)
                {
                    TempFolderFileMove(exeList[i]);
                }
            }
            else
            {
                TempFolderFileMove(ConstHelper.DefaultPatcherName);
            }
            Init();
            base.OnStartup(e);
        }
        private void Init()
        {
            DependenciesResolved();
            InitTemplate();

            ShutdownMode = ShutdownMode.OnLastWindowClose;
        }
        private void TempFolderFileMove(string fileName)
        {
            var processes = Process.GetProcesses().Where(r => r.ProcessName.Equals(Path.GetFileNameWithoutExtension(fileName))).ToArray();
            foreach (var process in processes)
            {
                process.Kill();
            }
            if (File.Exists(fileName) && File.Exists($@"{Path.GetTempPath()}Macro\{fileName}"))
            {
                File.Delete(fileName);
                File.Move($@"{Path.GetTempPath()}Macro\{fileName}", fileName);
            }
            else if (File.Exists($@"{Path.GetTempPath()}Macro\{fileName}"))
            {
                File.Move($@"{Path.GetTempPath()}Macro\{fileName}", fileName);
            }
        }
        private void InitTemplate()
        {
            Singleton<DocumentTemplate<Label>>.Instance.Init(ConstHelper.DefaultDatasFilePath);
            Singleton<DocumentTemplate<Message>>.Instance.Init(ConstHelper.DefaultDatasFilePath);
        }
        private void DependenciesResolved()
        {
            var path = Environment.CurrentDirectory + $@"\{ConstHelper.DefaultConfigFile}";
            if (!File.Exists(path))
            {
                File.WriteAllText(path, JsonHelper.SerializeObject(new Config(), true));
            }
            var config = JsonHelper.Load<Config>(path);

            ServiceProviderManager.Instance.AddSingleton<Config>(config);

            var documentHelper = Singleton<DocumentHelper>.Instance;
            documentHelper.Init(config);
            ServiceProviderManager.Instance.AddSingleton<DocumentHelper>(documentHelper);

            var applicationDataHelper = Singleton<ApplicationDataHelper>.Instance;
            applicationDataHelper.Init("ApplicationData");
            ServiceProviderManager.Instance.AddSingleton<ApplicationDataHelper>(applicationDataHelper);

            ServiceProviderManager.Instance.AddSingleton<IKeyboardInput, KeyboardInput>();
            ServiceProviderManager.Instance.AddSingleton<IMouseInput, MouseInput>();
            ServiceProviderManager.Instance.AddSingleton<InputManager, InputManager>();
            ServiceProviderManager.Instance.AddSingleton<CacheDataManager, CacheDataManager>();

            ServiceProviderManager.Instance.AddSingleton<EventConfigViewModel, EventConfigViewModel>(); 
            ServiceProviderManager.Instance.AddSingleton<LabelViewModel, LabelViewModel>();
            ServiceProviderManager.Instance.AddSingleton<SettingViewModel, SettingViewModel>();
            ServiceProviderManager.Instance.AddSingleton<ViewModelLocator, ViewModelLocator>();
        }
    }
}