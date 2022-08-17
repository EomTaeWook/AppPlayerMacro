using Patcher.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Patcher.Infrastructure
{
    internal class AssemblyManager
    {
        private AppDomain _appDomain;
        private string path;
        private Dictionary<string, AssemblyLoader> assemblyToMap = new Dictionary<string, AssemblyLoader>();
        public AssemblyManager()
        {
            path = $"{AppDomain.CurrentDomain.BaseDirectory}shadow";
            var setup = new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                PrivateBinPath = path,
                ShadowCopyFiles = "true",
                ShadowCopyDirectories = path,
            };

            _appDomain = AppDomain.CreateDomain("AssemblyManager", AppDomain.CurrentDomain.Evidence, setup);
        }

        public Assembly LoadAssembly(string path)
        {
            if(assemblyToMap.ContainsKey(path) == true)
            {
                assemblyToMap[path].Load(path);
            }

            AssemblyLoader loader = _appDomain.CreateInstanceAndUnwrap(typeof(AssemblyLoader).Assembly.FullName, "AssemblyLoader") as AssemblyLoader;

            assemblyToMap.Add(path, loader);

            return loader.Load(path);
        }

        public void UnLoad()
        {
            if(_appDomain != null)
            {
                AppDomain.Unload(_appDomain);
            }
        }

    }
}
