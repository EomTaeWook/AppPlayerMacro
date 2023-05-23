using Kosher.DependencyInjection;
using Kosher.Extensions.Log;
using Kosher.Framework;
using Kosher.Log;
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
        public App()
        {
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            LogBuilder.Configuration(LogConfigXmlReader.Load("KosherLog.config"));
            LogBuilder.Build();

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
                LogHelper.Debug(ex.Exception.Message);
#else
                LogHelper.Error(ex.Exception);
#endif
            };

            MovePatcherFile();
            Init();
            base.OnStartup(e);
        }
        private void Init()
        {
            DependenciesResolved();
            InitTemplate();

            ShutdownMode = ShutdownMode.OnLastWindowClose;
        }
        private void MovePatcherFile()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(ConstHelper.TempPath);
            if(directoryInfo.Exists == false)
            {
                return;
            }
            foreach (var item in directoryInfo.GetFiles())
            {
                if (item.Extension.ToLower().Equals(".exe") == true)
                {
                    var name = Path.GetFileNameWithoutExtension(item.Name);
                    var processes = Process.GetProcesses().Where(r => r.ProcessName.Equals(name));
                    foreach (var process in processes)
                    {
                        process.Kill();
                    }
                    File.Move(item.FullName, item.Name);
                }
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
            ServiceContainer serviceContainer = new ServiceContainer();
            serviceContainer.RegisterType(config);

            var documentHelper = Singleton<DocumentHelper>.Instance;
            documentHelper.Init(config);
            serviceContainer.RegisterType(documentHelper);

            var applicationDataHelper = Singleton<ApplicationDataHelper>.Instance;
            applicationDataHelper.Init("ApplicationData");
            serviceContainer.RegisterType(applicationDataHelper);
            serviceContainer.RegisterType<FileService, FileService>();

            serviceContainer.RegisterType<IKeyboardInput, KeyboardInput>();
            serviceContainer.RegisterType<IMouseInput, MouseInput>();
            serviceContainer.RegisterType<InputManager, InputManager>();
            serviceContainer.RegisterType<CacheDataManager, CacheDataManager>();

            serviceContainer.RegisterType<EventSettingViewModel, EventSettingViewModel>();
            serviceContainer.RegisterType<LabelViewModel, LabelViewModel>();
            serviceContainer.RegisterType<SettingViewModel, SettingViewModel>();
            serviceContainer.RegisterType<ViewModelLocator, ViewModelLocator>();

            ServiceDispatcher.SetContainer(serviceContainer);


        }
    }
}