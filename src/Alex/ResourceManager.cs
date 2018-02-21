using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Net;
using Alex.Utils;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ResourcePackLib;

namespace Alex
{
    public class ResourceManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ResourceManager));

		public MCResourcePack ResourcePack { get; private set; }
		public AtlasGenerator Atlas { get; private set; }

		private GraphicsDevice Graphics { get; set; }

	    public ResourceManager(GraphicsDevice graphics)
	    {
		    Graphics = graphics;
		    Atlas = new AtlasGenerator(Graphics);
		}

	    private const string AssetVersion = "1.12"; //"18w07c";
		private static readonly string ResourcePackDirectory = Path.Combine("assets", "resourcepacks");
	    private static readonly string DefaultResourcePackPath = Path.Combine(ResourcePackDirectory, $"{AssetVersion}.zip");
		private byte[] DownloadDefaultResources()
	    {
		    var sw = new Stopwatch();

		    Log.Info("Downloading vanilla Minecraft resources...");
		    byte[] resourceData;

		    sw.Start();
		    using (var client = new WebClient())
		    {
			    resourceData =
				    client.DownloadData(string.Format("https://s3.amazonaws.com/Minecraft.Download/versions/{0}/{0}.jar",
					    AssetVersion));
		    }
		    sw.Stop();

		    Log.Info("Download took: " + Math.Round(sw.ElapsedMilliseconds / 1000D, 2) +
		             " seconds to finish");

		    Log.Info("Saving default resources...");
		    File.WriteAllBytes(DefaultResourcePackPath, resourceData);

		    return resourceData; 
		}

        public void CheckResources(GraphicsDevice device, Settings setings)
        {
			Log.Info("Checking resources...");

	        if (!Directory.Exists("assets"))
				Directory.CreateDirectory("assets");

			if (!Directory.Exists(ResourcePackDirectory))
			{
				Directory.CreateDirectory(ResourcePackDirectory);
			}

	        byte[] defaultResources;
	        if (!File.Exists(DefaultResourcePackPath))
	        {
		        defaultResources = DownloadDefaultResources();
	        }
	        else
	        {
		        defaultResources = File.ReadAllBytes(DefaultResourcePackPath);
	        }

			using (var archive = new ZipArchive(new MemoryStream(defaultResources)))
			{
				ResourcePack = new MCResourcePack(archive);
				Atlas.GenerateAtlas(device, archive);
			}

	        /*if (!string.IsNullOrEmpty(setings.ResourcePack))
	        {
		        string resourcePackPath = Path.Combine(ResourcePackDirectory, setings.ResourcePack);
		        if (File.Exists(resourcePackPath))
		        {
					Log.Info($"Loading resourcepack {resourcePackPath}");
			        using (var archive = new ZipArchive(new MemoryStream(File.ReadAllBytes(resourcePackPath))))
			        {
						Atlas.LoadResourcePackOnTop(device, archive);
			        }
		        }
	        }*/
        }
    }
}
