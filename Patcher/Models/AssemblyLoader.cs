using System;
using System.Reflection;

namespace Patcher.Models
{
    internal class AssemblyLoader : MarshalByRefObject
    {
        private Assembly assembly;
        public Assembly Load(string path)
        {
            if(assembly != null)
            {
                return assembly;
            }
            assembly = Assembly.LoadFile(path);
            return assembly;
        }
    }
}
