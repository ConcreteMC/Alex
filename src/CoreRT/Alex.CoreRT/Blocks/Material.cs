using Alex.CoreRT.Utils;

namespace Alex.CoreRT.Blocks
{
    public class Material
    {
		public static Material Air = new MaterialTransparent(MapColor.AIR);
		public static Material Grass = new Material(MapColor.GRASS);
		public static Material Ground = new Material(MapColor.DIRT);
		public static Material Wood = (new Material(MapColor.WOOD)).SetBurning();
		public static Material Rock = (new Material(MapColor.STONE)).SetRequiresTool();
		public static Material Iron = (new Material(MapColor.IRON)).SetRequiresTool();
		public static Material Anvil = (new Material(MapColor.IRON)).SetRequiresTool().SetImmovableMobility();
		public static Material Water = (new MaterialLiquid(MapColor.WATER)).SetNoPushMobility();
		public static Material Lava = (new MaterialLiquid(MapColor.TNT)).SetNoPushMobility();
		public static Material Leaves = (new Material(MapColor.FOLIAGE)).SetBurning().SetTranslucent().SetNoPushMobility();
		public static Material Plants = (new MaterialLogic(MapColor.FOLIAGE)).SetNoPushMobility();
		public static Material Vine = (new MaterialLogic(MapColor.FOLIAGE)).SetBurning().SetNoPushMobility().SetReplaceable();
		public static Material Sponge = new Material(MapColor.YELLOW);
		public static Material Cloth = (new Material(MapColor.CLOTH)).SetBurning();
		public static Material Fire = (new MaterialTransparent(MapColor.AIR)).SetNoPushMobility();
		public static Material Sand = new Material(MapColor.SAND);
		public static Material Circuits = (new MaterialLogic(MapColor.AIR)).SetNoPushMobility();
		public static Material Carpet = (new MaterialLogic(MapColor.CLOTH)).SetBurning();
		public static Material Glass = (new Material(MapColor.AIR)).SetTranslucent().SetAdventureModeExempt();
		public static Material RedstoneLight = (new Material(MapColor.AIR)).SetAdventureModeExempt();
		public static Material Tnt = (new Material(MapColor.TNT)).SetBurning().SetTranslucent();
		public static Material Coral = (new Material(MapColor.FOLIAGE)).SetNoPushMobility();
		public static Material Ice = (new Material(MapColor.ICE)).SetTranslucent().SetAdventureModeExempt();
		public static Material PackedIce = (new Material(MapColor.ICE)).SetAdventureModeExempt();
		public static Material Snow = (new MaterialLogic(MapColor.SNOW)).SetReplaceable().SetTranslucent().SetRequiresTool().SetNoPushMobility();
		public static Material CraftedSnow = (new Material(MapColor.SNOW)).SetRequiresTool();
		public static Material Cactus = (new Material(MapColor.FOLIAGE)).SetTranslucent().SetNoPushMobility();
		public static Material Clay = new Material(MapColor.CLAY);
		public static Material Gourd = (new Material(MapColor.FOLIAGE)).SetNoPushMobility();
		public static Material DragonEgg = (new Material(MapColor.FOLIAGE)).SetNoPushMobility();
		public static Material Portal = (new MaterialPortal(MapColor.AIR)).SetImmovableMobility();
		public static Material Cake = (new Material(MapColor.AIR)).SetNoPushMobility();
		public static Material Web = (new Material(MapColor.CLOTH)).SetRequiresTool().SetNoPushMobility();

		public static Material Piston = (new Material(MapColor.STONE)).SetImmovableMobility();
		public static Material Barrier = (new Material(MapColor.AIR)).SetRequiresTool().SetImmovableMobility();
		public static Material StructureVoid = new MaterialTransparent(MapColor.AIR);

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

		private Material SetTranslucent()
		{
			this._isTranslucent = true;
			return this;
		}

		protected Material SetRequiresTool()
		{
			this._requiresNoTool = false;
			return this;
		}

		protected Material SetBurning()
		{
			this._canBurn = true;
			return this;
		}

		public virtual bool GetCanBurn()
		{
			return this._canBurn;
		}

		public Material SetReplaceable()
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
			return this._isTranslucent ? false : this.BlocksMovement();
		}

		public virtual bool IsToolNotRequired()
		{
			return this._requiresNoTool;
		}

		protected Material SetNoPushMobility()
		{
		//	this.mobilityFlag = EnumPushReaction.DESTROY;
			return this;
		}

		protected Material SetImmovableMobility()
		{
		//	this.mobilityFlag = EnumPushReaction.BLOCK;
			return this;
		}

		protected Material SetAdventureModeExempt()
		{
			this._isAdventureModeExempt = true;
			return this;
		}

		public virtual MapColor GetMaterialMapColor()
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
