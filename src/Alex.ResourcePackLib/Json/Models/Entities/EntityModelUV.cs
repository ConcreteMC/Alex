using System;
using Alex.API.Blocks;
using Alex.ResourcePackLib.Json.Converters;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Models.Entities
{
	public class EntityModelUV
	{
		[JsonProperty("north")]
		public EntityModelUVData North { get; set; }
		
		[JsonProperty("south")]
		public EntityModelUVData South { get; set; }
		
		[JsonProperty("east")]
		public EntityModelUVData East  { get; set; }
		
		[JsonProperty("west")]
		public EntityModelUVData West  { get; set; }
		
		[JsonProperty("up")]
		public EntityModelUVData Up    { get; set; }
		
		[JsonProperty("down")]
		public EntityModelUVData Down  { get; set; }

		public EntityModelUV() : this(Vector2.Zero, false)
		{
			
		}
		
		[JsonIgnore]
		public bool IsCube { get; }
		public EntityModelUV(Vector2 origin, bool isCube = true)
		{
			IsCube = isCube;
			var model = new EntityModelUVData() {Origin = origin};
			Up = Down = North = West = South = East = model;
		}

		public EntityModelUVData GetFace(BlockFace face)
		{
			switch (face)
			{
				case BlockFace.Down:
					return Down;
					break;

				case BlockFace.Up:
					return Up;
					break;

				case BlockFace.East:
					return East;
					break;

				case BlockFace.West:
					return West;
					break;

				case BlockFace.North:
					return North;
					break;

				case BlockFace.South:
					return South;
					break;
			}

			return null;
		}

		public static implicit operator EntityModelUV(Vector2 vector)
		{
			return new EntityModelUV(vector);
		}

		public bool IsOutOfBound(Vector2 textureSize)
		{
			if (IsCube)
			{
				return (Down.Origin.Y >= textureSize.Y);
			}
			
			foreach (BlockFace face in Enum.GetValues<BlockFace>())
			{
				var f = GetFace(face);

				if (f.Origin.Y >= textureSize.Y)
					return true;
			}

			return false;
		}
	}

	public class EntityModelUVData
	{
		/// <summary>
		/// Specifies the uv origin for the face. For this face, it is the upper-left corner, when looking at the face with y being up.
		/// </summary>
		[JsonProperty("uv")]
		public Vector2 Origin { get; set; }

		/// <summary>
		/// The face maps this many texels from the uv origin. If not specified, the box dimensions are used instead.
		/// </summary>
		[JsonProperty("uv_size"), JsonConverter(typeof(Vector2NullableConverter))]
		public Vector2? Size { get; set; } = null;

		public EntityModelUVData Offset(Vector2 amount)
		{
			return new EntityModelUVData() {Origin = Origin + amount, Size = Size};
		}
		
		public EntityModelUVData WithSize(Vector2 size)
		{
			return new EntityModelUVData() {Origin = Origin, Size = size};
		}
		
		public EntityModelUVData WithSize(float x, float y)
		{
			return new EntityModelUVData() {Origin = Origin, Size = new Vector2(x, y)};
		}
	}
}