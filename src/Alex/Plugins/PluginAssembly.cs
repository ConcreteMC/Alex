using System;
using System.Collections.Generic;
using System.Reflection;
using NLog;

namespace Alex.Plugins
{
    public class PluginAssembly
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        
        public Assembly Assembly { get; }
        public List<Type> PluginTypes { get; }
        private Dictionary<Assembly, LoadedAssembly> LoadedAssemblies { get; }
        public PluginAssembly(Assembly assembly, List<Type> pluginTypes)
        {
            Assembly = assembly;
            PluginTypes = pluginTypes;
        }

        /*public List<Plugin> InitiatePlugins()
        {
            var plugins = new List<Plugin>();
            foreach (var type in PluginTypes)
            {
                ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
                if (ctor != null)
                {
                    Plugin plugin;
                    try
                    {
                        plugin = (Plugin) ctor.Invoke(null);
                    }
                    catch (Exception ex)
                    {
                        plugin = null;
                        Log.Error(ex, $"An error has occurred: {ex.ToString()}");
                    }

                    if (plugin != null)
                    {
                        plugins.Add(plugin);
                    }
                }
                else
                {
                    foreach (ConstructorInfo constructor in type.GetConstructors())
                    {
                        List<Assembly> assembliesReferenced = new List<Assembly>();
                        List<object> parameters = new List<object>();
                        foreach (ParameterInfo argument in constructor.GetParameters())
                        {
                            /*if (argument.ParameterType == typeof(Alex))
                            {
                                parameters.Add(Parent);
                                continue;
                            }*

                            if (References.TryGetValue(argument.ParameterType, out object arg))
                            {
                                parameters.Add(arg);

                                Assembly argsAssembly = arg.GetType().Assembly;
                                if (!assembliesReferenced.Contains(argsAssembly))
                                {
                                    assembliesReferenced.Add(argsAssembly);
                                }

                                continue;
                            }

                            var serviceInstance = ServiceProvider.GetRequiredService(argument.ParameterType);
                            if (serviceInstance != null)
                            {
                                parameters.Add(serviceInstance);
                                continue;
                            }

                            foreach (LoadedAssembly loadedPlugin in LoadedAssemblies.Values)
                            {
                                foreach (Plugin loadedPlug in loadedPlugin.PluginInstances)
                                {
                                    if (argument.ParameterType == loadedPlug.GetType())
                                    {
                                        parameters.Add(loadedPlug);

                                        if (loadedPlugin.Assembly != assembly
                                        ) //If the instance of the type is not from the assembly being loaded, add the type's assembly to a list of dependencies
                                        {
                                            if (!assembliesReferenced.Contains(loadedPlugin.Assembly))
                                            {
                                                assembliesReferenced.Add(loadedPlugin.Assembly);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (parameters.Count == constructor.GetParameters().Length)
                        {
                            var plugin = (Plugin) constructor.Invoke(parameters.ToArray());
                            foreach (Assembly reference in assembliesReferenced)
                            {
                                if (!refAssemblies.Contains(reference))
                                {
                                    refAssemblies.Add(reference);
                                }
                            }

                            plugins.Add(plugin);
                            //Log.Info($"Plugin instance created: {plugin.GetType().FullName}");
                            break;
                        }
                        else
                        {
                            Log.Warn($"Could not call constructor for {constructor.ToString()}");
                        }
                    }
                }
            }

            return plugins;
        }*/
    }
}