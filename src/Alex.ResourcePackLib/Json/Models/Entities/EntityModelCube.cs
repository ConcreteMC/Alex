using Alex.Interfaces;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Models.Entities
{
	using J = Newtonsoft.Json.JsonPropertyAttribute;
	using R = Newtonsoft.Json.Required;
	using N = Newtonsoft.Json.NullValueHandling;

	public sealed class EntityModelCube
	{
		/// <summary>
		/// This point declares the unrotated lower corner of cube (smallest x/y/z value in model space units).
		/// </summary>
		[J("origin")]
		public IVector3 Origin { get; set; }

		/// <summary>
		/// If this field is specified, rotation of this cube occurs around this point, otherwise its rotation is around the center of the box.
		/// Note that in 1.12 this is flipped upside-down, but is fixed in 1.14.
		/// </summary>
		[J("pivot", NullValueHandling = N.Ignore)]
		public IVector3 Pivot { get; set; }

		/// <summary>
		/// The cube is rotated by this amount (in degrees, x-then-y-then-z order) around the pivot.
		/// </summary>
		[J("rotation", NullValueHandling = N.Ignore)]
		public IVector3 Rotation { get; set; }

		/// <summary>
		/// The cube extends this amount relative to its origin (in model space units).
		/// </summary>
		[J("size")]
		public IVector3 Size { get; set; }

		/// <summary>
		///		The uv mapping for this model.
		/// </summary>
		[J("uv")]
		public EntityModelUV Uv { get; set; }

		/// <summary>
		/// Mirrors this cube about the unrotated x axis (effectively flipping the east / west faces), overriding the bone's 'mirror' setting for this cube.
		/// </summary>
		[J("mirror", NullValueHandling = N.Ignore)]
		public bool? Mirror { get; set; }

		/// <summary>
		/// Grow this box by this additive amount in all directions (in model space units), this field overrides the bone's inflate field for this cube only.
		/// </summary>
		[J("inflate", NullValueHandling = N.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public double? Inflate { get; set; }

		//	[JsonIgnore]
		public IVector3 InflatedSize(float amount)
		{
			var inflation = amount;
			
			if (amount == 0f)
				return Size;

			var x = Size.X;
			var y = Size.Y;
			var z = Size.Z;
			
			if (x < 0) x -= inflation;
			else x += inflation;

			if (y < 0) y -= inflation;
			else y += inflation;

			if (z < 0)z -= inflation;
			else z += inflation;

			return Primitives.Factory.Vector3(x,y,z);
		}

		//  [JsonIgnore]
		public IVector3 InflatedOrigin(float amount)
		{
			var origin = Origin;

			if (amount == 0f)
				return origin;

			//  return Origin;
			// var origin = Origin;// * new Vector3(1f, 1f, -1f);
			//  var s         = InflatedSize(amount);
			var inflation = (amount / 2f);

			var x = origin.X;
			var y = origin.Y;
			var z = origin.Z;
			if (amount > 0f)
			{
				x -= inflation;

				y -= inflation;

				z -= inflation;
			}
			else
			{
				x += inflation;

				y += inflation;

				z += inflation;
			}

			// var origin         = new Vector3(Origin.X + inflation, Origin.Y + inflation, Origin.Z + inflation);

			// return origin;

			return Primitives.Factory.Vector3(x,y,z);
		}

		public IVector3 InflatedPivot(float amount)
		{
			if (amount == 0f)
				return Pivot;

			// var origin = Origin;// * new Vector3(1f, 1f, -1f);
			//  var s         = InflatedSize(amount);
			var inflation = (amount / 2f);

			//  if (amount > 0f)
			//	  inflation = -inflation;

			var p = Pivot;
			var pivot = Primitives.Factory.Vector3(p.X - inflation, p.Y - inflation, p.Z - inflation);

			return pivot;
		}

		public EntityModelCube Clone()
		{
			return new EntityModelCube()
			{
				Inflate = Inflate,
				Mirror = Mirror,
				Origin = Origin,
				Pivot = Pivot,
				Rotation = Rotation,
				Size = Size,
				Uv = Uv
			};
		}
	}
}