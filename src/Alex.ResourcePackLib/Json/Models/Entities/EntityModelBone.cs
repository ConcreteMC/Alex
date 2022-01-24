using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Models.Entities
{
	using J = Newtonsoft.Json.JsonPropertyAttribute;
	using R = Newtonsoft.Json.Required;
	using N = Newtonsoft.Json.NullValueHandling;

	public class EntityModelBone
	{
		/// <summary>
		/// Animation files refer to this bone via this identifier.
		/// </summary>
		[J("name")]
		public string Name { get; set; }

		/// <summary>
		/// Bone that this bone is relative to.  If the parent bone moves, this bone will move along with it.
		/// </summary>
		[J("parent", NullValueHandling = N.Ignore)]
		public string Parent { get; set; } = null;

		[J("locators", NullValueHandling = N.Ignore)]
		public EntityModelLocators Locators { get; set; } = null;

		/// <summary>
		/// Grow this box by this additive amount in all directions (in model space units)
		/// </summary>
		[J("inflate", NullValueHandling = N.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate),
		 DefaultValue(0d)]
		public double Inflate { get; set; } = 0;

		/// <summary>
		/// The bone pivots around this point (in model space units).
		/// </summary>
		[J("pivot", NullValueHandling = N.Ignore)]
		public Vector3? Pivot { get; set; }

		/// <summary>
		/// This is the initial rotation of the bone around the pivot, pre-animation (in degrees, x-then-y-then-z order).
		/// </summary>
		[J("rotation", NullValueHandling = N.Ignore)]
		public Vector3? Rotation { get; set; } = null;

		/// <summary>
		///		The rotation for the bone (1.8.0 geometry only)
		/// </summary>
		[J("bind_pose_rotation", NullValueHandling = N.Ignore)]
		public Vector3? BindPoseRotation { get; set; }

		[J("neverRender", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate), DefaultValue(false)]
		public bool NeverRender { get; set; } = false;

		/// <summary>
		/// Mirrors the UV's of the unrotated cubes along the x axis, also causes the east/west faces to get flipped.
		/// </summary>
		[JsonProperty("mirror", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate), DefaultValue(false)]
		public bool Mirror { get; set; } = false;

		[JsonProperty("reset", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate), DefaultValue(false)]
		public bool Reset { get; set; } = false;

		/// <summary>
		/// This is the list of cubes associated with this bone.
		/// </summary>
		[J("cubes", NullValueHandling = N.Ignore)]
		public EntityModelCube[] Cubes { get; set; }

		public EntityModelBone Clone()
		{
			return new EntityModelBone()
			{
				Inflate = Inflate,
				Mirror = Mirror,
				Pivot = Pivot,
				Rotation = Rotation,
				Locators = Locators,
				Cubes = Cubes?.Select(x => x.Clone()).ToArray(),
				Name = Name,
				Parent = Parent,
				Reset = Reset,
				NeverRender = NeverRender,
				BindPoseRotation = BindPoseRotation
			};
		}
	}

	public sealed class EntityModelLocators
	{
		[J("lead", NullValueHandling = N.Ignore)]
		public Vector3 Lead { get; set; }
	}
}