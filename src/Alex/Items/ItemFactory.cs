using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Alex.API.Resources;
using Alex.API.Utils;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Graphics.Models.Blocks;
using Alex.Graphics.Models.Items;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Items;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using NLog;
using ItemMaterial = MiNET.Items.ItemMaterial;

namespace Alex.Items
{
    public static class ItemFactory
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ItemFactory));

		private static ResourceManager ResourceManager { get; set; }
		//private static McResourcePack ResourcePack { get; set; }
		private static IReadOnlyDictionary<ResourceLocation, Func<Item>> Items { get; set; }
		private static SecondItemEntry[] SecItemEntries { get; set; }
		private static ItemEntry[] ItemEntries { get; set; }
		
		private static ConcurrentDictionary<ResourceLocation, ItemModelRenderer> ItemRenderers { get; } = new ConcurrentDictionary<ResourceLocation, ItemModelRenderer>();

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
		
		private static Dictionary<string, DisplayElement> _defaultDisplayElements = new Dictionary<string, DisplayElement>()
		{
			{"gui", new DisplayElement(new Vector3(30, 225, 0), new Vector3(0,0,0), new Vector3(0.625f, 0.625f, 0.625f))},
			{"ground", new DisplayElement(new Vector3(0, 0, 0), new Vector3(0,3,0), new Vector3(0.25f, 0.25f, 0.25f))},
			{"fixed", new DisplayElement(new Vector3(0, 0, 0), new Vector3(0,0,0), new Vector3(0.5f, 0.5f, 0.5f))},
			{"thirdperson_righthand", new DisplayElement(new Vector3(75, 45, 0), new Vector3(0,2.5f,0), new Vector3(0.375f, 0.375f, 0.375f))},
			{"firstperson_righthand", new DisplayElement(new Vector3(0, 45, 0), new Vector3(0,0,0), new Vector3(0.4f, 0.4f, 0.4f))},
			{"firstperson_lefthand", new DisplayElement(new Vector3(0, 225, 0), new Vector3(0,0,0), new Vector3(0.4f, 0.4f, 0.4f))}
		};
		
	    public static void Init(IRegistryManager registryManager, ResourceManager resources, IProgressReceiver progressReceiver = null)
	    {
		    ResourceManager = resources;
		   // ResourcePack = resourcePack;

		    var otherRaw = ResourceManager.ReadStringResource("Alex.Resources.items3.json");
		    SecItemEntries = JsonConvert.DeserializeObject<SecondItemEntry[]>(otherRaw);
		    
		    var raw = ResourceManager.ReadStringResource("Alex.Resources.items2.json");
		    
		    ItemEntries = JsonConvert.DeserializeObject<ItemEntry[]>(raw);


		    var ii = resources.Registries.Items.Entries;
		    var blocks = resources.Registries.Blocks.Entries;
		    
		   // LoadModels();
		    
            ConcurrentDictionary<ResourceLocation, Func<Item>> items = new ConcurrentDictionary<ResourceLocation, Func<Item>>();
            
           // for(int i = 0; i < blocks.Count; i++)
           // List<ResourceLocation> addedCurrently = n
           int done = 0;
           Parallel.ForEach(
	           blocks, e =>
	           {
		           try
		           {
			           var entry = e;
			           progressReceiver?.UpdateProgress(done, blocks.Count, $"Processing block items...", entry.Key);

			           Item item;
			           /*if (blockRegistry.TryGet(entry.Key, out var blockState))
			          {
				           item = new ItemBlock(blockState.Value);
	                   }*/
			           var bs = BlockFactory.GetBlockState(entry.Key);

			           if (!(bs.Block is Air) && bs != null)
			           {
				           item = new ItemBlock(bs);
				           //  Log.Info($"Registered block item: {entry.Key}");
			           }
			           else
			           {
				           return;
			           }

			           /*var minetItem = MiNET.Items.ItemFactory.GetItem(entry.Key.Replace("minecraft:", ""));

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
			           }*/

			           item.Name = entry.Key;
			           item.DisplayName = entry.Key;

			           var data = ItemEntries.FirstOrDefault(
				           x => x.name.Equals(entry.Key.Substring(10), StringComparison.OrdinalIgnoreCase));

			           if (data != null)
			           {
				           item.MaxStackSize = data.stackSize;
				           item.DisplayName = data.displayName;
			           }

			          
				          string ns   = ResourceLocation.DefaultNamespace;
				          string path = entry.Key;

				          if (entry.Key.Contains(':'))
				          {
					          var index = entry.Key.IndexOf(':');
					          ns = entry.Key.Substring(0, index);
					          path = entry.Key.Substring(index + 1);
				          }

				          
				         var resourceLocation = new ResourceLocation(ns, $"block/{path}");

				          ResourcePackModelBase model            = null;

			           if (!ResourceManager.TryGetBlockModel(resourceLocation, out model))
			           {
				           /*foreach (var it in ResourcePack.ItemModels)
				           {
					           if (it.Key.Path.Equals(key.Path, StringComparison.OrdinalIgnoreCase))
					           {
						           model = it.Value;

						           break;
					           }
				           }*/
			           }

			           if (model == null)
			           {
				           Log.Debug($"Missing item render definition for block {entry.Key}, using default.");
				         //  model = new ResourcePackItem() {Display = _defaultDisplayElements};
			           }
			           else
			           {
				           
				           item.Renderer = new ItemBlockModelRenderer(bs, model, resources.Atlas.GetAtlas());
				           //item.Renderer.Cache(resources);


				           if (!items.TryAdd(entry.Key, () => { return item.Clone(); }))
				           {
					          // items[entry.Key] = () => { return item.Clone(); };
				           }
			           }
		           }
		           finally
		           {
			           done++;
		           }
	           });

           int i = 0;

           Parallel.ForEach(
	           ii, (entry) =>
	           {
		           // var entry = ii.ElementAt(i);
		           progressReceiver?.UpdateProgress(i++, ii.Count, $"Processing items...", entry.Key);
		           var  resourceLocation = new ResourceLocation(entry.Key);
		           resourceLocation = new ResourceLocation(resourceLocation.Namespace, $"item/{resourceLocation.Path}");

		           if (items.ContainsKey(resourceLocation))
			           return;
		           
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

		           var minetItem = MiNET.Items.ItemFactory.GetItem(resourceLocation.Path);

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

		           var data = ItemEntries.FirstOrDefault(
			           x => x.name.Equals(resourceLocation.Path, StringComparison.OrdinalIgnoreCase));

		           if (data != null)
		           {
			           item.MaxStackSize = data.stackSize;
			           item.DisplayName = data.displayName;
		           }
		           
		           ItemModelRenderer renderer;
		           if (!ItemRenderers.TryGetValue(resourceLocation, out renderer))
		           {
			           if (ResourceManager.TryGetItemModel(resourceLocation, out var model))
			           {
				           renderer = new ItemModelRenderer(model);
				           //renderer.Cache(ResourceManager);

				           ItemRenderers.TryAdd(resourceLocation, renderer);
			           }

			           if (renderer == null)
			           {
				           var r = ItemRenderers.FirstOrDefault(
					           x => x.Key.Path.Equals(resourceLocation.Path, StringComparison.OrdinalIgnoreCase));

				           if (r.Value != null)
					           renderer = r.Value;
			           }

			           //  if (ResourcePack.ItemModels.TryGetValue(resourceLocation, out var itemModel)) { }
		           }

		           if (renderer != null)
					item.Renderer = renderer;

		           if (item.Renderer == null)
		           {
			           Log.Warn($"Could not find item model renderer for: {resourceLocation}");
		           }

		           if (!items.TryAdd(resourceLocation, () => { return item.Clone(); }))
		           {
			           //var oldItem = items[resourceLocation];
			         //  items[resourceLocation] = () => { return item.Clone(); };
		           }
	           });

			Items = new ReadOnlyDictionary<ResourceLocation, Func<Item>>(items);
	    }

	    /*private static void LoadModels()
	    {
		    void processItem(KeyValuePair<string, ResourcePackModelBase> model)
		    {
			    if (model.Value == null || model.Value.Textures == null || model.Value.Textures.Count == 0)
				    return;

			    ItemRenderers.AddOrUpdate(
				    model.Key, (a) =>
				    {
					    var render = new ItemModelRenderer(model.Value);
					    render.Cache(ResourceManager);

					    return render;
				    }, (s, renderer) =>
				    {
					    var render = new ItemModelRenderer(model.Value);
					    render.Cache(ResourceManager);

					    return render;
				    });
		    }

		    if (ResourceManager.Asynchronous)
		    {
			    Parallel.ForEach(ResourceManager.ItemModels, processItem);
		    }
		    else
		    {
			    foreach (var item in ResourceManager.ItemModels)
			    {
				    processItem(item);
			    }
		    }
	    }*/

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

	    public static bool TryGetItem(ResourceLocation name, out Item item)
	    {
		    if (Items.TryGetValue(name, out var gen))
		    {
			    item = gen();
			    return true;
		    }

		    var a = Items.Where(x => x.Key.Path.Length >= name.Path.Length)
			   .OrderBy(x => name.ToString().Length - x.Key.ToString().Length).FirstOrDefault(
				    x => x.Key.Path.EndsWith(name.Path, StringComparison.OrdinalIgnoreCase));

		    if (a.Value != null)
		    {
			    item = a.Value();

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
