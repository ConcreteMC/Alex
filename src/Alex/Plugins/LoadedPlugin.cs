using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.Plugins
{
    public class LoadedPlugin
    {
        public bool Enabled { get; }
        public PluginInfo Info { get; }
        public Plugin Instance { get; }
        public string[] Dependencies;
        internal LoadedPlugin(Plugin pluginInstance, PluginInfo info, bool enabled)
        {
            Instance = pluginInstance;
            Enabled = enabled;
            Info = info;
        }
    }
}
