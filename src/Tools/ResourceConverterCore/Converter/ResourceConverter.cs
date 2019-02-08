using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Alex.ResourcePackLib.Json.Models.Entities;
using NLog;
using ResourceConverter.Templates;

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

            var template = new EntityTemplate();
            template.Initialize();

//            template.Session["EntityModels"] = loader.EntityModels;
            ResourceConverterContext.EntityModels = loader.EntityModels;

            int count = 0;
            int totalCount = loader.EntityModels.Count;
            foreach (var model in loader.EntityModels)
            {
                Log.Info($"Starting Template Processing for '{model.Key}'");

                //template.Session["CurrentModelName"] = model.Key;
                //template.Session["CurrentModel"] = model.Value;
                ResourceConverterContext.CurrentModelName = model.Key;
                ResourceConverterContext.CurrentModel = model.Value;


                var output = template.TransformText();
                var outputPath = Path.Combine(outputDirectoryPath, model.Key + ".cs");

                File.WriteAllText(outputPath, output);
                count++;
                var pct = count / (double)totalCount;
                Log.Info($"Successfully Processed Template for entity '{model.Key}' ({count}/{totalCount}) - {pct:F1}%");
            }
        }

        private static string ToPascalCase(string key)
        {
            // TODO
            return key.ToLowerInvariant();
        }

    }
}
