using Alex.Worlds;

namespace Alex.Entities.Vehicles
{
	public class MinecartEntity : VehicleEntity
	{
		/// <inheritdoc />
		public MinecartEntity(World level) : base(level) { }
	}


	public class TntMinecartEntity : MinecartEntity
	{
		/// <inheritdoc />
		public TntMinecartEntity(World level) : base(level) { }
	}

	public class ChestMinecartEntity : MinecartEntity
	{
		/// <inheritdoc />
		public ChestMinecartEntity(World level) : base(level) { }
	}

	public class HopperMinecartEntity : MinecartEntity
	{
		/// <inheritdoc />
		public HopperMinecartEntity(World level) : base(level) { }
	}
}