using Alex.API.Blocks;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
    public class Material : IMaterial
	{
		public static IMaterial Air = new MaterialTransparent(MapColor.AIR);
		public static IMaterial Grass = new Material(MapColor.GRASS);
		public static IMaterial Ground = new Material(MapColor.DIRT);
		public static IMaterial Wood = (new Material(MapColor.WOOD)).SetBurning();
		public static IMaterial Rock = (new Material(MapColor.STONE)).SetRequiresTool();
		public static IMaterial Iron = (new Material(MapColor.IRON)).SetRequiresTool();
		public static IMaterial Anvil = (new Material(MapColor.IRON)).SetRequiresTool().SetImmovableMobility();
		public static IMaterial Water = (new MaterialLiquid(MapColor.WATER)).SetTranslucent().SetNoPushMobility();
		public static IMaterial Lava = (new MaterialLiquid(MapColor.TNT)).SetNoPushMobility();
		public static IMaterial Leaves = (new Material(MapColor.FOLIAGE)).SetBurning().SetNoPushMobility();
		public static IMaterial Plants = (new MaterialLogic(MapColor.FOLIAGE)).SetNoPushMobility();
		public static IMaterial Vine = (new MaterialLogic(MapColor.FOLIAGE)).SetBurning().SetNoPushMobility().SetReplaceable();
		public static IMaterial Sponge = new Material(MapColor.YELLOW);
		public static IMaterial Cloth = (new Material(MapColor.CLOTH)).SetBurning();
		public static IMaterial Fire = (new MaterialTransparent(MapColor.AIR)).SetNoPushMobility();
		public static IMaterial Sand = new Material(MapColor.SAND);
		public static IMaterial Circuits = (new MaterialLogic(MapColor.AIR)).SetNoPushMobility();
		public static IMaterial Carpet = (new MaterialLogic(MapColor.CLOTH)).SetBurning();
		public static IMaterial Glass = (new Material(MapColor.AIR)).SetTranslucent().SetAdventureModeExempt();
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

		public Material(MapColor color)
		{
		//	this.materialMapColor = color;
		}

		public virtual bool IsLiquid()
		{
			return false;
		}

		public virtual bool IsSolid()
		{
			return true;
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

		public virtual bool IsReplaceable()
		{
			return this._replaceable;
		}

		public virtual bool IsOpaque()
		{
			return !this._isTranslucent;
		}

		public virtual bool IsToolNotRequired()
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
			return MapColor.AIR;
		//	return this.materialMapColor;
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
