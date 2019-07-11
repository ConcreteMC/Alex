using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using NLog;

namespace Alex.Plugins
{
    public abstract class Plugin
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger(typeof(Plugin));

        public PluginInfo Info { get; internal set; }

        protected Plugin()
        {
            Info = LoadPluginInfo();
            //    Log.Info(JsonConvert.SerializeObject(Info, Formatting.Indented));
        }

        public abstract void Enabled(Alex alex);
        public abstract void Disabled(Alex alex);

        #region Plugin Initialisation

        private PluginInfo LoadPluginInfo()
        {
            var type = GetType();

            //var info = new OpenPluginInfo();
            var info = type.GetCustomAttribute<PluginInfo>();
            if (info == null) info = new PluginInfo();

            // Fill info from the plugin's type/assembly
            var assembly = type.Assembly;

            if (string.IsNullOrWhiteSpace(info.Name))
                info.Name = type.FullName;

            if (string.IsNullOrWhiteSpace(info.Version) && !string.IsNullOrEmpty(assembly.Location))
                info.Version = AssemblyName.GetAssemblyName(assembly.Location)?.Version?.ToString() ?? "";

            //info.Version = assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ?? "";

            if (string.IsNullOrWhiteSpace(info.Description))
                info.Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "";

            if (string.IsNullOrWhiteSpace(info.Author))
                info.Author = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "";

            return info;
        }

        #endregion
    }
}
