using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Models.Entities
{
	public partial class ModelDescription
	{
		/// <summary>
		///		Entity definition and Client Block definition files refer to this geometry via this identifier.
		/// </summary>
		[JsonProperty("identifier")]
		public string Identifier { get; set; }

		/// <summary>
		/// Assumed width in texels of the texture that will be bound to this geometry.
		/// </summary>
		[JsonProperty("texture_width")]
		public long TextureWidth { get; set; }

		/// <summary>
		/// Assumed height in texels of the texture that will be bound to this geometry.
		/// </summary>
		[JsonProperty("texture_height")]
		public long TextureHeight { get; set; }

		/// <summary>
		/// Offset of the visibility bounding box from the entity location point (in model space units).
		/// </summary>
		[JsonProperty("visible_bounds_offset", NullValueHandling = NullValueHandling.Ignore)]
		public Vector3 VisibleBoundsOffset { get; set; }

		/// <summary>
		///	 Width of the visibility bounding box (in model space units).
		/// </summary>
		[JsonProperty("visible_bounds_width")]
		public double VisibleBoundsWidth { get; set; }
	    
		/// <summary>
		/// Height of the visible bounding box (in model space units).
		/// </summary>
		[JsonProperty("visible_bounds_height")]
		public double VisibleBoundsHeight { get; set; }

		public ModelDescription Clone()
		{
			return new ModelDescription()
			{
				Identifier = Identifier,
				TextureHeight = TextureHeight,
				TextureWidth = TextureWidth,
				VisibleBoundsHeight = VisibleBoundsHeight,
				VisibleBoundsOffset = VisibleBoundsOffset,
				VisibleBoundsWidth = VisibleBoundsWidth
			};
		}
	}
}