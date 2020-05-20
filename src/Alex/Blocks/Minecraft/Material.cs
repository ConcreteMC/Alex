using Alex.API.Blocks;
using Alex.API.Utils;
using Alex.Utils;
using ItemType = Alex.API.Utils.ItemType;

namespace Alex.Blocks.Minecraft
{
    public class Material : IMaterial
	{
		public static IMaterial Air = new MaterialTransparent(MapColor.AIR);
		public static IMaterial Grass = new Material(MapColor.GRASS);
		public static IMaterial Ground = new Material(MapColor.DIRT).SetHardness(0.5f);
		public static IMaterial Wood = (new Material(MapColor.WOOD)).SetBurning().SetHardness(2f);

		public static IMaterial Stone = (new Material(MapColor.STONE)).SetRequiresTool()
			.SetRequiredTool(ItemType.PickAxe, ItemMaterial.AnyMaterial).SetHardness(3f);
		
		public static IMaterial Ore = new Material(MapColor.STONE).SetRequiresTool().SetRequiredTool(ItemType.PickAxe, ItemMaterial.Any).SetHardness(3);
		
		public static IMaterial Iron = (new Material(MapColor.IRON)).SetRequiresTool().SetHardness(5f);
		public static IMaterial Anvil = (new Material(MapColor.IRON)).SetRequiresTool().SetImmovableMobility();
		public static IMaterial Water = (new MaterialLiquid(MapColor.WATER)).SetTranslucent().SetNoPushMobility();
		public static IMaterial Lava = (new MaterialLiquid(MapColor.TNT)).SetNoPushMobility();
		public static IMaterial Leaves = (new Material(MapColor.FOLIAGE)).SetBurning().SetNoPushMobility().SetHardness(0.2f);
		public static IMaterial Plants = (new MaterialLogic(MapColor.FOLIAGE)).SetNoPushMobility().SetHardness(0.6f);
		public static IMaterial Vine = (new MaterialLogic(MapColor.FOLIAGE)).SetBurning().SetNoPushMobility().SetReplaceable();
		public static IMaterial Sponge = new Material(MapColor.YELLOW);
		public static IMaterial Cloth = (new Material(MapColor.CLOTH)).SetBurning();
		public static IMaterial Fire = (new MaterialTransparent(MapColor.AIR)).SetNoPushMobility();
		public static IMaterial Sand = new Material(MapColor.SAND);
		public static IMaterial Circuits = (new MaterialLogic(MapColor.AIR)).SetNoPushMobility().SetHardness(0.2f);
		public static IMaterial Carpet = (new MaterialLogic(MapColor.CLOTH)).SetBurning();
		public static IMaterial Glass = (new Material(MapColor.AIR)).SetTranslucent().SetAdventureModeExempt().SetHardness(0.3f);
		public static IMaterial RedstoneLight = (new Material(MapColor.AIR)).SetAdventureModeExempt();
		public static IMaterial Tnt = (new Material(MapColor.TNT)).SetBurning();
		public static IMaterial Coral = (new Material(MapColor.FOLIAGE)).SetNoPushMobility();
		public static IMaterial Ice = (new Material(MapColor.ICE)).SetTranslucent().SetAdventureModeExempt();
		public static IMaterial PackedIce = (new Material(MapColor.ICE)).SetAdventureModeExempt();
		public static IMaterial Snow = (new MaterialLogic(MapColor.SNOW)).SetReplaceable().SetRequiresTool().SetNoPushMobility();
		public static IMaterial CraftedSnow = (new Material(MapColor.SNOW)).SetRequiresTool();
		public static IMaterial Cactus = (new Material(MapColor.FOLIAGE)).SetNoPushMobility();
		public static IMaterial Clay = new Material(MapColor.CLAY);
		public static IMaterial Gourd = (new Material(MapColor.FOLIAGE)).SetNoPushMobility();
		public static IMaterial DragonEgg = (new Material(MapColor.FOLIAGE)).SetNoPushMobility();
		public static IMaterial Portal = (new MaterialPortal(MapColor.AIR)).SetTranslucent().SetImmovableMobility();
		public static IMaterial Cake = (new Material(MapColor.AIR)).SetNoPushMobility();
		public static IMaterial Web = (new Material(MapColor.CLOTH)).SetRequiresTool().SetNoPushMobility();

		public static IMaterial Piston = (new Material(MapColor.STONE)).SetImmovableMobility();
		public static IMaterial Barrier = (new Material(MapColor.AIR)).SetRequiresTool().SetImmovableMobility();
		public static IMaterial StructureVoid = new MaterialTransparent(MapColor.AIR);

		private bool _canBurn;
		private bool _replaceable;
		private bool _isTranslucent;
		private bool _requiresNoTool = true;
		private bool _isAdventureModeExempt;
		
		private MapColor MapColor { get; }
		public Material(MapColor color)
		{
			this.MapColor = color;
		}

		public virtual bool IsLiquid()
		{
			return false;
		}

		public virtual bool IsSolid()
		{
			return true;
		}

		public float Hardness { get; private set; } = 2f;

		public IMaterial SetHardness(float hardness)
		{
			Hardness = hardness;
			return this;
		}

		public virtual bool BlocksLight()
		{
			return true;
		}

		public virtual bool BlocksMovement()
		{
			return true;
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

		public virtual bool GetCanBurn()
		{
			return this._canBurn;
		}

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

		public virtual bool IsReplaceable()
		{
			return this._replaceable;
		}

		public virtual bool IsOpaque()
		{
			return !this._isTranslucent;
		}

		public virtual bool IsToolRequired()
		{
			return this._requiresNoTool;
		}

		public IMaterial SetNoPushMobility()
		{
		//	this.mobilityFlag = EnumPushReaction.DESTROY;
			return this;
		}

		public IMaterial SetImmovableMobility()
		{
		//	this.mobilityFlag = EnumPushReaction.BLOCK;
			return this;
		}

		public IMaterial SetAdventureModeExempt()
		{
			this._isAdventureModeExempt = true;
			return this;
		}

		public virtual IMapColor GetMaterialMapColor()
		{
			//return MapColor.AIR;
			return this.MapColor;
		}
		
		public IMaterial Clone()
		{
			return new Material(this.MapColor)
			{
				_replaceable = _replaceable,
				_canBurn = _canBurn,
				_isTranslucent = _isTranslucent,
				_requiredMaterial = _requiredMaterial,
				_requiredTool = _requiredTool,
				Hardness = Hardness,
				_requiresNoTool = _requiresNoTool,
				_isAdventureModeExempt = _isAdventureModeExempt
			};
		}
	}

    public class MaterialLeaves : Material
    {
	    public MaterialLeaves(MapColor color) : base(color)
	    {
		    
	    }

	    public override bool BlocksLight()
	    {
		    return false;
	    }
    }
    
	public class MaterialLogic : Material
	{
		public MaterialLogic(MapColor color) : base(color)
		{
			this.SetAdventureModeExempt();
		}

		public override bool IsSolid()
		{
			return false;
		}

		public override bool BlocksLight()
		{
			return false;
		}

		public override bool BlocksMovement()
		{
			return false;
		}
	}

	public class MaterialTransparent : Material
	{
		public MaterialTransparent(MapColor color) : base(color)
		{
			this.SetReplaceable();
		}

		public override bool IsSolid()
		{
			return false;
		}

		public override bool BlocksLight()
		{
			return false;
		}

		public override bool BlocksMovement()
		{
			return false;
		}
	}

	public class MaterialLiquid : Material
	{
		public MaterialLiquid(MapColor color) : base(color) {
			this.SetReplaceable();
			this.SetNoPushMobility();
		}

		public override bool IsLiquid()
		{
			return true;
		}

		public override bool BlocksMovement()
		{
			return false;
		}

		public override bool IsSolid()
		{
			return false;
		}
	}

	public class MaterialPortal : Material
	{
		public MaterialPortal(MapColor color) : base(color)
		{
		
		}

		public override bool IsSolid()
		{
			return false;
		}

		public override bool BlocksLight()
		{
			return false;
		}

		public override bool BlocksMovement()
		{
			return false;
		}
	}
}
