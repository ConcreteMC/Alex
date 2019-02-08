using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace ResourceConverter
{
    public class Program
    {

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
                Console.WriteLine("\tResourceConverter.exe <inputDirectory> [outputDirectory]");
                Console.ReadLine();
            }


            string inputDir = null, outputDir = null;

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


            ResourceConverter.Run(inputDir, outputDir);
            Console.WriteLine("Completed.");
            Console.ReadLine();
        }
    }
}
