using System;
using System.Collections.Generic;
using MiNET.Blocks;
using MiNET.Crafting;
using MiNET.Items;
using MiNET.Net;
using MiNET.Utils;
using NLog;
using Item = Alex.Items.Item;

namespace Alex.Net.Bedrock.Packets
{
	public class CraftingData : McpeCraftingData
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
		
		protected override void DecodePacket()
		{
			Id = (byte) ReadVarInt();
			
			recipes = ReadNewRecipes();
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
		
		private Recipes ReadNewRecipes()
		{
			var result = new Recipes();

			var count = ReadUnsignedVarInt();

			for (int i = 0; i < count; i++)
			{
				var recipeType = ReadVarInt();

				switch (recipeType)
				{
					case Shapeless:
					case ShapelessChemistry:
					case ShulkerBox:
						result.Add(ReadShapelessRecipe());
						break;
					
					case Shaped:
					case ShapedChemistry:
						result.Add(ReadShapedRecipe());
						break;
					
					case Furnace:
						result.Add(ReadSmeltingRecipe());
						break;
					
					case FurnaceData:
						result.Add(ReadSmeltingRecipeData());
						break;
					
					case Multi:
						result.Add(ReadMultiRecipe());
						break;
				}
			}

			Log.Trace($"Done reading {count} recipes");

			return result;
		}

		private MultiRecipe ReadMultiRecipe()
		{
			var uuid      = ReadUUID();
			var networkId = ReadUnsignedVarInt();

			return new MultiRecipe() {Id = uuid, UniqueId = (int) networkId};
		}
		
		private SmeltingRecipe ReadSmeltingRecipeData()
		{
			var inputId     = ReadVarInt();
			var meta        = ReadVarInt();
			var output      = CreativeContent.ReadItem2(this);
			var craftingTag = ReadString();

			return new SmeltingRecipe(output, ItemFactory.GetItem((short) inputId, (short)meta), craftingTag);
		}

		private SmeltingRecipe ReadSmeltingRecipe()
		{
			var inputId     = ReadVarInt();
			var output      = CreativeContent.ReadItem2(this);
			var craftingTag = ReadString();

			return new SmeltingRecipe(output, ItemFactory.GetItem((short) inputId), craftingTag);
		}
		
		private ShapedRecipe ReadShapedRecipe()
		{
			var                    recipeId   = ReadString();
			int                    width      = ReadVarInt();
			int                    height     = ReadVarInt();
			var                    inputCount = width * height;
			
			List<MiNET.Items.Item> inputs  = new List<MiNET.Items.Item>(inputCount);
			List<MiNET.Items.Item> outputs = new List<MiNET.Items.Item>();

			for (int i = 0; i < inputCount; i++)
			{
				inputs.Add(ReadRecipeIngredient());
			}

			var  count = ReadVarInt();

			for (int i = 0; i < count; i++)
			{
				outputs.Add(CreativeContent.ReadItem2(this));
			}

			var uuid        = ReadUUID();
			var craftingTag = ReadString();
			var priority    = ReadVarInt();
			var networkId   = ReadUnsignedVarInt();
			
			return  new ShapedRecipe( width, height, outputs, inputs.ToArray())
			{
				Id = uuid,
				UniqueId = (int) networkId,
				Block = craftingTag
			};
		}
		
		private ShapelessRecipe ReadShapelessRecipe()
		{
			var                    recipeId = ReadString();
			List<MiNET.Items.Item> inputs   = new List<MiNET.Items.Item>();
			List<MiNET.Items.Item> outputs  = new List<MiNET.Items.Item>();

			int count = ReadVarInt();

			for (int i = 0; i < count; i++)
			{
				inputs.Add(ReadRecipeIngredient());
			}

			count = ReadVarInt();

			for (int i = 0; i < count; i++)
			{
				outputs.Add(CreativeContent.ReadItem2(this));
			}

			var uuid        = ReadUUID();
			var craftingTag = ReadString();
			var priority    = ReadVarInt();
			var networkId   = ReadUnsignedVarInt();
			
			return  new ShapelessRecipe(outputs, inputs)
			{
				Id = uuid,
				UniqueId = (int) networkId,
				Block = craftingTag
			};
		}

	}
}