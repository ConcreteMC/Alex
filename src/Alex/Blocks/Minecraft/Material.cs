using Alex.API.Blocks;
using Alex.API.Utils;
using Alex.Utils;
using Microsoft.Xna.Framework;
using ItemType = Alex.API.Utils.ItemType;

namespace Alex.Blocks.Minecraft
{
    public class Material : IMaterial
	{
		public static readonly IMaterial Air = new MaterialTransparent(MapColor.AIR);
		public static readonly IMaterial Grass = new Material(MapColor.GRASS).SetTintType(TintType.Grass);
		public static readonly IMaterial Ground = new Material(MapColor.DIRT).SetHardness(0.5f);
		public static readonly IMaterial Wood = (new Material(MapColor.WOOD)).SetBurning().SetHardness(2f);
		public static readonly IMaterial NetherWood = new Material(MapColor.WOOD).SetBlocksMovement().SetHardness(2f);
		
		public static readonly IMaterial Stone = (new Material(MapColor.STONE)).SetRequiresTool()
			.SetRequiredTool(ItemType.PickAxe, ItemMaterial.AnyMaterial).SetHardness(3f);
		
		public static readonly IMaterial Ore = new Material(MapColor.STONE).SetRequiresTool().SetRequiredTool(ItemType.PickAxe, ItemMaterial.Any).SetHardness(3);
		
		public static readonly IMaterial Iron = (new Material(MapColor.IRON)).SetRequiresTool().SetHardness(5f);
		public static readonly IMaterial Anvil = (new Material(MapColor.IRON)).SetRequiresTool();

		public static readonly IMaterial Water = (new MaterialLiquid(MapColor.WATER)).SetTranslucent()
		   .SetTintType(TintType.Water, new Color(68, 175, 245));
		//;.SetTintType(TintType.Color, new Color(68, 175, 245));
		
		public static readonly IMaterial Lava = (new MaterialLiquid(MapColor.TNT));
		public static readonly IMaterial Leaves = (new Material(MapColor.FOLIAGE)).SetTintType(TintType.Foliage).SetBurning().SetHardness(0.2f);
		
		public static readonly IMaterial Plants = (new MaterialLogic(MapColor.FOLIAGE)).SetTintType(TintType.Foliage).SetHardness(0.6f);
		public static readonly IMaterial ReplaceablePlants = (new MaterialLogic(MapColor.FOLIAGE)).SetTintType(TintType.Foliage).SetHardness(0.6f).SetReplaceable();
		
		public static readonly IMaterial Vine = (new MaterialLogic(MapColor.FOLIAGE)).SetTintType(TintType.Foliage).SetBurning().SetReplaceable();
		public static readonly IMaterial Sponge = new Material(MapColor.YELLOW);
		public static readonly IMaterial Cloth = (new Material(MapColor.CLOTH)).SetBurning();
		public static readonly IMaterial Fire = (new MaterialTransparent(MapColor.AIR));
		public static readonly IMaterial Sand = new Material(MapColor.SAND);
		public static readonly IMaterial Circuits = (new MaterialLogic(MapColor.AIR)).SetHardness(0.2f);
		public static readonly IMaterial Carpet = (new MaterialLogic(MapColor.CLOTH)).SetBurning();
		public static readonly IMaterial Glass = (new Material(MapColor.AIR)).SetTranslucent().SetHardness(0.3f);
		public static readonly IMaterial RedstoneLight = (new Material(MapColor.AIR));
		public static readonly IMaterial Tnt = (new Material(MapColor.TNT)).SetBurning();
		public static readonly IMaterial Coral = (new Material(MapColor.FOLIAGE));
		public static readonly IMaterial Ice = (new Material(MapColor.ICE)).SetTranslucent().SetSlipperines(0.98d);
		public static readonly IMaterial BlueIce = (new Material(MapColor.ICE)).SetTranslucent().SetSlipperines(0.989d);
		public static readonly IMaterial PackedIce = (new Material(MapColor.ICE)).SetSlipperines(0.989d);
		public static readonly IMaterial Snow = (new MaterialLogic(MapColor.SNOW)).SetReplaceable().SetRequiresTool();
		public static readonly IMaterial CraftedSnow = (new Material(MapColor.SNOW)).SetRequiresTool();
		public static readonly IMaterial Cactus = (new Material(MapColor.FOLIAGE));
		public static readonly IMaterial Clay = new Material(MapColor.CLAY);
		public static readonly IMaterial Gourd = (new Material(MapColor.FOLIAGE));
		public static readonly IMaterial DragonEgg = (new Material(MapColor.FOLIAGE));
		public static readonly IMaterial Portal = (new MaterialPortal(MapColor.AIR)).SetTranslucent();
		public static readonly IMaterial Cake = (new Material(MapColor.AIR));
		public static readonly IMaterial Web = (new Material(MapColor.CLOTH)).SetRequiresTool();

		public static readonly IMaterial Piston = (new Material(MapColor.STONE));
		public static readonly IMaterial Barrier = (new Material(MapColor.AIR)).SetRequiresTool();
		public static readonly IMaterial StructureVoid = new MaterialTransparent(MapColor.AIR);

		public static readonly IMaterial Slime = new Material(MapColor.GREEN).SetTranslucent().SetSlipperines(0.8d);
		//https://minecraft.gamepedia.com/Materials
		public static readonly IMaterial WaterPlant = new Material(MapColor.WATER).SetWaterLoggable();
		public static readonly IMaterial ReplaceableWaterPlant = new Material(MapColor.WATER).SetReplaceable().SetWaterLoggable();
		private bool _canBurn;
		private bool _replaceable;
		private bool _isTranslucent;
		private bool _requiresNoTool = true;

		public IMapColor MapColorValue { get; }
		public Material(IMapColor color)
		{
			this.MapColorValue = color;
		}

		public virtual bool IsLiquid { get; protected set; } = false;

		public virtual bool IsSolid { get; protected set; } = true;

		public float Hardness { get; private set; } = 2f;

		public IMaterial SetHardness(float hardness)
		{
			Hardness = hardness;
			return this;
		}

		public virtual bool BlocksLight { get; protected set; } = true;

		public virtual bool BlocksMovement { get; protected set; } = true;

		public IMaterial SetBlocksMovement()
		{
			BlocksMovement = false;

			return this;
		}
		
		public IMaterial SetTranslucent()
		{
			this._isTranslucent = true;
			return this;
		}

		public IMaterial SetRequiresTool()
		{
			this._requiresNoTool = false;
			return this;
		}

		public IMaterial SetBurning()
		{
			this._canBurn = true;
			return this;
		}

		public virtual bool CanBurn => _canBurn;

		public IMaterial SetReplaceable()
		{
			this._replaceable = true;
			return this;
		}

		private ItemType _requiredTool = ItemType.Any;
		private ItemMaterial _requiredMaterial = ItemMaterial.Any;
		public bool CanUseTool(ItemType type, ItemMaterial material)
		{
			bool hasRequiredType = _requiredTool == type || _requiredTool.HasFlag(type);
			bool hasRequiredMaterial = _requiredMaterial == material || _requiredMaterial.HasFlag(material) ||
			                           (material > _requiredMaterial);

			return hasRequiredType && hasRequiredMaterial;
		}

		public IMaterial SetRequiredTool(ItemType type, ItemMaterial material = ItemMaterial.Any)
		{
			_requiredTool = type;
			_requiredMaterial = material;
			
			return this;
		}

		public virtual bool IsWatterLoggable { get; private set; } = false;

		public IMaterial SetWaterLoggable()
		{
			IsWatterLoggable = true;
			return this;
		}
		
		public virtual bool IsReplaceable => this._replaceable;

		public virtual bool IsOpaque => !this._isTranslucent;

		public virtual bool IsToolRequired => !_requiresNoTool;

		public TintType TintType { get; protected set; } = TintType.Default;
		public Color TintColor { get; protected set; } = Color.White;
		public IMaterial SetTintType(TintType type, Color color)
		{
			TintType = type;
			TintColor = color;

			return this;
		}
		
		public IMaterial SetTintType(TintType type)
		{
			return this.SetTintType(type, TintColor);
		}

		public double Slipperiness { get; private set; } = 0.6;

		public IMaterial SetSlipperines(double value)
		{
			Slipperiness = value;

			return this;
		}
		
		public IMaterial Clone()
		{
			return new Material(this.MapColorValue)
			{
				_replaceable = _replaceable,
				_canBurn = _canBurn,
				_isTranslucent = _isTranslucent,
				_requiredMaterial = _requiredMaterial,
				_requiredTool = _requiredTool,
				Hardness = Hardness,
				_requiresNoTool = _requiresNoTool,
				Slipperiness = Slipperiness,
				TintType = this.TintType,
				TintColor = this.TintColor
			};
		}
	}

    public class MaterialLogic : Material
	{
		public MaterialLogic(MapColor color) : base(color)
		{
			base.IsSolid = false;
			base.BlocksLight = false;
			base.BlocksMovement = false;
		}
	}

	public class MaterialTransparent : Material
	{
		public MaterialTransparent(MapColor color) : base(color)
		{
			this.SetReplaceable();
			
			base.IsSolid = false;
			base.BlocksLight = false;
			base.BlocksMovement = false;
		}
	}

	public class MaterialLiquid : Material
	{
		public MaterialLiquid(MapColor color) : base(color) {
			this.SetReplaceable();
			
			base.IsSolid = false;
			base.BlocksMovement = false;
			base.IsLiquid = true;
		}
	}

	public class MaterialPortal : Material
	{
		public MaterialPortal(MapColor color) : base(color)
		{
			base.IsSolid = false;
			base.BlocksLight = false;
			base.BlocksMovement = false;
		}
	}
}
