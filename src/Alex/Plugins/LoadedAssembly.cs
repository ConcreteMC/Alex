using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Alex.Plugins
{
    public class LoadedAssembly
    {
        //	public PluginHost PluginHost { get; }
        public Assembly Assembly { get; }
        public List<Type> PluginTypes { get; }
        public List<Assembly> AssemblyReferences { get; }
        public string Path { get; }
        public LoadedAssembly(/*PluginHost host,*/ Assembly assembly, IEnumerable<Type> pluginInstances, IEnumerable<Assembly> referencedAssemblies, string path)
        {
            //	PluginHost = host;
            Assembly = assembly;
            PluginTypes = new List<Type>(pluginInstances);
            AssemblyReferences = new List<Assembly>(referencedAssemblies);
            Path = path;
        }
    }
}
