using Macro.Extensions;
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
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                ExceptionProcess(s, ex.ExceptionObject as Exception);
            };
            AppDomain.CurrentDomain.FirstChanceException += (s, ex) =>
            {
#if DEBUG
                LogHelper.Debug(ex.Exception.Message);
                
#else
                ExceptionProcess(s, ex.Exception);
#endif

            };
            
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
            var path = $@"{Environment.CurrentDirectory}\Datas\";
            Singleton<DocumentTemplate<Label>>.Instance.Init(path);
            Singleton<DocumentTemplate<Message>>.Instance.Init(path);
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
