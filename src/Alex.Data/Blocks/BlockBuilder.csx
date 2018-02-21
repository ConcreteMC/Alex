using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using ResourcePackLib;
using System.Net;

private const string AssetVersion = "1.12"; //"18w07c";
private static readonly string ResourcePackDirectory = Path.Combine("assets", "resourcepacks");
private static readonly string DefaultResourcePackPath = Path.Combine(ResourcePackDirectory, $"{AssetVersion}.zip");

private byte[] DownloadDefaultResources()
{
	var sw = new Stopwatch();

	Console.WriteLine("Downloading vanilla Minecraft resources...");
	byte[] resourceData;

	sw.Start();
	using (var client = new WebClient())
	{
		resourceData =
			client.DownloadData(string.Format("https://s3.amazonaws.com/Minecraft.Download/versions/{0}/{0}.jar",
				AssetVersion));
	}
	sw.Stop();

	Console.WriteLine("Download took: " + Math.Round(sw.ElapsedMilliseconds / 1000D, 2) +
			 " seconds to finish");

	Console.WriteLine("Saving default resources...");
	File.WriteAllBytes(DefaultResourcePackPath, resourceData);

	return resourceData;
}

