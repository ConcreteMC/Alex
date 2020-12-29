using Alex.API.Utils;
using Alex.Graphics.Models.Entity;
using Alex.Networking.Java.Packets.Play;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Worlds;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Alex.Entities.Hostile
{
	public class Creeper : HostileMob
	{
		private bool _isCharged = false;

		public bool IsCharged
		{
			get
			{
				return _isCharged;
			}
			set
			{
				_isCharged = value;

				Level.BackgroundWorker.Enqueue(
					() =>
					{
						Image<Rgba32> texture        = null;
						EntityModel   normal         = null;
						EntityModel   charged        = null;
						string        defaultTexture = null;
						string        chargedTexture = null;

						if (Alex.Instance.Resources.BedrockResourcePack.EntityDefinitions.TryGetValue(
							"minecraft:creeper", out var entityDescription))
						{
							if (entityDescription.Textures.TryGetValue("default", out defaultTexture)
							    && entityDescription.Geometry.TryGetValue("default", out var geometryName))
							{
								if (Alex.Instance.Resources.BedrockResourcePack.TryGetTexture(
									defaultTexture, out var newTexture))
								{
									texture = newTexture;
								}

								if (ModelFactory.TryGetModel(geometryName, out var newModel))
								{
									normal = newModel;
								}
							}

							if (entityDescription.Textures.TryGetValue("charged", out chargedTexture)
							    && entityDescription.Geometry.TryGetValue("charged", out var chargedGeoName))
							{
								if (Alex.Instance.Resources.BedrockResourcePack.TryGetTexture(
									chargedTexture, out var newTexture))
								{
									//if (texture == null)
									{
										texture = newTexture;
									}
								//	else
									{
								//		texture = texture.Clone();
								//		texture.Mutate(m => m.DrawImage(newTexture, PixelColorBlendingMode.Normal, 1f));
									}
								}

								if (ModelFactory.TryGetModel(chargedGeoName, out var newModel))
								{
									charged = newModel;
								}
							}
						}

						if (_isCharged)
						{
							if (normal == null)
							{
								normal = charged;
							}
							else
							{
								normal = charged + normal;
							}
						}

						if (normal != null && texture != null)
						{
							ModelRenderer = new EntityModelRenderer(
								normal, TextureUtils.BitmapToTexture2D(Alex.Instance.GraphicsDevice, texture));
						}
					});

				//TryUpdateGeometry(
				//	"minecraft:creeper", value ? "charged" : "default", value ? "charged" : "default");
			}
		}

		public Creeper(World level) : base((EntityType)33, level)
		{
			Height = 1.7;
			Width = 0.6;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 16 && entry is MetadataBool charged)
			{
				IsCharged = charged.Value;
			}
			else if (entry.Index == 17 && entry is MetadataBool ignited)
			{
				IsIgnited = ignited.Value;
			}
		}
	}
}
