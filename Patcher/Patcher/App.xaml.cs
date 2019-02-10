using Newtonsoft.Json.Linq;
using Patcher.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Patcher
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
#if DEBUG
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
#endif
        }
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblies = Assembly.GetExecutingAssembly();
            var name = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";
            var resources = assemblies.GetManifestResourceNames().Where(s => s.EndsWith(name));
            if (resources.Count() > 0)
            {
                var resourceName = resources.First();
                using (Stream stream = assemblies.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        var buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                        return Assembly.Load(buffer);
                    }
                }
            }
            return null;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Init();
            InitTemplate();
            base.OnStartup(e);
        }

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            if (args.LoadedAssembly.ManifestModule.Name.Equals("Utils.dll"))
            {
                Trace.WriteLine($">>>>{args.LoadedAssembly.ManifestModule.Name}<<<<");
            }
        }
        private void Init()
        {
            var path = Environment.CurrentDirectory + $@"\{ConstHelper.DefaultConfigFile}";

            if (File.Exists(path))
            {
                var configObj = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(File.ReadAllText(path));
                var pair = configObj.First;
                while (pair != null)
                {
                    var value = pair.GetType().GetProperty("Value").GetValue(pair);
                    if (Enum.TryParse(value.ToString(), true, out Language language))
                    {
                        ObjectCache.SetValue("language", value.ToString());
                        break;
                    }
                    pair = pair.Next;
                }
            }
        }
        private void InitTemplate()
        {
            var path = ConstHelper.DefaultDatasFile;
#if DEBUG
            path = $@"..\..\..\Datas\";
#endif
            var labelTemplate = new DocumentTemplate<Label>();
            labelTemplate.Init(path);
            var messageTemplate = new DocumentTemplate<Message>();
            messageTemplate.Init(path);

            ObjectCache.SetValue("PatchUrl", ConstHelper.PatchUrl.ToString());

            if (Enum.TryParse(ObjectCache.GetValue("language").ToString(), out Language language))
            {
                ObjectCache.SetValue("SearchPatchList", labelTemplate[Label.SearchPatchList, language]);
                ObjectCache.SetValue("Cancel", labelTemplate[Label.Cancel, language]);
                ObjectCache.SetValue("Download", labelTemplate[Label.Download, language]);
                ObjectCache.SetValue("Patching", labelTemplate[Label.Patching, language]);
                ObjectCache.SetValue("Rollback", labelTemplate[Label.Rollback, language]);

                ObjectCache.SetValue("CancelPatch", messageTemplate[Message.CancelPatch, language]);
            }
        }
    }
}
