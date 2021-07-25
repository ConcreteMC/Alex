using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Common.Items;
using Alex.Common.Resources;
using Alex.Common.Utils;
using Alex.Entities;
using Alex.Entities.BlockEntities;
using Alex.Gamestates;
using Alex.Graphics.Models.Blocks;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Entity.BlockEntities;
using Alex.Graphics.Models.Items;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
		//private static SecondItemEntry[] SecItemEntries { get; set; }

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
					item.Material = Common.Items.ItemMaterial.None;
					break;
				case ItemMaterial.Wood:
					item.Material = Common.Items.ItemMaterial.Wood;
					break;
				case ItemMaterial.Stone:
					item.Material = Common.Items.ItemMaterial.Stone;
					break;
				case ItemMaterial.Gold:
					item.Material = Common.Items.ItemMaterial.Gold;
					break;
				case ItemMaterial.Iron:
					item.Material = Common.Items.ItemMaterial.Iron;
					break;
				case ItemMaterial.Diamond:
					item.Material = Common.Items.ItemMaterial.Diamond;
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

		private static IReadOnlyDictionary<ResourceLocation, ItemMapping> _itemMappings;// = new Dictionary<string, ItemMapping>();
		private static IReadOnlyDictionary<ReverseMapperKey, ResourceLocation> _reverseItemMappings;
		private static HashSet<ItemMapping> _allItemMapping = new HashSet<ItemMapping>();
		
		private static string GetDisplayName(ResourceLocation location)
		{
			var key = $"item.{location.Namespace}.{location.Path}";
			var itemTranslation = Alex.Instance.GuiRenderer.GetTranslation(key);
			if (itemTranslation == key)
				itemTranslation = Alex.Instance.GuiRenderer.GetTranslation($"block.{location.Namespace}.{location.Path}");

			return itemTranslation;
		}

		private static bool TryGetRenderer(string itemName, ResourceManager resources, ResourceLocation resourceLocation, out IItemRenderer renderer)
		{
			var modelRegistry = resources.BlockModelRegistry;
			if (modelRegistry.TryGet(
				resourceLocation,
				out var modelEntry))
			{
				if (modelEntry.Value.Type == ModelType.Item)
				{
					renderer = new ItemModelRenderer(modelEntry.Value);

					return true;
				}

				if (modelEntry.Value.Type == ModelType.Block)
				{
					var bs = BlockFactory.GetBlockState(itemName);

					renderer = new ItemBlockModelRenderer(
						bs, modelEntry.Value, resources.BlockAtlas.GetAtlas());

					return true;
				}

				if (modelEntry.Value.Type == ModelType.Entity)
				{
					EntityModelRenderer modelRenderer = null;
					Texture2D modelTexture = null;
					
					switch (itemName)
					{
						case "minecraft:chest":
							if (EntityModelRenderer.TryGetRenderer(new ChestEntityModel(), out modelRenderer))
							{
								modelTexture = BlockEntityFactory.ChestTexture;
							}
							break;
						case "minecraft:ender_chest":
							if (EntityModelRenderer.TryGetRenderer(new ChestEntityModel(), out modelRenderer))
							{
								modelTexture = BlockEntityFactory.EnderChestTexture;
							}
							break;
						case "minecraft:player_head":
						case "minecraft:skull":
							if (EntityModelRenderer.TryGetRenderer(new SkullBlockEntityModel(), out modelRenderer))
							{
								modelTexture = BlockEntityFactory.SkullTexture;
							}
							break;
					}

					if (modelRenderer == null || modelTexture == null)
					{
						if (resources.TryGetEntityDefinition(itemName, out var description, out var resourcePack))
						{
							if (modelRenderer == null)
								modelRenderer = EntityFactory.GetEntityRenderer(description.Identifier);

							if (modelRenderer != null)
							{
								if (description.Geometry.TryGetValue("default", out var defaultGeometry)
								    && ModelFactory.TryGetModel(defaultGeometry, out var model) && model != null)
								{
									var textures = description.Textures;
									string texture;

									if (!(textures.TryGetValue("default", out texture) || textures.TryGetValue(
										description.Identifier, out texture)))
									{
										texture = textures.FirstOrDefault().Value;
									}

									if (resources.TryGetBedrockBitmap(texture, out var bmp))
									{
										modelTexture = TextureUtils.BitmapToTexture2D(
											Alex.Instance.GraphicsDevice, bmp);
									}
									else if (resources.TryGetBitmap(texture, out var bmp2))
									{
										modelTexture = TextureUtils.BitmapToTexture2D(
											Alex.Instance.GraphicsDevice, bmp2);
									}
								}
							}
						}
					}

					if (modelRenderer != null && modelTexture != null)
					{
						renderer = new EntityItemRenderer(itemName, modelRenderer, modelTexture);
						return true;
					}
					else
					{
						modelRenderer?.Dispose();
						modelTexture?.Dispose();
					}
				}

				if (modelEntry.Value.Textures.Count > 0)
				{
					renderer = new ItemModelRenderer(modelEntry.Value);
					return true;
				}

				Log.Debug($"Unsupported model for item. ModelType={modelEntry.Value.Type} Item={resourceLocation}");
			}

			renderer = null;
			return false;
		}
		
		private static LegacyIdMap _legacyIdMap;
	    public static void Init(IRegistryManager registryManager, ResourceManager resources, IProgressReceiver progressReceiver = null)
	    {
		    _allItemMapping.Clear();
		    ResourceManager = resources;
		    var modelRegistry = resources.BlockModelRegistry;
		   // ResourcePack = resourcePack;

		   _itemMappings =
			   MCJsonConvert.DeserializeObject<IReadOnlyDictionary<ResourceLocation, ItemMapping>>(ResourceManager.ReadStringResource("Alex.Resources.itemmapping.json"));

		   
		   Dictionary<ReverseMapperKey, ResourceLocation> reverseMap = new Dictionary<ReverseMapperKey, ResourceLocation>();

		   if (_itemMappings != null)
		   {
			   foreach (var item in _itemMappings)
			   {
				   var value = item.Value;
				   value.JavaId = item.Key;
				   
				   reverseMap.Add(new ReverseMapperKey(item.Value.BedrockId, item.Value.BedrockData), item.Key);

				   _allItemMapping.Add(value);
			   }
		   }

		   _reverseItemMappings = new ReadOnlyDictionary<ReverseMapperKey, ResourceLocation>(reverseMap);
		  
		   var otherRaw = ResourceManager.ReadStringResource("Alex.Resources.items3.json");
		    var legacyIdMapping = JsonConvert.DeserializeObject<LegacyIdMap>(otherRaw);
		    _legacyIdMap = legacyIdMapping;
		    
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
		            IItemRenderer renderer = null;

		            if (!TryGetRenderer(entry.Key, resources, new ResourceLocation(resourceLocation.Namespace, $"item/{resourceLocation.Path}"), out renderer))
		            {
						Log.Debug($"No model found for item: {entry.Key}");
						return;
		            }

		            item.DisplayName = GetDisplayName(resourceLocation);

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

			           IItemRenderer renderer = null;
			           if (!TryGetRenderer(entry.Key, resources, new ResourceLocation(resourceLocation.Namespace, $"block/{resourceLocation.Path}"), out renderer))
			           {
				           ResourcePackModelBase model            = null;

				           if (modelRegistry.TryGet(new ResourceLocation(resourceLocation.Namespace, $"block/{resourceLocation.Path}"), out var modelEntry))
				           {
					           model = modelEntry.Value;
					           renderer = new ItemBlockModelRenderer(bs, model, resources.BlockAtlas.GetAtlas());
				           }
			           }

			           if (renderer == null)
			           {
				           Log.Debug($"Missing item render definition for block {entry.Key}, using default.");
			           }
			           else
			           {
				           var item = new ItemBlock(bs) { };
				           item.Name = entry.Key;
				           item.DisplayName = GetDisplayName(resourceLocation);
				          // item.DisplayName = Alex.Instance.GuiRenderer.GetTranslation($"block.{resourceLocation.Namespace}.{resourceLocation.Path}");

				          item.Renderer = renderer;
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

           if (items.TryGetValue("minecraft:player_head", out var func))
	           items.TryAdd("minecraft:skull", func);
           
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
		    if (_legacyIdMap.Items.TryGetValue($"{id}:{meta}", out var value))
		    {
			    if (TryGetItem(value, out item))
			    {
				    return true;
			    } 
		    }

		    item = null;
		    return false;
	    }

	    public static bool TryGetBedrockItem(int id, int meta, out Item item)
	    {
		    item = null;

		    if (_reverseItemMappings.TryGetValue(new ReverseMapperKey(id, meta), out var itemName))
		    {
			    if (TryGetItem(itemName, out item))
				    return true;
		    }
		    else if (_allItemMapping.TryGetValue(
			    new ItemMapping() {BedrockData = id, BlockRuntimeId = meta}, out var actualValue))
		    {
			    return TryGetItem(actualValue.JavaId, out item);
		    }
		    
		    return false;
	    }

	    private class ReverseMapperKey
	    {
		    private readonly int _id;
		    private readonly int _meta;

		    public ReverseMapperKey(int id, int meta)
		    {
			    _id = id;
			    _meta = meta;
		    }

		    /// <inheritdoc />
		    public override int GetHashCode()
		    {
			    return HashCode.Combine(_id, _meta);
		    }
	    }

	    private class LegacyIdMap
	    {
		    [JsonProperty("blocks")]
			public IReadOnlyDictionary<string, string> Blocks { get; set; }
			
			[JsonProperty("items")]
			public IReadOnlyDictionary<string, string> Items { get; set; }
	    }
    }
}
