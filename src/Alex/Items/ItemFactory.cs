using System;
using System.Collections;
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
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Utils;
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
			get;
			private set;
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

		private static Dictionary<ResourceLocation, ItemMapping> _itemMappings;// = new Dictionary<string, ItemMapping>();
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
				if ((modelEntry.Value.Type & ModelType.Item) != 0)
				{
					renderer = new ItemModelRenderer(modelEntry.Value);

					return true;
				}

				if ((modelEntry.Value.Type & ModelType.Block) != 0)
				{
					var bs = BlockFactory.GetBlockState(itemName);

					renderer = new ItemBlockModelRenderer(
						bs, modelEntry.Value, resources.BlockAtlas.GetAtlas());

					return true;
				}

				if ((modelEntry.Value.Type & ModelType.Entity) != 0)
				{
					ModelRenderer modelRenderer = null;
					Texture2D modelTexture = null;
					
					switch (itemName)
					{
						case "minecraft:chest":
							if (new ChestEntityModel().TryGetRenderer(out modelRenderer))
							{
								modelTexture = BlockEntityFactory.ChestTexture;
							}
							break;
						case "minecraft:ender_chest":
							if (new ChestEntityModel().TryGetRenderer(out modelRenderer))
							{
								modelTexture = BlockEntityFactory.EnderChestTexture;
							}
							break;
						case "minecraft:player_head":
						case "minecraft:skull":
							if (new SkullBlockEntityModel().TryGetRenderer(out modelRenderer))
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

				//	if (modelRenderer != null && modelTexture != null)
					{
						renderer = new EntityItemRenderer(modelRenderer, modelTexture);
						return true;
					}
				//	else
					{
					//	modelRenderer?.Dispose();
					//	modelTexture?.Dispose();
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
			   MCJsonConvert.DeserializeObject<Dictionary<ResourceLocation, ItemMapping>>(ResourceManager.ReadStringResource("Alex.Resources.itemmapping.json"));

		   
		   Dictionary<ReverseMapperKey, ResourceLocation> reverseMap = new Dictionary<ReverseMapperKey, ResourceLocation>();
		   var r16Mapping =  MCJsonConvert.DeserializeObject<R16ToCurrentMap>(ResourceManager.ReadStringResource("Alex.Items.Resources.r16_to_current_item_map.json"));

		   foreach (var entry in r16Mapping.Complex)
		   {
			   var legacyId = entry.Key;

			   foreach (var meta in entry.Value)
			   {
				   var newBedrockId = meta.Value;

				   var matchingMapping = _itemMappings.FirstOrDefault(x => x.Value.BedrockId == newBedrockId);

				   if (matchingMapping.Value != null)
				   {
					   if (int.TryParse(meta.Key, out var metadata))
					   {
						   var value = new ItemMapping()
						   {
							   BedrockId = legacyId, BedrockData = metadata, JavaId = matchingMapping.Key
						   };

						   reverseMap.Add(new ReverseMapperKey(legacyId, metadata), matchingMapping.Key);

						   _allItemMapping.Add(value);
					   }
				   }
			   }
		   }

		   /*
		   foreach (var simple in r16Mapping.Simple)
		   {
			   
		   }*/
		   
		    if (_itemMappings != null)
		   {
			   foreach (var item in _itemMappings)
			   {
				   var value = item.Value;
				   value.JavaId = item.Key;
				   
				   reverseMap.TryAdd(new ReverseMapperKey(value.BedrockId, value.BedrockData), item.Key);

				   _allItemMapping.Add(value);
			   }
		   }
		    
		   _reverseItemMappings = new ReadOnlyDictionary<ReverseMapperKey, ResourceLocation>(reverseMap);
		  
		   var otherRaw = ResourceManager.ReadStringResource("Alex.Resources.legacyItemMapping.json");
		    var legacyIdMapping = JsonConvert.DeserializeObject<LegacyIdMap>(otherRaw);
		    _legacyIdMap = legacyIdMapping;
		    
		    var blocks = resources.Registries.Blocks.Entries;
		    
            ConcurrentDictionary<ResourceLocation, Func<Item>> items = new ConcurrentDictionary<ResourceLocation, Func<Item>>();
            
            List<Item> allItems = new List<Item>();
            void HandleEntry(KeyValuePair<string, Registries.RegistryEntry> entry, bool isBlock)
            {
	            var resourceLocation = new ResourceLocation(entry.Key);

	            if (items.ContainsKey(resourceLocation))
		            return;

	            IItemRenderer renderer = null;
	            Item item;

	            ResourceLocation rendererResourceLocation;
	            

	            if (isBlock)
	            {
		            rendererResourceLocation = new ResourceLocation(
			            resourceLocation.Namespace, $"block/{resourceLocation.Path}");
		            
		            var bs = BlockFactory.GetBlockState(entry.Key);
		            item = new ItemBlock(bs);
	            }
	            else
	            {
		            rendererResourceLocation = new ResourceLocation(
			            resourceLocation.Namespace, $"item/{resourceLocation.Path}");
		            
		            item = new Item();
	            }
	            
	            if (!TryGetRenderer(entry.Key, resources, rendererResourceLocation, out renderer))
	            {
		            if (isBlock && item is ItemBlock itemBlock)
		            {
			            if (modelRegistry.TryGet(rendererResourceLocation, out var modelEntry))
			            {
				            ResourcePackModelBase model = modelEntry.Value;
				            renderer = new ItemBlockModelRenderer(itemBlock.Block, model, resources.BlockAtlas.GetAtlas(true));
			            }
		            }
	            }

	            if (renderer == default)
	            {
		         //   Log.Warn($"No renderer for item: {resourceLocation}");
		        //    return;
	            }

	            item.Renderer = renderer;

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

	            item.DisplayName = GetDisplayName(resourceLocation);

	            if (renderer != null)
	            {
		            renderer.Cache(resources);
		            item.Renderer = renderer;
	            }

	            if (item.Renderer == null)
	            {
		            Log.Warn($"Could not find model renderer for: {resourceLocation}");
	            }

	            if (items.TryAdd(resourceLocation, () => { return item.Clone(); }))
	            {
		            allItems.Add(item);
	            }
            }
            
            int i = 0;

            Parallel.ForEach(
	            resources.Registries.Items.Entries.Where(x => blocks.All(b => b.Key != x.Key)), (entry) =>
	            {
		            progressReceiver?.UpdateProgress(
			            i++, resources.Registries.Items.Entries.Count, $"Processing items...", entry.Key);
		            
		            HandleEntry(entry, false);
	            });

            
           int done = 0;
           Parallel.ForEach(
	           blocks, entry =>
	           {
		           progressReceiver?.UpdateProgress(done++, blocks.Count, $"Processing block items...", entry.Key);
		           HandleEntry(entry, true);
	           });

           if (items.TryGetValue("minecraft:player_head", out var func))
	           items.TryAdd("minecraft:skull", func);

           if (items.TryGetValue("minecraft:oak_door", out var oakDoorFunc))
	           items.TryAdd("minecraft:wooden_door", oakDoorFunc);
           
			Items = new ReadOnlyDictionary<ResourceLocation, Func<Item>>(items);
			AllItems = allItems.ToArray();
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

	    public static bool TryGetBedrockItem(string id, int meta, out Item item)
	    {
		    item = null;

		    if (_reverseItemMappings.TryGetValue(new ReverseMapperKey(id, meta), out var itemName))
		    {
			    if (TryGetItem(itemName, out item))
			    {
				    return true;
			    }
			    else
			    {
				    Log.Warn($"Unknown item: {itemName}");
			    }
		    }
		    else if (_allItemMapping.TryGetValue(
			    new ItemMapping() {BedrockId = id, BedrockData = meta}, out var actualValue))
		    {
			    return TryGetItem(actualValue.JavaId, out item);
		    }
		    
		    return false;
	    }

	    private class ReverseMapperKey : IEquatable<ReverseMapperKey>
	    {
		    private readonly string _id;
		    private readonly int _meta;

		    public ReverseMapperKey(string id, int meta)
		    {
			    _id = id.ToLowerInvariant();
			    _meta = meta;
		    }

		    /// <inheritdoc />
		    public override int GetHashCode()
		    {
			    return HashCode.Combine(_id, _meta);
		    }

		    /// <inheritdoc />
		    public bool Equals(ReverseMapperKey other)
		    {
			    if (ReferenceEquals(null, other)) return false;
			    if (ReferenceEquals(this, other)) return true;

			    return _id == other._id && _meta == other._meta;
		    }

		    /// <inheritdoc />
		    public override bool Equals(object obj)
		    {
			    if (ReferenceEquals(null, obj)) return false;
			    if (ReferenceEquals(this, obj)) return true;
			    if (obj.GetType() != this.GetType()) return false;

			    return Equals((ReverseMapperKey)obj);
		    }
	    }

	    private class LegacyIdMap
	    {
		    [JsonProperty("blocks")]
			public IReadOnlyDictionary<string, string> Blocks { get; set; }
			
			[JsonProperty("items")]
			public IReadOnlyDictionary<string, string> Items { get; set; }
	    }
	    
	    class R16ToCurrentMap
	    {
		    [JsonProperty("complex")]
		    public Dictionary<string, Dictionary<string, string>> Complex { get; set; }
			
		    [JsonProperty("simple")]
		    public Dictionary<string, string> Simple { get; set; }
	    }
    }
}
