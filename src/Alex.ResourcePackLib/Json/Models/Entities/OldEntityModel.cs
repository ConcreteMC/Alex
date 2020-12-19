using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Converters;
using Microsoft.Xna.Framework;
using MiNET.Utils.Skins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Alex.ResourcePackLib.Json.Models.Entities
{
	using J = Newtonsoft.Json.JsonPropertyAttribute;
	using R = Newtonsoft.Json.Required;
	using N = Newtonsoft.Json.NullValueHandling;

	public class OldEntityModel : EntityModel
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

		public string Name
		{
			get
			{
				return Description.Identifier;
			}
			set
			{
				Description.Identifier = value;
			}
		}

		[J("visible_bounds_width", NullValueHandling = N.Ignore)]
		public double VisibleBoundsWidth
		{
			get
			{
				return Description.VisibleBoundsWidth;
			}
			set
			{
				Description.VisibleBoundsWidth = value;
			}
		}

		[J("visible_bounds_height", NullValueHandling = N.Ignore)]
		public double VisibleBoundsHeight
		{
			get
			{
				return Description.VisibleBoundsHeight;
			}
			set
			{
				Description.VisibleBoundsHeight = value;
			}
		}

		[J("visible_bounds_offset", NullValueHandling = N.Ignore)]
		public Vector3 VisibleBoundsOffset
		{
			get
			{
				return Description.VisibleBoundsOffset;
			}
			set
			{
				Description.VisibleBoundsOffset = value;
			}
		}

		[J("texturewidth", NullValueHandling = N.Ignore)]
		public long Texturewidth
		{
			get
			{
				return Description.TextureWidth;
			}
			set
			{
				Description.TextureWidth = value;
			}
		}

		[J("textureheight", NullValueHandling = N.Ignore)]
		public long Textureheight
		{
			get
			{
				return Description.TextureHeight;
			}
			set
			{
				Description.TextureHeight = value;
			}
		}

		public OldEntityModel()
		{
			Description = new ModelDescription();
		}
	}
}
