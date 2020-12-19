using System;
using MiNET.Crafting;
using MiNET.Items;
using MiNET.Net;
using MiNET.Utils;
using NLog;

namespace Alex.Net.Bedrock.Packets
{
	public class CraftingData : McpeCraftingData
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
		
		protected override void DecodePacket()
		{
			//base.DecodePacket();
			Id = IsMcpe ? (byte) ReadVarInt() : ReadByte();
			
			ReadNewRecipes();
			potionTypeRecipes = ReadPotionTypeRecipes();
			potionContainerRecipes = ReadPotionContainerChangeRecipes();
			isClean = ReadBool();
		}
		
		const byte Shapeless          = 0;
		const byte Shaped             = 1;
		const byte Furnace            = 2;
		const byte FurnaceData        = 3;
		const byte Multi              = 4;
		const byte ShulkerBox         = 5;
		const byte ShapelessChemistry = 6;
		const byte ShapedChemistry    = 7;
		
		private void ReadNewRecipes()
		{
			recipes = new Recipes();

			var count = ReadUnsignedVarInt();
			Log.Trace($"Reading {count} recipes");

			for (int i = 0; i < count; i++)
			{
				int recipeType = ReadSignedVarInt();

				Log.Trace($"Read recipe no={i} type={recipeType}");

				if (recipeType < 0 /*|| len == 0*/)
				{
					Log.Error("Read void recipe");
					break;
				}

				switch (recipeType)
				{
					case ShapelessChemistry:
					case Shapeless:
					case ShulkerBox:
					{
						var recipe = new ShapelessRecipe();
						ReadString(); // some unique id
						int ingrediensCount = ReadVarInt(); // 
						for (int j = 0; j < ingrediensCount; j++)
						{
							recipe.Input.Add(ReadRecipeIngredient());
						}
						int resultCount = ReadVarInt(); // 1?
						for (int j = 0; j < resultCount; j++)
						{
							recipe.Result.Add(this.ReadItem2());
						}
						recipe.Id = ReadUUID(); // Id
						recipe.Block = ReadString(); // block?
						ReadSignedVarInt(); // priority
						recipe.UniqueId = ReadVarInt(); // unique id
						recipes.Add(recipe);
						//Log.Error("Read shapeless recipe");
						break;
					}
					case ShapedChemistry:
					case Shaped:
					{
						ReadString(); // some unique id
						int width = ReadVarInt(); // Width
						int height = ReadVarInt(); // Height
						
						var recipe = new ShapedRecipe(width, height);
						
						if (width > 3 || height > 3) throw new Exception("Wrong number of ingredience. Width=" + width + ", height=" + height);
						for (int w = 0; w < width; w++)
						{
							for (int h = 0; h < height; h++)
							{
								recipe.Input[(h * width) + w] = ReadRecipeIngredient();
							}
						}

						int resultCount = ReadVarInt(); // 1?
						for (int j = 0; j < resultCount; j++)
						{
							recipe.Result.Add(this.ReadItem2());
						}
						recipe.Id = ReadUUID(); // Id
						recipe.Block = ReadString(); // block?
						ReadSignedVarInt(); // priority
						recipe.UniqueId = ReadVarInt(); // unique id
						recipes.Add(recipe);
						//Log.Error("Read shaped recipe");
						break;
					}
					case FurnaceData:
					case Furnace:
					{
						var recipe = new SmeltingRecipe();
						short id = (short) ReadSignedVarInt(); // input (with metadata) 
						Item result = this.ReadItem2(); // Result
						recipe.Block = ReadString(); // block?
						recipe.Input = ItemFactory.GetItem(id, 0);
						recipe.Result = result;
						recipes.Add(recipe);
						//Log.Error("Read furnace recipe");
						//Log.Error($"Input={id}, meta={""} Item={result.Id}, Meta={result.Metadata}");
						break;
					}
					/*case FurnaceData:
					{
						//const ENTRY_FURNACE_DATA = 3;
						var recipe = new SmeltingRecipe();
						short id = (short) ReadSignedVarInt(); // input (with metadata) 
						short meta = (short) ReadSignedVarInt(); // input (with metadata) 
						Item result = ReadItem(); // Result
						recipe.Block = ReadString(); // block?
						recipe.Input = ItemFactory.GetItem(id, meta);
						recipe.Result = result;
						recipes.Add(recipe);
						//Log.Error("Read smelting recipe");
						//Log.Error($"Input={id}, meta={meta} Item={result.Id}, Meta={result.Metadata}");
						break;
					}*/
					case Multi:
					{
						//Log.Error("Reading MULTI");

						var recipe = new MultiRecipe();
						recipe.Id = ReadUUID();
						recipe.UniqueId = ReadVarInt(); // unique id
						recipes.Add(recipe);
						break;
					}
					/*case ShapelessChemistry:
					{
						var recipe = new ShapelessRecipe();
						ReadString(); // some unique id
						int ingrediensCount = ReadVarInt(); // 
						for (int j = 0; j < ingrediensCount; j++)
						{
							recipe.Input.Add(ReadRecipeIngredient());
						}
						int resultCount = ReadVarInt(); // 1?
						for (int j = 0; j < resultCount; j++)
						{
							recipe.Result.Add(ReadItem());
						}
						recipe.Id = ReadUUID(); // Id
						recipe.Block = ReadString(); // block?
						ReadSignedVarInt(); // priority
						recipe.UniqueId = ReadVarInt(); // unique id
						//recipes.Add(recipe);
						//Log.Error("Read shapeless recipe");
						break;
					}*/
					/*case ShapedChemistry:
					{
						ReadString(); // some unique id
						int width = ReadSignedVarInt(); // Width
						int height = ReadSignedVarInt(); // Height
						var recipe = new ShapedRecipe(width, height);
						if (width > 3 || height > 3) throw new Exception("Wrong number of ingredience. Width=" + width + ", height=" + height);
						for (int w = 0; w < width; w++)
						{
							for (int h = 0; h < height; h++)
							{
								recipe.Input[(h * width) + w] = ReadRecipeIngredient();
							}
						}

						int resultCount = ReadVarInt(); // 1?
						for (int j = 0; j < resultCount; j++)
						{
							recipe.Result.Add(ReadItem());
						}
						recipe.Id = ReadUUID(); // Id
						recipe.Block = ReadString(); // block?
						ReadSignedVarInt(); // priority
						recipe.UniqueId = ReadVarInt(); // unique id
						//recipes.Add(recipe);
						//Log.Error("Read shaped recipe");
						break;
					}
				*/
					default:
						Log.Error($"Read unknown recipe type: {recipeType}");
						//ReadBytes(len);
						break;
				}
			}

			Log.Trace($"Done reading {count} recipes");
		}
	}
}