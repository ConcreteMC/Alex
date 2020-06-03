using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Alex.API.Resources;
using Alex.API.Utils;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Graphics.Models.Items;
using Alex.ResourcePackLib;
using Newtonsoft.Json;
using NLog;
using ItemMaterial = MiNET.Items.ItemMaterial;

namespace Alex.Items
{
    public static class ItemFactory
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ItemFactory));

		private static ResourceManager ResourceManager { get; set; }
		private static McResourcePack ResourcePack { get; set; }
		private static IReadOnlyDictionary<string, Func<Item>> Items { get; set; }
		private static SecondItemEntry[] SecItemEntries { get; set; }
		private static ItemEntry[] ItemEntries { get; set; }
		
		private static ConcurrentDictionary<string, ItemModelRenderer> ItemRenderers { get; } = new ConcurrentDictionary<string, ItemModelRenderer>();

		public static Item[] AllItems
		{
			get
			{
				List<Item> items = new List<Item>();

				foreach (var item in Items.Values.ToArray())
				{
					items.Add(item());
				}
				
				return items.ToArray();
			}
		}

		private static void SetItemMaterial(Item item, ItemMaterial material)
		{
			switch (material)
			{
				case ItemMaterial.None:
					item.Material = API.Utils.ItemMaterial.None;
					break;
				case ItemMaterial.Wood:
					item.Material = API.Utils.ItemMaterial.Wood;
					break;
				case ItemMaterial.Stone:
					item.Material = API.Utils.ItemMaterial.Stone;
					break;
				case ItemMaterial.Gold:
					item.Material = API.Utils.ItemMaterial.Gold;
					break;
				case ItemMaterial.Iron:
					item.Material = API.Utils.ItemMaterial.Iron;
					break;
				case ItemMaterial.Diamond:
					item.Material = API.Utils.ItemMaterial.Diamond;
					break;
			}
		}
		
	    public static void Init(IRegistryManager registryManager, ResourceManager resources, McResourcePack resourcePack, IProgressReceiver progressReceiver = null)
	    {
		      var blockRegistry = registryManager.GetRegistry<Block>();
		    
		    ResourceManager = resources;
		    ResourcePack = resourcePack;

		    var otherRaw = ResourceManager.ReadStringResource("Alex.Resources.items3.json");
		    SecItemEntries = JsonConvert.DeserializeObject<SecondItemEntry[]>(otherRaw);
		    
		    var raw = ResourceManager.ReadStringResource("Alex.Resources.items2.json");
		    
		    ItemEntries = JsonConvert.DeserializeObject<ItemEntry[]>(raw);


		    var ii = resources.Registries.Items.Entries;
		    var blocks = resources.Registries.Blocks.Entries;
		    
		    LoadModels();
		    
            Dictionary<string, Func<Item>> items = new Dictionary<string, Func<Item>>();
            
            for(int i = 0; i < blocks.Count; i++)
		    {
			    var entry = blocks.ElementAt(i);
                progressReceiver?.UpdateProgress(i * (100 / blocks.Count), $"Processing block items...", entry.Key);
                
			    Item item;
			    /*if (blockRegistry.TryGet(entry.Key, out var blockState))
			   {
				    item = new ItemBlock(blockState.Value);
                }*/
			    var bs = BlockFactory.GetBlockState(entry.Key);
			    if (!(bs.Block is Air))
			    {
				    item = new ItemBlock(bs);
				  //  Log.Info($"Registered block item: {entry.Key}");
			    }
			    else
			    {
				    continue;
			    }

			    var minetItem = MiNET.Items.ItemFactory.GetItem(entry.Key.Replace("minecraft:", ""));
			    if (minetItem != null)
			    {
				    if (Enum.TryParse<ItemType>(minetItem.ItemType.ToString(), out ItemType t))
				    {
					    item.ItemType = t;
				    }

				    SetItemMaterial(item, minetItem.ItemMaterial);
				   // item.Material = minetItem.ItemMaterial;
				    
				    item.Meta = minetItem.Metadata;
				    item.Id = minetItem.Id;
			    }
			    
			    item.Name = entry.Key;
                item.DisplayName = entry.Key;

			    var data = ItemEntries.FirstOrDefault(x =>
				    x.name.Equals(entry.Key.Substring(10), StringComparison.InvariantCultureIgnoreCase));
			    if (data != null)
			    {
				    item.MaxStackSize = data.stackSize;
				    item.DisplayName = data.displayName;
			    }


			    var rpItem = ResourcePack.ItemModels.FirstOrDefault(x =>
				    x.Key.Equals(entry.Key.Replace("minecraft:", "minecraft:item/"), StringComparison.InvariantCultureIgnoreCase)).Value;

			    if (rpItem == null)
			    {
				    foreach (var it in ResourcePack.ItemModels)
				    {
					    if (it.Key.Contains(entry.Key.Replace("minecraft:", "minecraft:item/"),
						    StringComparison.InvariantCultureIgnoreCase))
					    {
						    rpItem = it.Value;
						    break;
					    }
				    }
			    }

			    if (rpItem != null)
			    {
				    item.Renderer = new ItemBlockModelRenderer(bs, rpItem, resourcePack, resources);
				    item.Renderer.Cache(resourcePack);
			    }
			    
			    items.TryAdd(entry.Key, () =>
			    {
				    return item.Clone();
			    });
		    }
            
		    for(int i = 0; i < ii.Count; i++)
		    {
			    var entry = ii.ElementAt(i);
                progressReceiver?.UpdateProgress(i * (100 / ii.Count), $"Processing items...", entry.Key);
                
			    Item item;
			    /*if (blockRegistry.TryGet(entry.Key, out var blockState))
			   {
				    item = new ItemBlock(blockState.Value);
                }*/
			 /*   if (blocks.ContainsKey(entry.Key) && blockRegistry.TryGet(entry.Key, out var registryEntry))
			    {
				    item = new ItemBlock(registryEntry.Value);
			    }
			    else
			    {*/
				    item = new Item();
			   // }

			    var minetItem = MiNET.Items.ItemFactory.GetItem(entry.Key.Replace("minecraft:", ""));
			    if (minetItem != null)
			    {
				    if (Enum.TryParse<ItemType>(minetItem.ItemType.ToString(), out ItemType t))
				    {
					    item.ItemType = t;
				    }
				    
				    SetItemMaterial(item, minetItem.ItemMaterial);
				    
				   // item.Material = minetItem.ItemMaterial;
				    item.Meta = minetItem.Metadata;
				    item.Id = minetItem.Id;
			    }
			    
			    item.Name = entry.Key;
                item.DisplayName = entry.Key;

			    var data = ItemEntries.FirstOrDefault(x =>
				    x.name.Equals(entry.Key.Substring(10), StringComparison.InvariantCultureIgnoreCase));
			    if (data != null)
			    {
				    item.MaxStackSize = data.stackSize;
				    item.DisplayName = data.displayName;
			    }

			    
			    foreach (var it in ResourcePack.ItemModels)
			    {
				    if (it.Key.Contains(entry.Key.Replace("minecraft:", ""),
					    StringComparison.InvariantCultureIgnoreCase))
				    {
					    //Log.Info($"Model found: {entry.Key} = {it.Key}");
					    ItemModelRenderer renderer;
					    if (ItemRenderers.TryGetValue(it.Key, out renderer))
					    {

					    }
					    else if (ItemRenderers.TryGetValue(entry.Key, out renderer))

					    {

					    }

					    if (renderer != null)
					    {
						    //Log.Debug($"Found renderer for {entry.Key}, textures: {it.Value.Textures.Count}");
						    item.Renderer = renderer;
						    break;
					    }
				    }
			    }

			   /* if (ResourcePack.ItemModels.TryGetValue(entry.Key.Replace("minecraft:", "minecraft:item/"), out ResourcePackItem iii))
			    {
				    ItemModelRenderer renderer;
				    if (ItemRenderers.TryGetValue(entry.Key, out renderer))
				    {

				    }
				    else if (ItemRenderers.TryGetValue(entry.Key, out renderer))

				    {

				    }

				    if (renderer != null)
				    {
					    Log.Info($"Found renderer for {entry.Key}, textures: {iii.Textures.Count}");
				    }

				    item.Renderer = renderer;
			    }*/

			 //   Log.Info($"Loaded item: {entry.Key} (Renderer: {item.Renderer != null})");
			    items.TryAdd(entry.Key, () => { return item.Clone(); });
		    }

			Items = new ReadOnlyDictionary<string, Func<Item>>(items);
	    }

	    private static void LoadModels()
	    {
		    foreach (var model in ResourcePack.ItemModels)
		    {
			    if (model.Value == null || model.Value.Textures == null || model.Value.Textures.Count == 0)
				    continue;
			    
			    var renderer = ItemRenderers.AddOrUpdate(model.Key,
				    (a) =>
				    {
					    var render = new ItemModelRenderer(model.Value, ResourcePack);
					    render.Cache(ResourcePack);
					    return render;
				    },
				    (s, renderer) =>
				    {
					    var render = new ItemModelRenderer(model.Value, ResourcePack);
					    render.Cache(ResourcePack);
					    
					    return render;
				    });
			    
		    }
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
		    if (Items.TryGetValue(name, out var gen))
		    {
			    item = gen();
			    return true;
		    }

		    item = default;
		    return false;
	    }

	    public static bool TryGetItem(short id, short meta, out Item item)
	    {
		    /*var minetItem = MiNET.Items.ItemFactory.GetItem(id, meta);
		    if (minetItem != null)
		    {
			    if (TryGetItem($"minecraft:{minetItem.}"))
		    }*/

		    var reverseResult = MiNET.Items.ItemFactory.NameToId.FirstOrDefault(x => x.Value == id);
		    if (!string.IsNullOrWhiteSpace(reverseResult.Key))
		    {
			    if (TryGetItem($"minecraft:{reverseResult.Key}", out item))
			    {
				    return true;
			    }
		    }

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
