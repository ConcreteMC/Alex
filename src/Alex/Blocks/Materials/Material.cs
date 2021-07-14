using Alex.Common.Blocks;
using Alex.Common.Items;
using Microsoft.Xna.Framework;
using ItemType = Alex.Common.Items.ItemType;

namespace Alex.Blocks.Materials
{
	//https://minecraft.gamepedia.com/Materials
	public class Material : IMaterial
	{
		public static readonly IMaterial Air = new MaterialTransparent(Utils.MapColor.Air);

		public static readonly IMaterial Grass = new Material(Utils.MapColor.Grass)
		   .WithTintType(TintType.Grass)
		   .WithHardness(0.6f)
		   .WithSoundCategory("grass");

		public static readonly IMaterial Dirt = new Material(Utils.MapColor.Dirt)
		   .WithHardness(0.5f);

		public static readonly IMaterial Wood = (new Material(Utils.MapColor.Wood))
		   .SetFlammable()
		   .WithHardness(2f)
		   .WithSoundCategory("wood");

		public static readonly IMaterial Wool = (new Material(Utils.MapColor.Cloth))
		   .SetFlammable()
		   .WithHardness(0.8f)
		   .WithSoundCategory("wool");

		public static readonly IMaterial Stone = (new Material(Utils.MapColor.Stone))
		   .SetRequiresTool()
		   .SetRequiredTool(ItemType.PickAxe, ItemMaterial.AnyMaterial)
		   .WithHardness(3f)
		   .WithSoundCategory("normal");

		public static readonly IMaterial Ore = new Material(Utils.MapColor.Stone)
		   .SetRequiresTool()
		   .SetRequiredTool(ItemType.PickAxe, ItemMaterial.Any)
		   .WithHardness(3);

		public static readonly IMaterial Metal = (new Material(Utils.MapColor.Iron))
		   .SetRequiresTool()
		   .WithHardness(5f)
		   .WithSoundCategory("metal");

		public static readonly IMaterial Anvil = (new Material(Utils.MapColor.Iron))
		   .SetRequiresTool()
		   .WithHardness(5f);

		public static readonly IMaterial Water = (new MaterialLiquid(Utils.MapColor.Water))
		   .SetTranslucent()
		   .WithTintType(TintType.Water, new Color(68, 175, 245));

		public static readonly IMaterial Lava = (new MaterialLiquid(Utils.MapColor.Fire));

		public static readonly IMaterial Leaves = (new Material(Utils.MapColor.Foliage))
		   .WithTintType(TintType.Foliage)
		   .SetFlammable()
		   .WithHardness(0.2f);

		public static readonly IMaterial Plants = (new MaterialLogic(Utils.MapColor.Foliage))
		   .WithTintType(TintType.Foliage)
		   .WithHardness(0.6f);

		public static readonly IMaterial Vine = (new MaterialLogic(Utils.MapColor.Foliage))
		   .WithTintType(TintType.Foliage)
		   .SetFlammable()
		   .SetReplaceable()
		   .WithHardness(0.2f);

		public static readonly IMaterial Sponge = new Material(Utils.MapColor.Yellow);
		public static readonly IMaterial Cloth = (new Material(Utils.MapColor.Cloth))
		   .SetFlammable();
		
		public static readonly IMaterial Fire = (new MaterialTransparent(Utils.MapColor.Fire));
		public static readonly IMaterial Sand = new Material(Utils.MapColor.Sand);
		public static readonly IMaterial Decoration = (new MaterialLogic(Utils.MapColor.Air))
		   .WithHardness(0.2f);

		public static readonly IMaterial Carpet = (new MaterialLogic(Utils.MapColor.Cloth))
		   .SetFlammable()
		   .WithHardness(0.1f);

		public static readonly IMaterial Glass = (new Material(Utils.MapColor.Air))
		   .SetTranslucent()
		   .WithHardness(0.3f);
		
		public static readonly IMaterial RedstoneLight = (new Material(Utils.MapColor.Air));
		public static readonly IMaterial Explosive = (new Material(Utils.MapColor.Fire))
		   .SetFlammable();
		
		public static readonly IMaterial Coral = (new Material(Utils.MapColor.Foliage));

		public static readonly IMaterial Ice = (new Material(Utils.MapColor.Ice))
		   .SetTranslucent()
		   .WithSlipperiness(0.98d);

		public static readonly IMaterial BlueIce = (new Material(Utils.MapColor.Ice))
		   .SetTranslucent()
		   .WithSlipperiness(0.989d);

		public static readonly IMaterial PackedIce = (new Material(Utils.MapColor.Ice))
		   .WithSlipperiness(0.989d);

		public static readonly IMaterial Snow = (new MaterialLogic(Utils.MapColor.Snow))
		   .SetReplaceable()
		   .SetRequiresTool()
		   .WithTintType(TintType.Color, Color.Snow);

		public static readonly IMaterial CraftedSnow = (new Material(Utils.MapColor.Snow))
		   .SetRequiresTool();
		
		public static readonly IMaterial Cactus = (new Material(Utils.MapColor.Foliage))
		   .WithHardness(0.4f);
		
		public static readonly IMaterial Clay = new Material(Utils.MapColor.Clay);
		public static readonly IMaterial Gourd = (new Material(Utils.MapColor.Foliage));
		public static readonly IMaterial DragonEgg = (new Material(Utils.MapColor.Foliage));
		public static readonly IMaterial Portal = (new MaterialPortal(Utils.MapColor.Air))
		   .SetTranslucent();
		
		public static readonly IMaterial Cake = (new Material(Utils.MapColor.Air));
		public static readonly IMaterial Web = (new Material(Utils.MapColor.Cloth))
		   .SetRequiresTool();
		
		public static readonly IMaterial Piston = (new Material(Utils.MapColor.Stone));
		public static readonly IMaterial Barrier = (new Material(Utils.MapColor.Air))
		   .SetRequiresTool();
		
		public static readonly IMaterial StructureVoid = new MaterialTransparent(Utils.MapColor.Air);

		public static readonly IMaterial Slime = new Material(Utils.MapColor.Green)
		   .SetTranslucent()
		   .WithSlipperiness(0.8d);

		public static readonly IMaterial WaterPlant = new Material(Utils.MapColor.Water)
		   .SetWaterLoggable();

		public static readonly IMaterial ReplaceableWaterPlant =
			new Material(Utils.MapColor.Water)
			   .SetReplaceable()
			   .SetWaterLoggable();

		public static readonly IMaterial Bamboo = new Material(Utils.MapColor.Brown)
		   .SetFlammable();

		public static readonly IMaterial BambooSapling = new Material(Utils.MapColor.Brown)
		   .SetFlammable()
		   .SetCollisionBehavior(BlockCollisionBehavior.None);

		public static readonly IMaterial BubbleColumn = new MaterialLiquid(Utils.MapColor.Water);

		private ItemType _requiredTool = ItemType.Any;
		private ItemMaterial _requiredMaterial = ItemMaterial.Any;

		private int _mapColorIndex;
		public int ColorIndex => MapColor.Index * 4 + _mapColorIndex;
		public Color BlockColor => Utils.MapColor.GetBlockColor(MapColor.Index * 4 + _mapColorIndex);
		public IMapColor MapColor { get; set; }
		public TintType TintType { get; protected set; } = TintType.Default;
		public Color TintColor { get; protected set; } = Color.White;

		public Material(IMapColor color)
		{
			MapColor = color;
			_mapColorIndex = 2;
			//this.MapColor = color;
		}

		public bool IsWatterLoggable { get; set; } = false;

		public bool IsReplaceable { get; set; } = false;

		public bool BlocksLight { get; set; } = true;

		public bool BlocksMovement { get; set; } = true;

		public bool IsLiquid { get; set; } = false;

		public bool IsSolid { get; set; } = true;

		public float Hardness { get; set; } = 2f;

		public bool IsFlammable { get; set; } = false;

		public double Slipperiness { get; set; } = 0.6;

		public bool IsTranslucent { get; set; } = false;

		public string SoundCategory { get; set; } = "normal";

		public bool IsToolRequired { get; set; } = false;

		public bool IsOpaque => !IsTranslucent;

		public IMaterial WithHardness(float hardness)
		{
			Hardness = hardness;

			return this;
		}

		public IMaterial WithSoundCategory(string soundCategory)
		{
			SoundCategory = soundCategory;

			return this;
		}

		public IMaterial WithMapColor(IMapColor color, int index = 2)
		{
			MapColor = color;
			_mapColorIndex = index;
			//MapColor = color;

			return this;
		}

		public IMaterial SetCollisionBehavior(BlockCollisionBehavior collisionBehavior)
		{
			BlocksMovement = collisionBehavior == BlockCollisionBehavior.Blocking;

			return this;
		}

		public IMaterial SetTranslucent()
		{
			this.IsTranslucent = true;

			return this;
		}

		public IMaterial SetRequiresTool()
		{
			this.IsToolRequired = true;

			return this;
		}

		public IMaterial SetFlammable()
		{
			this.IsFlammable = true;

			return this;
		}

		public IMaterial SetReplaceable()
		{
			this.IsReplaceable = true;

			return this;
		}

		public IMaterial SetRequiredTool(ItemType type, ItemMaterial material = ItemMaterial.Any)
		{
			_requiredTool = type;
			_requiredMaterial = material;

			return this;
		}

		public IMaterial SetWaterLoggable()
		{
			IsWatterLoggable = true;

			return this;
		}

		public IMaterial WithTintType(TintType type, Color color)
		{
			TintType = type;
			TintColor = color;

			return this;
		}

		public IMaterial WithTintType(TintType type)
		{
			return this.WithTintType(type, TintColor);
		}

		public IMaterial WithSlipperiness(double value)
		{
			Slipperiness = value;

			return this;
		}

		public bool CanUseTool(ItemType type, ItemMaterial material)
		{
			bool hasRequiredType = _requiredTool == type || _requiredTool.HasFlag(type);

			bool hasRequiredMaterial = _requiredMaterial == material || _requiredMaterial.HasFlag(material)
			                                                         || (material > _requiredMaterial);

			return hasRequiredType && hasRequiredMaterial;
		}

		public IMaterial Clone()
		{
			return new Material(this.MapColor)
			{
				IsReplaceable = IsReplaceable,
				IsFlammable = IsFlammable,
				IsTranslucent = IsTranslucent,
				Hardness = Hardness,
				Slipperiness = Slipperiness,
				TintType = this.TintType,
				TintColor = this.TintColor,
				BlocksLight = BlocksLight,
				BlocksMovement = BlocksMovement,
				IsLiquid = IsLiquid,
				IsSolid = IsSolid,
				SoundCategory = SoundCategory,
				IsWatterLoggable = IsWatterLoggable,
				_requiredMaterial = _requiredMaterial,
				_requiredTool = _requiredTool,
				IsToolRequired = IsToolRequired,
				MapColor = MapColor
			};
		}
	}
}
