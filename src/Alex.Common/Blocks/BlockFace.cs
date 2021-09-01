using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Alex.Common.Blocks
{
	[JsonConverter(typeof(TolerantEnumConverter))]
	public enum BlockFace : byte
	{
		Down  = 0,
		Up    = 1,
		East  = 2,
		West  = 3,
		North = 4,
		South = 5,
		None  = 255
	}

	public static class BlockFaceHelper
    	{
    		public static Vector3 GetVector3(this BlockFace face)
            {
	            return face switch
	            {
		            BlockFace.Down  => Vector3.Down,
		            BlockFace.Up    => Vector3.Up,
		            BlockFace.East  => Vector3.Left,
		            BlockFace.West  => Vector3.Right,
		            BlockFace.North => Vector3.Forward,
		            BlockFace.South => Vector3.Backward,
		            _               => Vector3.Zero
	            };
            }
    
    		public static BlockCoordinates GetBlockCoordinates(this BlockFace face)
            {
	            return face switch
	            {
		            BlockFace.Down  => BlockCoordinates.Down,
		            BlockFace.Up    => BlockCoordinates.Up,
		            BlockFace.East  => BlockCoordinates.East,
		            BlockFace.West  => BlockCoordinates.West,
		            BlockFace.North => BlockCoordinates.North,
		            BlockFace.South => BlockCoordinates.South,
		            _               => BlockCoordinates.Zero
	            };
            }
    		
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