using Alex.Interfaces.Converters;
using Newtonsoft.Json;

namespace Alex.Interfaces
{
	[JsonConverter(typeof(TolerantEnumConverter))]
	public enum BlockFace : byte
	{
		Down = 0,
		Up = 1,
		East = 2,
		West = 3,
		North = 4,
		South = 5,
		None = 255
	}
	
	public static class BlockFaceHelper
	{
		public static BlockFace Opposite(this BlockFace face)
		{
			return face switch
			{
				BlockFace.Down  => BlockFace.Up,
				BlockFace.Up    => BlockFace.Down,
				BlockFace.East  => BlockFace.West,
				BlockFace.West  => BlockFace.East,
				BlockFace.North => BlockFace.South,
				BlockFace.South => BlockFace.North,
				_               => BlockFace.None
			};
		}
	}
}