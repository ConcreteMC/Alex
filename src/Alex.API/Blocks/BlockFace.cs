using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Alex.API.Blocks
{
	[JsonConverter(typeof(TolerantEnumConverter))]
	public enum BlockFace : int
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
    			switch (face)
    			{
    				case BlockFace.Down:
    					return Vector3.Down;
    				case BlockFace.Up:
    					return Vector3.Up;
    				case BlockFace.East:
    					return Vector3.Right;
    				case BlockFace.West:
    					return Vector3.Left;
    				case BlockFace.North:
    					return Vector3.Backward;
    				case BlockFace.South:
    					return Vector3.Forward;
    				default:
    					return Vector3.Zero;
    			}
    		}
    
    		public static BlockCoordinates GetBlockCoordinates(this BlockFace face)
    		{
    			switch (face)
    			{
    				case BlockFace.Down:
    					return BlockCoordinates.Down;
    				case BlockFace.Up:
    					return BlockCoordinates.Up;
    				case BlockFace.East:
    					return BlockCoordinates.Right;
    				case BlockFace.West:
    					return BlockCoordinates.Left;
    				case BlockFace.North:
    					return BlockCoordinates.Forwards;
    				case BlockFace.South:
    					return BlockCoordinates.Backwards;
    				default:
    					return BlockCoordinates.Zero;
    			}
    		}
    		
    		public static BlockFace Opposite(this BlockFace face)
    		{
    			switch (face)
    			{
    				case BlockFace.Down:
    					return BlockFace.Up;
    				case BlockFace.Up:
    					return BlockFace.Down;
    				case BlockFace.East:
    					return BlockFace.West;
    				case BlockFace.West:
    					return BlockFace.East;
    				case BlockFace.North:
    					return BlockFace.South;
    				case BlockFace.South:
    					return BlockFace.North;
    
    					default:
    					return BlockFace.None;
    			}
    		}
    	}
}