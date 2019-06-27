using Macro.Infrastructure;
using Macro.Infrastructure.Manager;
using Macro.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Unity;
using Utils;
using Utils.Document;

namespace Macro
{
    public partial class App : Application
    {
        private void Init()
        {
            LogHelper.Init();
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
            Singleton<DocumentTemplate<Label>>.Instance.Init(ConstHelper.DefaultDatasFile);
            Singleton<DocumentTemplate<Message>>.Instance.Init(ConstHelper.DefaultDatasFile);

            Singleton<ApplicationDataManager>.Instance.Init("ApplicationData");
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

            container.RegisterSingleton<CacheDataManager>();
        }
    }

}
