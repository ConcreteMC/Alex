using System;
using Alex.ResourcePackLib.Json.Converters;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Textures
{
	public struct TextureInfoJson
	{
		[JsonProperty("nineslice_size")]
		[JsonConverter(typeof(SingleOrArrayConverter<int>))]
		public int[] NineSliceSize { get; set; }

		[JsonProperty("base_size")]
		[JsonConverter(typeof(SingleOrArrayConverter<int>))]
		public int[] BaseSize { get; set; }

		public Rectangle InnerBounds
		{
			get
			{
				var bounds = Bounds;
				if (NineSliceSize != null)
				{
					if (NineSliceSize.Length == 4)
					{
						return new Rectangle(NineSliceSize[0], NineSliceSize[1],
							bounds.Width - NineSliceSize[0]  - NineSliceSize[2],
							bounds.Height - NineSliceSize[1] - NineSliceSize[3]);
					}
					else if (NineSliceSize.Length == 2)
					{
						return new Rectangle(NineSliceSize[0], NineSliceSize[1], bounds.Width - 2 * NineSliceSize[0],
							bounds.Height                                                     - 2 * NineSliceSize[1]);
					}
					else if (NineSliceSize.Length == 1)
					{
						return new Rectangle(NineSliceSize[0], NineSliceSize[0], bounds.Width - 2 * NineSliceSize[0],
							bounds.Height                                                     - 2 * NineSliceSize[0]);
					}

					throw new ArgumentOutOfRangeException(nameof(NineSliceSize.Length), NineSliceSize.Length,
						$"NineSiceSize value {NineSliceSize.Length} not handled");
				}

				return bounds;
			}
		}

		public Rectangle Bounds
		{
			get
			{
				if (BaseSize != null && BaseSize.Length == 2)
				{
					return new Rectangle(0, 0, BaseSize[0], BaseSize[1]);
				}

				return new Rectangle();
			}
		}
	}
}