using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Alex.ResourcePackLib.Json.Models.Entities;
using Newtonsoft.Json;
using NLog;
using Templates;

namespace ResourceConverter
{
    public static class ResourceConverter
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        

        public static void Run(string inputDirectoryPath, string outputDirectoryPath)
        {
            var inputDirectory = new DirectoryInfo(inputDirectoryPath);
            var outputDirectory = new DirectoryInfo(outputDirectoryPath);


            if (!outputDirectory.Exists)
            {
                //outputDirectory.Delete(true);
                outputDirectory.Create();
            }

            var loader = new ResourceLoader(inputDirectory);
			
            loader.Load();

			GenerateModelFiles(loader, outputDirectory, out var classMapping);

        }

        private static void GenerateModelFactory(IReadOnlyDictionary<string, string> geometryToClass)
        {

        }
		
        private static void GenerateModelFiles(ResourceLoader loader, DirectoryInfo outputDirectory, out Dictionary<string, string> geometryToClass)
        {
	        geometryToClass = new Dictionary<string, string>();

               var template = new EntityTemplate();
	        template.Initialize();

	        //            template.Session["EntityModels"] = loader.EntityModels;
	        ResourceConverterContext.EntityModels = loader.EntityModels;
			
	        int count = 0;
	        int totalCount = loader.EntityModels.Count;
	        foreach (var model in loader.EntityModels)
	        {
		        var pct = 100D * ((double)count / (double)totalCount);

		        Log.Info($"Starting Template Processing for '{model.Key}'");

		        //template.Session["CurrentModelName"] = model.Key;
		        //template.Session["CurrentModel"] = model.Value;
		        ResourceConverterContext.CurrentModelName = CodeTypeName(model.Value.Name);
		        ResourceConverterContext.CurrentModel = model.Value;

		        var output = template.TransformText();
		        var outputPath = Path.Combine(outputDirectory.FullName, CodeTypeName(model.Value.Name) + "Model.cs");
		        if (File.Exists(outputPath))
		        {
			        Log.Warn($"Class already exists: {outputPath} ({count}/{totalCount}) - {pct:F1}%");
		        }
		        else
		        {
			        geometryToClass.Add(model.Key, ResourceConverterContext.CurrentModelName + "Model");
                    File.WriteAllText(outputPath, output);
			        Log.Info($"Successfully Processed Template for entity '{model.Key}' ({count}/{totalCount}) - {pct:F1}%");
		        }

		        

                count++;
		        // Log.Info($"Successfully Processed Template for entity '{model.Key}' ({count}/{totalCount}) - {pct:F1}%");
	        }
        }

        private static string CodeTypeName(string name)
        {
	        if (name.StartsWith("geometry."))
	        {
		        name = name.Substring("geometry.".Length);

	        }
	        else if (name.StartsWith("definition."))
	        {
		        name = name.Substring("definition.".Length);
            }

	        return CodeName(name, true);
        }

        private static string CodeName(string name, bool firstUpper = false)
        {
	        name = name.ToLowerInvariant();

	        string result = name;
	        bool upperCase = firstUpper;

	        result = string.Empty;
	        for (int i = 0; i < name.Length; i++)
	        {
		        if (name[i] == '.' || name[i] == ' ' || name[i] == '_')
		        {
			        upperCase = true;
		        }
		        else
		        {
			        if ((i == 0 && firstUpper) || upperCase)
			        {
				        result += name[i].ToString().ToUpperInvariant();
				        upperCase = false;
			        }
			        else
			        {
				        result += name[i];
			        }
		        }
	        }

	        result = result.Replace(@"[]", "s");
	        return result;
        }

        private static string ToPascalCase(string key)
        {
            // TODO
            return key.ToLowerInvariant();
        }

    }
}
