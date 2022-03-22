using System;
using System.Collections.Generic;
using ConcreteMC.MolangSharp.Parser;
using ConcreteMC.MolangSharp.Parser.Expressions;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	public class LoopElement
	{
		public bool IsString { get; set; }
		public bool IsExpression { get; set; }
	}
	/// <summary>
	///		Defines a model's animations.
	/// </summary>
	public class Animation
	{
		public static IExpression DefaultTimeUpdate { get; }

		static Animation()
		{
			DefaultTimeUpdate = MoLangParser.Parse("query.anim_time + query.delta_time");
		}

		/// <summary>
		///		Determines whether the animation should go back to T0 when finished.
		/// </summary>
		[JsonProperty("loop")]
		public string Loop { get; set; } 

		/// <summary>
		///		 How long to wait in seconds before playing this animation.
		///		Note that this expression is evaluated once before playing, and only re-evaluated if asked to play from the beginning again.
		///		A looping animation should use 'loop_delay' if it wants a delay between loops.
		/// </summary>
		[JsonProperty("start_delay")]
		public IExpression StartDelay { get; set; }
		
		/// <summary>
		///		How long to wait in seconds before looping this animation.
		///		Note that this expression is evaluated after each loop and on looping animation only.
		/// </summary>
		[JsonProperty("loop_delay")]
		public IExpression LoopDelay { get; set; }
		
		/// <summary>
		///		How does time pass when playing the animation.
		///     Defaults to "query.anim_time + query.delta_time" which means advance in seconds.
		/// </summary>
		[JsonProperty("anim_time_update")]
		public IExpression AnimationTimeUpdate { get; set; } = null;

		/// <summary>
		///		How much should this animation be blended in with the others?
		///		0.0 = off.  1.0 = fully apply all transforms.
		/// </summary>
		[JsonProperty("blend_weight")]
		public IExpression BlendWeight { get; set; } = new NumberExpression(1d);

		/// <summary>
		///		At what time does the system consider this animation finished
		/// </summary>
		[JsonProperty("animation_length")]
		public float AnimationLength { get; set; } = 0f;

		/// <summary>
		///	 reset bones in this animation to the default pose before applying this animation
		/// </summary>
		[JsonProperty("override_previous_animation")]
		public bool OverridePreviousAnimation { get; set; } = false;

		/// <summary>
		///		Hold all the actual animation values for animated bones.
		///		The key has to match the name of the bone as specified in the geometry.
		/// </summary>
		[JsonProperty("bones")]
		public Dictionary<string, AnimationBoneElement> Bones { get; set; } =
			new Dictionary<string, AnimationBoneElement>(StringComparer.Ordinal);
	}
}