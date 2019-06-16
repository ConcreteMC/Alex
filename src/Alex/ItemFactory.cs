using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Alex.API.Utils;
using Alex.Blocks;
using Alex.Items;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Models.Items;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;

namespace Alex
{
    public static class ItemFactory
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ItemFactory));

		private static ResourceManager ResourceManager { get; set; }
		private static McResourcePack ResourcePack { get; set; }
		private static IReadOnlyDictionary<string, Item> Items { get; set; }
		private static SecondItemEntry[] SecItemEntries { get; set; }
	    public static void Init(ResourceManager resources, McResourcePack resourcePack, IProgressReceiver progressReceiver = null)
	    {
		    ResourceManager = resources;
		    ResourcePack = resourcePack;

		    var otherRaw = ResourceManager.ReadStringResource("Alex.Resources.items3.json");
		    SecItemEntries = JsonConvert.DeserializeObject<SecondItemEntry[]>(otherRaw);
		    
		    var raw = ResourceManager.ReadStringResource("Alex.Resources.items2.json");
		    
		    ItemEntry[] itemData = JsonConvert.DeserializeObject<ItemEntry[]>(raw);


		    var ii = resources.Registries.Items.Entries;

            Dictionary<string, Item> items = new Dictionary<string, Item>();
		    for(int i = 0; i < ii.Count; i++)
		    {
			    var entry = ii.ElementAt(i);
                progressReceiver?.UpdateProgress(i * (100 / ii.Count), $"Processing items...", entry.Key);
                
			    var blockState = BlockFactory.GetBlockState(entry.Key);

			    Item item;
			    if (blockState != null)
			    {
				    item = new ItemBlock(blockState);
                }
			    else
			    {
				    item = new Item();
			    }

			    var minetItem = MiNET.Items.ItemFactory.GetItem(entry.Key.Replace("minecraft:", ""));
			    if (minetItem != null)
			    {
				    if (Enum.TryParse<ItemType>(minetItem.ItemType.ToString(), out ItemType t))
				    {
					    item.ItemType = t;
				    }
				    item.Material = minetItem.ItemMaterial;
			    }

                item.DisplayName = entry.Key;

			    var data = itemData.FirstOrDefault(x =>
				    x.name.Equals(entry.Key.Substring(10), StringComparison.InvariantCultureIgnoreCase));
			    if (data != null)
			    {
				    item.MaxStackSize = data.stackSize;
				    item.DisplayName = data.displayName;
			    }

			    items.TryAdd(entry.Key, item);
		    }

			Items = new ReadOnlyDictionary<string, Item>(items);
	    }

	    public static bool ResolveItemTexture(string itemName, out Texture2D texture)
	    {
		    if (ResourcePack.ItemModels.TryGetValue(itemName, out ResourcePackItem item))
		    {
			    var texture0 = item.Textures.FirstOrDefault();
			    if (texture0.Value != null)
			    {
				    if (ResourcePack.TryGetTexture(texture0.Value, out texture))
				    {
					    return true;
                    }
				    else
				    {
						Log.Debug($"Could not find texture for item: {itemName} (Search Term: {texture0.Value})");
				    }
			    }
            }
		    else
		    {
			    if (ResourcePack.TryGetBlockModel(itemName, out var b))
			    {
				    var texture0 = b.Textures.OrderBy(x => x.Value.Contains("side")).FirstOrDefault();
				    if (texture0.Value != null)
				    {
					    if (ResourcePack.TryGetTexture(texture0.Value, out texture))
					    {
						    return true;
					    }
					    else
					    {
						    Log.Debug($"Could not find texture for item: {itemName} (Search Term: {texture0.Value})");
					    }
				    }
                }
			    else
			    {
				    Log.Debug($"Could not find model for item: {itemName}");
                }
            }

		    texture = null;
		    return false;
	    }

	    public static bool ResolveItemName(int protocolId, out string res)
	    {
		    var result = ResourceManager.Registries.Items.Entries.FirstOrDefault(x => x.Value.ProtocolId == protocolId).Key;
		    if (result != null)
		    {
			    res = result;
			    return true;
		    }

		    res = null;
		    return false;
	    }

	    public static bool TryGetItem(string name, out Item item)
	    {
		    return Items.TryGetValue(name, out item);
	    }

	    public static bool TryGetItem(short id, short meta, out Item item)
	    {
		    var entry = SecItemEntries.FirstOrDefault(x => x.Type == id);
		    if (entry == null)
		    {
			    item = null;
			    return false;
		    }

		    if (TryGetItem($"minecraft:{entry.TextType}", out item))
		    {
			    return true;
		    }

		    return false;
	    }
	    
	    public static bool IsItem(string name)
	    {
		    return ResourceManager.Registries.Items.Entries.ContainsKey(name);
	    }


	    public class ItemEntry
	    {
		    public int id { get; set; }
		    public string displayName { get; set; }
		    public string name { get; set; }
		    public int stackSize { get; set; }
	    }

	    private class SecondItemEntry
	    {
		    [JsonProperty("type")]
		    public long Type { get; set; }

		    [JsonProperty("meta")]
		    public long Meta { get; set; }

		    [JsonProperty("name")]
		    public string Name { get; set; }

		    [JsonProperty("text_type")]
		    public string TextType { get; set; }
	    }
    }
}
