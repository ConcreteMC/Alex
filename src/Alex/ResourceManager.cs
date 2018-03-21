using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Alex.Entities;
using Alex.Rendering;
using Alex.ResourcePackLib;
using Alex.Utils;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using NLog;

namespace Alex
{
    public class ResourceManager
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ResourceManager));

		public McResourcePack ResourcePack { get; private set; }
		public BedrockResourcePack BedrockResourcePack { get; private set; }
		public AtlasGenerator Atlas { get; private set; }

		public ResourcePackUiThemeBuilder UiThemeFactory { get;private set; }

		private GraphicsDevice Graphics { get; set; }

	    public ResourceManager(GraphicsDevice graphics)
	    {
		    Graphics = graphics;
		    Atlas = new AtlasGenerator(Graphics);
			UiThemeFactory = new ResourcePackUiThemeBuilder(Graphics);
		}

		private static readonly string ResourcePackDirectory = Path.Combine("assets", "resourcepacks");
	    private static readonly string BedrockResourcePackPath = Path.Combine("assets", "bedrock.zip");

		private byte[] GetLatestAssets()
		{
			using (WebClient wc = new WebClient())
			{
				var rawJson = wc.DownloadString("https://launchermeta.mojang.com/mc/game/version_manifest.json?_t=" +
				                                   (long) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);

				VersionManifest manifest = VersionManifest.FromJson(rawJson);
				Version latestSnapshotVersion =
					manifest.Versions.OrderByDescending(x => x.ReleaseTime.ToUnixTimeSeconds()).FirstOrDefault();

				string savedPath = Path.Combine("assets", latestSnapshotVersion.Id + ".bin");
				if (!File.Exists(savedPath))
				{
					Log.Info("Downloading latest vanilla Minecraft resources...");
					LauncherMeta meta = LauncherMeta.FromJson(wc.DownloadString(latestSnapshotVersion.Url));
					byte[] clientData = wc.DownloadData(meta.Downloads.Client.Url);
					File.WriteAllBytesAsync(savedPath, clientData);
					return clientData;
				}
				else
				{
					return File.ReadAllBytes(savedPath);
				}
			}
		}

	    private McResourcePack LoadResourcePack(GraphicsDevice graphics, Stream stream, bool replaceModels = false, bool replaceTextures = false, bool reportMissingModels = false)
	    {
		    McResourcePack resourcePack = null;

			using (var archive = new ZipArchive(stream))
		    {
			    resourcePack = new McResourcePack(archive);
			    if (!replaceTextures)
			    {
				    Atlas.LoadResourcePackOnTop(graphics, archive);
			    }
			    else
			    {
				    Atlas.GenerateAtlas(graphics, archive);
			    }

				Stopwatch sw = Stopwatch.StartNew();
				int imported = BlockFactory.LoadResources(this, resourcePack, replaceModels, reportMissingModels);
				sw.Stop();

				Log.Info($"Imported {imported} blockstate variants from resourcepack in {sw.ElapsedMilliseconds}ms!");
		    }

		    return resourcePack;
	    }

        public bool CheckResources(GraphicsDevice device, Settings setings)
        {
	        if (!Directory.Exists("assets"))
				Directory.CreateDirectory("assets");

			if (!Directory.Exists(ResourcePackDirectory))
			{
				Directory.CreateDirectory(ResourcePackDirectory);
			}

	        byte[] defaultResources = GetLatestAssets();

	        Log.Info($"Loading vanilla resources...");
	        using (MemoryStream stream = new MemoryStream(defaultResources))
	        {
		        ResourcePack = LoadResourcePack(device, stream, true, true, true);
	        }

			 if (File.Exists(BedrockResourcePackPath))
			 {
				 Log.Info($"Loading bedrock resources...");
				 BedrockResourcePack = new BedrockResourcePack(File.ReadAllBytes(BedrockResourcePackPath));
				 UiThemeFactory.LoadResources(BedrockResourcePack);
			}
	        else
	        {
		        Log.Error($"Could not start, missing bedrock resources! Please place 'bedrock.zip' in the assets folder!");
		        return false;
	        }

			EntityFactory.LoadModels(this, device, true);

			foreach (string file in setings.ResourcePacks)
	        {
		        try
		        {
			        string resourcePackPath = Path.Combine(ResourcePackDirectory, file);
			        if (File.Exists(resourcePackPath))
			        {
				        Log.Info($"Loading resourcepack {file}...");

				        using (FileStream stream = new FileStream(resourcePackPath, FileMode.Open))
				        {
					        LoadResourcePack(device, stream, true, false);
				        }
			        }
		        }
		        catch (Exception e)
		        {
					Log.Warn(e, $"Could not load resourcepack {file}!");
		        }
	        }
			return true;
        }
    }
}
