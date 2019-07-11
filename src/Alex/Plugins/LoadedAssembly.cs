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
        public List<Plugin> PluginInstances { get; }
        public List<Assembly> AssemblyReferences { get; }
        public string Path { get; }
        public LoadedAssembly(/*PluginHost host,*/ Assembly assembly, IEnumerable<Plugin> pluginInstances, IEnumerable<Assembly> referencedAssemblies, string path)
        {
            //	PluginHost = host;
            Assembly = assembly;
            PluginInstances = new List<Plugin>(pluginInstances);
            AssemblyReferences = new List<Assembly>(referencedAssemblies);
            Path = path;
        }
    }
}
