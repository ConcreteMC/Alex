using Alex.ResourcePackLib.Json.Bedrock.MoLang;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	/// <summary>
	///		Holds the position, rotation & scale to apply when playing the animation.
	/// </summary>
	public class AnimationBoneElement
	{
		/// <summary>
		///		The position of the bone
		/// </summary>
		[JsonProperty("position")]
		public MoLangVector3Expression Position { get; set; }

		/// <summary>
		///		The rotation to apply to the bone
		/// </summary>
		[JsonProperty("rotation")]
		public MoLangVector3Expression Rotation { get; set; }

		/// <summary>
		///		The scale of the bone.
		/// </summary>
		[JsonProperty("scale")]
		public MoLangVector3Expression Scale { get; set; }
	}
}