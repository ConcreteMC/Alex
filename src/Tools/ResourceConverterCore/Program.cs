using System;
using System.IO;
using System.Reflection;
using log4net;
using NLog;
using ResourceConverterCore.Converter;
using ResourceConverterCore.Properties;
using LogManager = NLog.LogManager;

namespace ResourceConverterCore
{
    public class Program
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static string DefaultOutputDir
        {
            get => Path.Combine(Assembly.GetEntryAssembly().Location, "Output");
    }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                // Show Help
                Console.WriteLine("Usage:");
                Console.WriteLine("\tResourceConverter.exe <inputDirectory> [outputDirectory] [type]");
                Console.ReadLine();
            }


            string inputDir = null, outputDir = null, type = null;

            if (args.Length == 1)
            {
                // Only input dir, default output dir
                inputDir = args[0];
                outputDir = DefaultOutputDir;
            }
            else if(args.Length == 2)
            {
                inputDir = args[0];
                outputDir = args[1];
            }
            else if(args.Length == 3)
            {
                inputDir = args[0];
                outputDir = args[1];
                type = args[2];
            }

            if (string.IsNullOrWhiteSpace(inputDir))
            {
                Console.WriteLine("Input dir not specified!");
                Console.ReadLine();
                return;
            }
            if (string.IsNullOrWhiteSpace(outputDir))
            {
                Console.WriteLine("Output dir not specified!");
                Console.ReadLine();
                return;
            }

            if (!Directory.Exists(inputDir))
            {
                Console.WriteLine("Input directory does not exist! ({0})", inputDir);
                Console.ReadLine();
                return;
            }

            if (!Directory.Exists(outputDir))
            {
                Console.WriteLine("Output directory does not exist! ({0})", outputDir);
                Console.ReadLine();
                return;
            }

            var actualOutput = Path.Combine(outputDir, "output");

            if (!Directory.Exists(actualOutput))
	            Directory.CreateDirectory(actualOutput);

			ConfigureNLog(outputDir);

			ResourceConverter.Run(inputDir, actualOutput, type);
            Console.WriteLine("Completed.");
            Console.ReadLine();
        }

        private static void ConfigureNLog(string baseDir)
        {
	        string loggerConfigFile = Path.Combine(baseDir, "NLog.config");
	        if (!File.Exists(loggerConfigFile))
	        {
		        File.WriteAllText(loggerConfigFile, Resources.NLogConfig);
	        }

	        string logsDir = Path.Combine(baseDir, "logs");
	        if (!Directory.Exists(logsDir))
	        {
		        Directory.CreateDirectory(logsDir);
	        }

	        NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(loggerConfigFile, true);
	        LogManager.Configuration.Variables["basedir"] = baseDir;

	        NLogAppender.Initialize();
        }
    }
}
