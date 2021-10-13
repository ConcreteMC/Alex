using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Alex.ResourcePackLib.Bedrock;
using Alex.ResourcePackLib.Exceptions;
using Alex.ResourcePackLib.IO.Abstract;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Bedrock;
using NLog;

namespace Alex.ResourcePackLib
{
	public class MCPack
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MCPack));
		
		public McPackManifest Manifest { get; private set; }
		public MCPackModule[] Modules { get; private set; }
		
		public MCPack(IFilesystem archive)
		{
			Load(archive);
		}

		private void Load(IFilesystem archive)
		{
			var manifestEntry = archive.GetEntry("manifest.json");
			//var contentEntry  = archive.GetEntry("content.zipe");
			
			if (manifestEntry == null)
			{
				throw new InvalidMCPackException("No manifest found!");
			}

			Manifest = MCJsonConvert.DeserializeObject<McPackManifest>(manifestEntry.ReadAsString());

			//if (contentEntry == null)
			//{
			//	throw new InvalidMCPackException($"No content found for MCPack: {Manifest.Header.Name}");
			//}
			
			List<MCPackModule> modules = new List<MCPackModule>();
			
			foreach (var module in Manifest.Modules)
			{
				switch (module.Type.ToLower())
				{
					//case "resources":
					//	modules.Add(new ResourceModule(archive));
					//	break;
					case "skin_pack":
						try
						{
							modules.Add(new SkinModule(archive));
						}
						catch
						{
							
						}
						break;
					default:
						Log.Warn($"Unknown resourcepack module type found in pack! Found '{module.Type}' in manifest '{Manifest.Header.Name}'");
						break;
				}
			}

			List<MCPackModule> toRemove = new List<MCPackModule>();
			foreach (var module in modules)
			{
				bool loaded = false;
				try
				{
					loaded = module.Load();
				}
				catch (Exception ex)
				{
					Log.Error(ex,$"Failed to load MCPack module: {module.Name} from {Manifest.Header.Name}: {ex}");
					loaded = false;
					//toRemove.Add(module);
				}

				if (!loaded)
				{
					Log.Warn($"Failed to load MCPack module \'{module.Name}\' from {Manifest.Header.Name}.");
					toRemove.Add(module);
				}
			}

			foreach (var rm in toRemove)
			{
				modules.Remove(rm);
			}
			
			Modules = modules.ToArray();
		}
	}
}