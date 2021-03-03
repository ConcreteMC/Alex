using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using Alex.MoLang.Parser;
using Alex.MoLang.Parser.Expressions;
using Alex.ResourcePackLib.Json.Converters;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	/// <summary>
	///		Defines a model's animations.
	/// </summary>
	public class Animation
	{
		/// <summary>
		///		Determines whether the animation should go back to T0 when finished.
		/// </summary>
		[JsonProperty("loop")]
		public bool Loop { get; set; } = false;

		/// <summary>
		///		Determines what the value of query.anim_time should be.
		///		NOT CURRENTLY IMPLEMENTED
		/// </summary>
		[JsonProperty("anim_time_update")]
		public List<IExpression> AnimationTimeUpdate { get; set; } = null;

		/// <summary>
		///		How much should this animation be blended in with the others?
		///		0.0 = off.  1.0 = fully apply all transforms.
		/// </summary>
		[JsonProperty("blend_weight")]
		public List<IExpression> BlendWeight { get; set; } = new List<IExpression>
		{
			new NumberExpression(1d)
		};
		
		/// <summary>
		///		At what time does the system consider this animation finished
		/// </summary>
		[JsonProperty("animation_length")]
		public float AnimationLength { get; set; } = 0f;

		/// <summary>
		///		Should this animation override the rotation etc?
		///		If set to true, all animations applied before this one don't have any effect.
		/// </summary>
		[JsonProperty("override_previous_animation")]
		public bool OverridePreviousAnimation { get; set; } = false;
		
		/// <summary>
		///		Hold all the actual animation values for animated bones.
		///		The key has to match the name of the bone as specified in the geometry.
		/// </summary>
		[JsonProperty("bones")]
		public Dictionary<string, AnimationBoneElement> Bones { get; set; } =
			new Dictionary<string, AnimationBoneElement>();
	}
}