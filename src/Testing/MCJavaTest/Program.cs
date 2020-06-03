using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Alex.API.Services;
using Alex.Networking.Java;
using Alex.Utils;

namespace MCJavaTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
	        Regex regex = new Regex(@"(?<start>.*)(?<lambda>\(\)\s\=\>.*)\.WithLocation\((?<location>.+?(?=\)))\)\);", RegexOptions.Multiline);
	        string file = "../../../../../Alex/Blocks/BlockRegistry.cs";
	        string fileContents = File.ReadAllText(file);

	        foreach (Match match in regex.Matches(fileContents))
	        {
		        Console.WriteLine($"this.Register({match.Groups["location"].Value}, {match.Groups["lambda"]});");
	        }
	        
	        /*var path = Path.Combine(Path.GetTempPath(), "Alex");
	        Directory.CreateDirectory(path);
	        Console.WriteLine($"Hello World: {path}");
            

            var mcJavaAssets = new MCJavaAssetsUtil(new StorageSystem(path));
            await mcJavaAssets.EnsureTargetReleaseAsync(JavaProtocol.VersionId, new SplashScreen());
            */
            Console.WriteLine("Done!");
            Console.ReadLine();
        }
        
        
       /* static async Task Main(string[] args)
        {
        	Console.WriteLine("Hello World!");
        
        	var path = Path.Combine(Path.GetTempPath(), "Alex");
        	
            // uncomment this!
            //Alex.ConfigureNLog(path);
        	//Log.Info("Hello Console!");
        	
                  
        	var mcJavaAssets = new MCJavaAssetsUtil(new StorageSystem(path));
        	await mcJavaAssets.EnsureLatestReleaseAsync();
                  
        	Console.WriteLine("Done!");
        	Console.ReadLine();
        }*/
    }
}