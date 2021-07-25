using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Mono.Cecil;
using NLog;

namespace Alex.Plugins
{
    internal class AssemblyResolver
    {
	    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
	    
	    private AssemblyManager AssemblyManager { get; }
        public AssemblyResolver(AssemblyManager assemblyManager)
        {
	        AssemblyManager = assemblyManager;
	        
	        AppDomain.CurrentDomain.AssemblyResolve += PluginManagerOnAssemblyResolve;
        }
        
        private Assembly PluginManagerOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
	        try
	        {
		        //  AssemblyName name = new AssemblyName(args.Name);
		        AssemblyNameReference name = AssemblyNameReference.Parse(args.Name);
		        if (name.Name.EndsWith(".resources") && !name.Culture.EndsWith("neutral")) return null;
				
		        if (AssemblyManager.IsLoaded(name.Name, out Assembly loadedPluginAssembly))
		        {
			        return loadedPluginAssembly;
		        }

		        string rootPath = "";
		        if (args.RequestingAssembly != null && !string.IsNullOrWhiteSpace(args.RequestingAssembly.Location))
		        {
			        rootPath = Path.GetDirectoryName(args.RequestingAssembly.Location);
		        }

		        Assembly result = null;
		        if (TryFindAssemblyPath(name, rootPath, out string resultPath))
		        {
			        AssemblyManager.TryLoadAssemblyFromFile(name.Name, resultPath, out result);
			        //result = Assembly.LoadFile(resultPath);
		        }
		        else
		        {
			        var assembly = AppDomain.CurrentDomain.GetAssemblies()
				        .FirstOrDefault(x => x.GetName().Name == args.Name);
			        
			        if (assembly != null) 
				        result = assembly;
		        }

		        return result;
	        }
	        catch (Exception ex)
	        {
		        Log.Error(ex, $"Failed to resolve!");
		        return null;
	        }
        }
        
        internal bool TryResolve(string path, ModuleDefinition module, out Assembly[] assemblies)
        {
	        //var proxy = Proxy.CreateProxy(domain);
	        List<AssemblyNameReference> assemblyNames = module.AssemblyReferences.ToList();

	        Dictionary<AssemblyNameReference, Assembly> resolvedAssemblies = new Dictionary<AssemblyNameReference, Assembly>();
	        Dictionary<AssemblyNameReference, string> resolvedPaths = new Dictionary<AssemblyNameReference, string>();
	        foreach (var assemblyName in assemblyNames)
	        {
		        if (AssemblyManager.IsLoaded(assemblyName.Name, out Assembly loadedAssembly))
		        {
			        resolvedAssemblies.Add(assemblyName, loadedAssembly);
			        continue;
		        }

		        try
		        {
			        if (TryFindAssemblyPath(assemblyName, path, out string resultPath))
			        {
				        resolvedPaths.Add(assemblyName, resultPath);
			        }
			        else
			        {
				        /*var resolved = module.AssemblyResolver.Resolve(assemblyName);

				        if (resolved != null)
				        {
					        using (MemoryStream ms = new MemoryStream())
					        {
						        resolved.Write(ms);
						        var assembly = Assembly.Load(ms.ToArray());
						        resolvedAssemblies.Add(assemblyName, assembly);
					        }
					        continue;
				        }
*/
				        Log.Warn($"Plugin \"{module.FileName}\" requires \"{assemblyName}\" but it could not be found.");
				          assemblies = default(Assembly[]);
				         return false;
			        }
		        }
		        catch(Exception e)
		        {
			        Log.Warn($"Could not find path for {assemblyName} - {e.ToString()}");
			        assemblies = default(Assembly[]);
			        return false;
		        }
	        }

	        foreach (var resolved in resolvedPaths)
	        {
		        if (AssemblyManager.TryLoadAssemblyFromFile(resolved.Key.Name, resolved.Value, out Assembly loaded))
		        {
			        resolvedAssemblies.TryAdd(resolved.Key, loaded);
		        }
	        }

	        assemblies = resolvedAssemblies.Values.ToArray();
	        return true;
        }

        private bool TryFindAssemblyPath(AssemblyNameReference name, string rootPath, out string resultingPath)
	    {
		    AssemblyDependencyResolver resolver = new AssemblyDependencyResolver(rootPath);
		    var p = resolver.ResolveAssemblyToPath(new AssemblyName(name.ToString()));
		    if (p != null)
		    {
			    resultingPath = p;
			    return true;
		    }

		    List<string> directories = new List<string>();
		    directories.Add(rootPath);
		    string execPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		    if (!directories.Contains(execPath))
				directories.Add(execPath);
		    
		    execPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
		    if (!directories.Contains(execPath))
			    directories.Add(execPath);

		    foreach (var path in AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic)
			   .Select(x => Path.GetDirectoryName(x.Location)))
		    {
			    if (!directories.Contains(path))
				    directories.Add(path);
		    }

		    string dllName = name.Name + ".dll";
		    string result = null;
		    
		    foreach (var directory in directories)
		    {
			    string file = Path.Combine(directory, dllName);
			    if (CompareFileToAssemblyName(file, name) == FileAssemblyComparisonResult.Match)
			    {
				    result = file;
				    break;
			    }
		    }

		    if (string.IsNullOrWhiteSpace(result))
		    {
			    resultingPath = default(string);
			    return false;
			}
		    else
		    {
			    resultingPath = result;
			    return true;
		    }
		}

	    private enum FileAssemblyComparisonResult
	    {
			FileNotFound,
			NotEqual,
			Match
	    }

	    private FileAssemblyComparisonResult CompareFileToAssemblyName(string file, AssemblyNameReference name)
	    {
		   // Log.Info($"Lookup: {file}");

		    if (!File.Exists(file))
			    return FileAssemblyComparisonResult.FileNotFound;

		    var module = ModuleDefinition.ReadModule(file);
			//AssemblyName fileAssemblyName = AssemblyName.GetAssemblyName(file);
				// if (AssemblyName.ReferenceMatchesDefinition(fileAssemblyName, new AssemblyName(name.FullName)))
			if (name.Name.Equals(module.Assembly.Name.Name, StringComparison.InvariantCultureIgnoreCase))
		    {
			    return FileAssemblyComparisonResult.Match;
			}
		    else
		    {
			    return FileAssemblyComparisonResult.NotEqual;
		    }
	    }
    }
}