using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Common.Resources;
using Alex.Common.Utils;
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

		//private static ConcurrentDictionary<ResourceLocation, ItemModelRenderer> ItemRenderers { get; } = new ConcurrentDictionary<ResourceLocation, ItemModelRenderer>();

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
					item.Material = Common.Utils.ItemMaterial.None;
					break;
				case ItemMaterial.Wood:
					item.Material = Common.Utils.ItemMaterial.Wood;
					break;
				case ItemMaterial.Stone:
					item.Material = Common.Utils.ItemMaterial.Stone;
					break;
				case ItemMaterial.Gold:
					item.Material = Common.Utils.ItemMaterial.Gold;
					break;
				case ItemMaterial.Iron:
					item.Material = Common.Utils.ItemMaterial.Iron;
					break;
				case ItemMaterial.Diamond:
					item.Material = Common.Utils.ItemMaterial.Diamond;
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

		private static Dictionary<string, ItemMapping> _itemMappings;// = new Dictionary<string, ItemMapping>();
	    public static void Init(IRegistryManager registryManager, ResourceManager resources, IProgressReceiver progressReceiver = null)
	    {
		    ResourceManager = resources;
		    var modelRegistry = resources.BlockModelRegistry;
		   // ResourcePack = resourcePack;

		   _itemMappings =
			   JsonConvert.DeserializeObject<Dictionary<string, ItemMapping>>(ResourceManager.ReadStringResource("Alex.Resources.itemmapping.json"));

		  
		    var otherRaw = ResourceManager.ReadStringResource("Alex.Resources.items3.json");
		    SecItemEntries = JsonConvert.DeserializeObject<SecondItemEntry[]>(otherRaw);

		    var blocks = resources.Registries.Blocks.Entries;
		    
            ConcurrentDictionary<ResourceLocation, Func<Item>> items = new ConcurrentDictionary<ResourceLocation, Func<Item>>();
            
            int i = 0;

            Parallel.ForEach(
	            resources.Registries.Items.Entries, (entry) =>
	            {
		            progressReceiver?.UpdateProgress(i++,  resources.Registries.Items.Entries.Count, $"Processing items...", entry.Key);
		        
		            var resourceLocation = new ResourceLocation(entry.Key);

		            if (items.ContainsKey(resourceLocation))
			            return;
		           
		            Item item = new Item();

		            var minetItem = MiNET.Items.ItemFactory.GetItem(resourceLocation.Path);

		            if (minetItem != null)
		            {
			            if (Enum.TryParse<ItemType>(minetItem.ItemType.ToString(), out ItemType t))
			            {
				            item.ItemType = t;
			            }

			            SetItemMaterial(item, minetItem.ItemMaterial);

			            item.Meta = minetItem.Metadata;
			            item.Id = minetItem.Id;
		            }

		            item.Name = entry.Key;
		            item.DisplayName = entry.Key;

		            var first = SecItemEntries.FirstOrDefault(x => x.TextType.Equals(resourceLocation.Path));
		            if (first != null)
		            {
			            item.DisplayName = first.Name;
		            }
		           
		            IItemRenderer renderer = null;

		            if (modelRegistry.TryGet(
			            new ResourceLocation(resourceLocation.Namespace, $"item/{resourceLocation.Path}"),
			            out var modelEntry))
		            {
			            if (modelEntry.Value.Type == ModelType.Item)
			            {
				            renderer = new ItemModelRenderer(modelEntry.Value);
			            }
			            else if (modelEntry.Value.Type == ModelType.Block)
			            {
				            var bs = BlockFactory.GetBlockState(entry.Key);
				            renderer = new ItemBlockModelRenderer(bs, modelEntry.Value, resources.BlockAtlas.GetAtlas());
			            }
		            }

		            if (renderer != null)
			            item.Renderer = renderer;

		            if (item.Renderer == null)
		            {
			            Log.Warn($"Could not find item model renderer for: {resourceLocation}");
		            }

		            items.TryAdd(resourceLocation, () => { return item.Clone(); });
	            });

            
           int done = 0;
           Parallel.ForEach(
	           blocks, e =>
	           {
		           try
		           {
			           var entry = e;
			           progressReceiver?.UpdateProgress(done, blocks.Count, $"Processing block items...", entry.Key);
			           
			           var resourceLocation = new ResourceLocation(entry.Key);
			           if (items.ContainsKey(resourceLocation))
				           return;
			           
			           var bs = BlockFactory.GetBlockState(entry.Key);

			           if (!bs.Block.Renderable)
			           {
				           return;
			           }
			           
			           ResourcePackModelBase model            = null;

			           if (modelRegistry.TryGet(new ResourceLocation(resourceLocation.Namespace, $"block/{resourceLocation.Path}"), out var modelEntry))
			           {
				           model = modelEntry.Value;
			           }

			           if (model == null)
			           {
				           Log.Debug($"Missing item render definition for block {entry.Key}, using default.");
			           }
			           else
			           {
				           var item = new ItemBlock(bs) { };
				           item.Name = entry.Key;
				           item.DisplayName = entry.Key;
				           
				           var first = SecItemEntries.FirstOrDefault(x => x.TextType.Equals(resourceLocation.Path));
				           if (first != null)
				           {
					           item.DisplayName = first.Name;
				           }

				           item.Renderer = new ItemBlockModelRenderer(bs, model, resources.BlockAtlas.GetAtlas());
				           item.Renderer.Cache(resources);

				           items.TryAdd(
					           resourceLocation, () =>
					           {
						           return item.Clone();
					           });
			           }
		           }
		           finally
		           {
			           done++;
		           }
	           });
           
			Items = new ReadOnlyDictionary<ResourceLocation, Func<Item>>(items);
	    }

	    public static bool ResolveItemName(int protocolId, out ResourceLocation res)
	    {
		    var result = ResourceManager.Registries.Items.Entries.FirstOrDefault(x => x.Value.ProtocolId == protocolId).Key;
		    if (result != null)
		    {
			    res = new ResourceLocation(result);
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

		    item = default;
		    return false;
	    }

	    public static bool TryGetItem(short id, short meta, out Item item)
	    {
		    SecondItemEntry entry = SecItemEntries.FirstOrDefault(x => x.Type == id && x.Meta == meta);
		    if (entry == null)
		    {
			    entry = SecItemEntries.FirstOrDefault(x => x.Type == id);
		    }

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
