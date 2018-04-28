using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Alex.API;
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
		
		private GraphicsDevice Graphics { get; set; }

		public ResourceManager(GraphicsDevice graphics)
		{
			Graphics = graphics;
			Atlas = new AtlasGenerator(Graphics);
		}


		private byte[] GetLatestAssets()
		{
			using (WebClient wc = new WebClient())
			{
				var rawJson = wc.DownloadString("https://launchermeta.mojang.com/mc/game/version_manifest.json?_t=" +
				                                (long) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);

				VersionManifest manifest = VersionManifest.FromJson(rawJson);
				Version latestSnapshotVersion =
					manifest.Versions.OrderByDescending(x => x.ReleaseTime.ToUnixTimeSeconds()).FirstOrDefault();

				Log.Info($"Using assets version {latestSnapshotVersion.Id}");

				string savedPath = Path.Combine("assets", latestSnapshotVersion.Id + ".zip");
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

		private McResourcePack LoadResourcePack(IProgressReceiver progressReceiver, GraphicsDevice graphics, Stream stream, bool replaceModels = false,
			bool replaceTextures = false, bool reportMissingModels = false, McResourcePack.McResourcePackPreloadCallback preloadCallback = null)
		{
			McResourcePack resourcePack = null;

			using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, false))
			{
				resourcePack = new McResourcePack(archive, graphics, preloadCallback);
			}

			Log.Info($"Loaded {resourcePack.BlockModels.Count} block models from resourcepack");
			Log.Info($"Loaded {resourcePack.ItemModels.Count} item models from resourcepack");

			if (!replaceTextures)
			{
				Atlas.LoadResourcePackOnTop(resourcePack.TexturesAsBitmaps.Where(x => x.Key.StartsWith("blocks")).ToArray(),
					progressReceiver);
			}
			else
			{
				Atlas.GenerateAtlas(resourcePack.TexturesAsBitmaps.Where(x => x.Key.StartsWith("blocks")).ToArray(),
					progressReceiver);
			}

			Stopwatch sw = Stopwatch.StartNew();
			int imported = BlockFactory.LoadResources(this, resourcePack, replaceModels, reportMissingModels, progressReceiver);
			sw.Stop();

			Log.Info($"Imported {imported} blockstate variants from resourcepack in {sw.ElapsedMilliseconds}ms!");

			ItemFactory.Init(this, resourcePack);

			return resourcePack;
		}

		private static string Root = "assets";
		private static readonly string ResourcePackDirectory = Path.Combine(Root, "resourcepacks");
		private static readonly string BedrockResourcePackPath = Path.Combine(Root, "bedrock.zip");

		private bool CheckRequiredPaths(out byte[] javaResources, out byte[] bedrockResources)
		{
			if (!Directory.Exists(Root))
			{
				Directory.CreateDirectory(Root);
			}

			if (!Directory.Exists(ResourcePackDirectory))
			{
				Directory.CreateDirectory(ResourcePackDirectory);
			}

			if (!File.Exists(BedrockResourcePackPath))
			{
				Log.Error(
					$"Missing bedrock edition resources! Please put a copy of the bedrock resources in a zip archive with the path '{BedrockResourcePackPath}'");
				javaResources = null;
				bedrockResources = null;
				return false;
			}

			try
			{
				javaResources = GetLatestAssets();
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Could not check for latests assets! Do you have a internet connection up?");
				javaResources = null;
				bedrockResources = null;
				return false;
			}

			bedrockResources = File.ReadAllBytes(BedrockResourcePackPath);

			return true;
		}

		public bool CheckResources(GraphicsDevice device, Settings setings, IProgressReceiver progressReceiver, McResourcePack.McResourcePackPreloadCallback preloadCallback)
		{
			byte[] defaultResources;
			byte[] bedrockResources;

			if (!CheckRequiredPaths(out defaultResources, out bedrockResources))
			{
				return false;
			}

			Log.Info($"Loading vanilla resources...");
			using (MemoryStream stream = new MemoryStream(defaultResources))
			{
				ResourcePack = LoadResourcePack(progressReceiver, device, stream, true, true, true, preloadCallback);
			}

			//report(ResourcePack.AsciiFont);

			Log.Info($"Loading bedrock resources...");
			BedrockResourcePack = new BedrockResourcePack(bedrockResources);

			EntityFactory.LoadModels(this, device, true, progressReceiver);


			Log.Info($"Loading known entity data...");
			EntityFactory.Load();

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
							LoadResourcePack(progressReceiver, device, stream, true, false, false, preloadCallback);
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
