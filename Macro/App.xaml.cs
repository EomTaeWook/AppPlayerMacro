using Macro.Infrastructure;
using Macro.Infrastructure.Manager;
using Macro.Models;
using System;
using System.IO;
using System.Windows;
using Unity;
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
                ExceptionProcess(s, ex.Exception);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                ExceptionProcess(s, ex.ExceptionObject as Exception);
            };
            AppDomain.CurrentDomain.FirstChanceException += (s, ex) =>
            {
#if DEBUG
                LogHelper.Debug(ex.Exception.Message, 0, ex.Exception.TargetSite.DeclaringType.FullName);
#else
                LogHelper.Warning(ex.Exception.Message, 0, ex.Exception.TargetSite.DeclaringType.FullName);
#endif
            };
            for (int i = 0; i < e.Args.Length; ++i)
            {
                if(File.Exists(e.Args[i]))
                    File.Delete(e.Args[i]);

                File.Move($@"{Path.GetTempPath()}Macro\{e.Args[i]}", e.Args[i]);
            }
            Init();
            base.OnStartup(e);
        }
        private void Init()
        {
            DependenciesResolved();
            InitTemplate();
            LogHelper.Init();
            ShutdownMode = ShutdownMode.OnLastWindowClose;
        }
        private void InitTemplate()
        {
            Singleton<DocumentTemplate<Label>>.Instance.Init(ConstHelper.DefaultDatasFile);
            Singleton<DocumentTemplate<Message>>.Instance.Init(ConstHelper.DefaultDatasFile);

            Singleton<DynamicDPIManager>.Instance.Init("dynamicDPI");
        }
        private void DependenciesResolved()
        {
            var path = Environment.CurrentDirectory + $@"\{ConstHelper.DefaultConfigFile}";
            if (!File.Exists(path))
                File.WriteAllText(path, JsonHelper.SerializeObject(new Config(), true));

            var config = JsonHelper.Load<Config>(path);

            var container = Singleton<UnityContainer>.Instance;
            container.RegisterType<IMouseInput, MouseInput>();
            container.RegisterType<IKeyboardInput, KeyboardInput>();
            container.RegisterType<InputManager>();

            container.RegisterInstance<IConfig>(config);
            container.RegisterInstance(new DocumentHelper());

            container.RegisterSingleton<ProcessManager>();
        }
        private void ExceptionProcess(object sender, Exception ex)
        {
            //Debug.Assert(false, ex.Message);
            LogHelper.Warning(ex.Message);
        }
    }
}
