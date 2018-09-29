using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Tags;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Alex.ResourcePackLib
{
	public class DataPack
	{
		private readonly Dictionary<string, Tag> _blockTags = new Dictionary<string, Tag>();
		private readonly Dictionary<string, Tag> _itemTags = new Dictionary<string, Tag>();

		private ZipArchive _archive;
	    public DataPack(ZipArchive archive)
	    {
		    _archive = archive;

		    Load();
	    }

	    private void Load()
	    {
		    LoadBlockTags();
		    LoadItemTags();
	    }

		private void LoadItemTags()
		{
			var jsonFiles = _archive.Entries
				.Where(e => e.FullName.StartsWith("data/minecraft/tags/items/") && e.FullName.EndsWith(".json")).ToArray();

			foreach (var jsonFile in jsonFiles)
			{
				var tag = LoadTag(jsonFile);
				_itemTags[$"{tag.Namespace}:{tag.Name}"] = tag;
			}
		}

	    private void LoadBlockTags()
	    {
		    var jsonFiles = _archive.Entries
			    .Where(e => e.FullName.StartsWith("data/minecraft/tags/block/") && e.FullName.EndsWith(".json")).ToArray();

		    foreach (var jsonFile in jsonFiles)
		    {
			    var tag = LoadTag(jsonFile);
			    _blockTags[$"{tag.Namespace}:{tag.Name}"] = tag;
			}
	    }

		private Tag LoadTag(ZipArchiveEntry entry)
		{
			string nameSpace = entry.FullName.Split('/')[1];
			string name = Path.GetFileNameWithoutExtension(entry.FullName);
			using (var r = new StreamReader(entry.Open()))
			{
				var blockModel = MCJsonConvert.DeserializeObject<Tag>(r.ReadToEnd());
				blockModel.Name = name;
				blockModel.Namespace = nameSpace;

				blockModel = ProcessTag(blockModel);
				return blockModel;
			}
		}

		private Tag ProcessTag(Tag tag)
		{

			return tag;
		}
	}
}
