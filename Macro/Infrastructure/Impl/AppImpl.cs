using KosherUtils.Framework;
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
using Unity;
using Utils;
using Utils.Document;

namespace Macro
{
    public partial class App : Application
    {
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

            Singleton<ApplicationDataManager>.Instance.Init("ApplicationData");
        }
        private void DependenciesResolved()
        {
            var path = Environment.CurrentDirectory + $@"\{ConstHelper.DefaultConfigFile}";
            if (!File.Exists(path))
            {
                File.WriteAllText(path, JsonHelper.SerializeObject(new Config(), true));
            }

            var config = JsonHelper.Load<Config>(path);

            var container = Singleton<UnityContainer>.Instance;
            container.RegisterType<IMouseInput, MouseInput>();
            container.RegisterType<IKeyboardInput, KeyboardInput>();

            //ViewModel
            //container.RegisterSingleton<GameEventConfigViewModel>();
            container.RegisterSingleton<CommonEventConfigViewModel>();

            container.RegisterInstance(typeof(IConfig), config);
            container.RegisterInstance(new DocumentHelper());

            var inputManager = new InputManager();
            inputManager.SetKeyboardInput((KeyboardInput)container.Resolve(typeof(IKeyboardInput)));
            inputManager.SetMouseInput((MouseInput)container.Resolve(typeof(IMouseInput)));
            container.RegisterInstance<InputManager>(inputManager);
            container.RegisterSingleton<CacheDataManager>();
        }
    }

}
