using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Alex.API.Graphics;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.Entities.Models;
using Alex.Entities.Properties;
using Alex.Graphics.Models.Entity;
using Alex.Net;
using Alex.Networking.Java.Packets.Play;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Converters;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Alex.Worlds;
using Alex.Worlds.Multiplayer.Bedrock;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET;
using Newtonsoft.Json;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using LogManager = NLog.LogManager;
using MathF = Alex.API.Utils.MathF;
using Point = System.Drawing.Point;
using Skin = MiNET.Utils.Skins.Skin;

namespace Alex.Entities
{
	public class RemotePlayer : LivingEntity
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(RemotePlayer));
		
		public UUID Uuid { get; private set; }

		public Gamemode Gamemode { get; private set; }

		private EntityModel _model;

		public string Name { get; }
		public string GeometryName { get; set; }
		
		public PlayerSkinFlags SkinFlags { get; }

		public int Score { get; set; } = 0;
		
		public RemotePlayer(string name, World level, NetworkProvider network, PooledTexture2D skinTexture, string geometry = "geometry.humanoid.customSlim") : base(63, level, network)
		{
			SkinFlags = new PlayerSkinFlags()
			{
				Value = 0xff
			};
			
			//DoRotationCalculations = false;
			Name = name;
			Uuid = new UUID(Guid.NewGuid().ToByteArray());

			Width = 0.6;
			//Length = 0.6;
			Height = 1.80;

			IsSpawned = false;

			NameTag = name;
			//Skin = new Skin { Slim = false, SkinData = Encoding.Default.GetBytes(new string('Z', 8192)) };

		//	ItemInHand = new ItemAir();

			HideNameTag = false;
			IsAlwaysShowName = true;
			ShowItemInHand = true;
			
			IsInWater = false;
			NoAi = true;
			//HealthManager.IsOnFire = false;
			Velocity = Vector3.Zero;
			PositionOffset = 1.62f;

			GeometryName = geometry;
			
			UpdateSkin(skinTexture);
			
			//Inventory = new Inventory(46);
			
			MovementSpeed = 0.1f;
			FlyingSpeed = 0.4f;
		}

		private static JsonSerializerSettings GeometrySerializationSettings = new JsonSerializerSettings()
		{
			Converters = new List<JsonConverter>()
			{
				new SingleOrArrayConverter<Vector3>(),
				new SingleOrArrayConverter<Vector2>(),
				new Vector3Converter(),
				new Vector2Converter()
			},
			MissingMemberHandling = MissingMemberHandling.Ignore
		};
		
		public void LoadSkin(Skin skin)
		{
			//PooledTexture2D skinTexture = null;
			Image<Rgba32>   skinBitmap  = null;
			if (!skin.TryGetBitmap(out skinBitmap))
			{
				Log.Warn($"No custom skin data for player {Name}");
						
				if (Alex.Instance.Resources.ResourcePack.TryGetBitmap("entity/alex", out var rawTexture))
				{
					skinBitmap = rawTexture;
					//skinTexture = TextureUtils.BitmapToTexture2D(AlexInstance.GraphicsDevice, rawTexture);
				}
			}
			
			EntityModel model = null;

			if (!skin.IsPersonaSkin)
			{
				if (!string.IsNullOrWhiteSpace(skin.GeometryData) && skin.GeometryData != "null")
				{
					try
					{
						if (string.IsNullOrWhiteSpace(skin.ResourcePatch) || skin.ResourcePatch == "null")
						{
							Log.Warn($"Resourcepatch null for player {Name}");
						}
						else
						{
							var resourcePatch = JsonConvert.DeserializeObject<SkinResourcePatch>(
								skin.ResourcePatch, GeometrySerializationSettings);
							
							GeometryModel geometryModel = null;
							if (!GeometryModel.TryParse(skin.GeometryData, resourcePatch, out geometryModel))
							{
								Log.Warn($"Failed to parse geometry for player {Name}");
							}
							
							if (geometryModel == null || geometryModel.Geometry.Count == 0)
							{
								Log.Warn($"!! Model count was 0 for player {Name} !!");
							}
							else
							{
								/*if (!string.IsNullOrWhiteSpace(Name) && geometryModel.Geometry.Any(
									x => x.Description.TextureHeight == 32 && x.Description.TextureWidth == 64
									                                       && !x.Description.Identifier.Contains(
										                                       "geometry.cape",
										                                       StringComparison
											                                      .InvariantCultureIgnoreCase)))
								{
									string geometryFileName = $"geometry.{Name}.json";
									geometryFileName = Path.Combine("/home/kenny/skins/unique", geometryFileName);

									if (!File.Exists(geometryFileName))
										File.WriteAllText(
											geometryFileName, MCJsonConvert.SerializeObject(geometryModel, true));

									geometryFileName = $"resource.{Name}.json";
									geometryFileName = Path.Combine("/home/kenny/skins/unique", geometryFileName);

									if (!File.Exists(geometryFileName))
										File.WriteAllText(
											geometryFileName, MCJsonConvert.SerializeObject(resourcePatch, true));
								}*/
							

								if (resourcePatch?.Geometry != null)
								{
									model = geometryModel.FindGeometry(resourcePatch.Geometry.Default);

									if (model == null)
									{
										Log.Warn(
											$"Invalid geometry: {resourcePatch.Geometry.Default} for player {Name}");
									}
									else
									{
										var modelTextureSize = new Point((int) model.Description.TextureWidth, (int) model.Description.TextureHeight);
										
										var textureSize = new Point(skinBitmap.Width, skinBitmap.Height);

										if (modelTextureSize != textureSize)
										{
											int newHeight = modelTextureSize.Y > textureSize.Y ? textureSize.Y : modelTextureSize.Y;
											int newWidth = modelTextureSize.X > textureSize.X ? textureSize.X: modelTextureSize.X;
					
											var bitmap = skinBitmap;
											bitmap.Mutate<Rgba32>(xx => xx.Resize(modelTextureSize.X, modelTextureSize.Y));

											skinBitmap = bitmap;
											//Image<Rgba32> skinTexture = new Image<Rgba32>(modelTextureSize.X, modelTextureSize.Y);

											/*skinTexture.Mutate<Rgba32>(
												c =>
												{
													c.DrawImage(bitmap, new SixLabors.ImageSharp.Point(0, 0), 1f);
												});*/

											//skinBitmap = skinTexture;
										}
									}
								}
								else
								{
									Log.Warn($"Resourcepatch geometry was null for player {Name}");
								}
							}
						}
					}
					catch (Exception ex)
					{
						string name = "N/A";
						Log.Warn(ex, $"Could not create geometry ({name}): {ex.ToString()} for player {Name}");
						
					//	File.WriteAllBytes(Path.Combine("/home/kenny/.config/Alex/skinDebug/failed", $"failed-{Environment.TickCount}.json"), Encoding.UTF8.GetBytes(skin.GeometryData));
					}
				}
				else
				{
					Log.Warn($"Geometry data null for player {Name}");
				}
			}

			if (model == null)
			{
				model = skin.Slim ? (EntityModel) new Models.HumanoidCustomslimModel() :
					(EntityModel) new HumanoidModel();// new Models.HumanoidCustomGeometryHumanoidModel();
			}

			if (model != null && ValidateModel(model, Name))
			{
				var modelRenderer = new EntityModelRenderer(model, TextureUtils.BitmapToTexture2D(Alex.Instance.GraphicsDevice, skinBitmap));

				if (modelRenderer.Valid)
				{
					ModelRenderer = modelRenderer;
				}
				else
				{
					modelRenderer.Dispose();
					Log.Warn($"Invalid model: for player {Name} (Disposing)");
				}
			}
			else
			{
				Log.Warn($"Invalid model for player {Name}");
			}
		}
		
		private bool ValidateModel(EntityModel model, string playername)
		{
			bool valid = true;

			if (model.Bones == null || model.Bones.Length == 0)
			{
				valid = false;
				Log.Warn($"Missing bones for player model for player: {playername}");
			}
			else
			{
				foreach (var bone in model.Bones)
				{
					if (bone == null)
					{
						Log.Warn($"Found null bone in {playername}'s geometry.");
						continue;
					}

					if (!bone.NeverRender && (bone.Cubes == null || bone.Cubes.Length == 0))
					{
						//	Log.Warn($"Invalid bone, missing cube definitions for player {playername} (Bone: {bone.Name})");
					}

					if (bone.Cubes != null)
					{
						foreach (var cube in bone.Cubes)
						{
						}
					}
				}
			}

			return valid;
		}

		/// <inheritdoc />
		public override bool NoAi {
			get
			{
				return true;
			}
			set
			{
				
			} 
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 14 && entry is MetadataFloat flt)
			{
				//Additional hearts
				//HealthManager.MaxHealth 
			}
			else if (entry.Index == 15 && entry is MetadataVarInt score)
			{
				Score = score.Value;
			}
			else if (entry.Index == 16 && entry is MetadataByte data)
			{
				SkinFlags.Value = data.Value;

				if (ModelRenderer != null)
				{
					SkinFlags.ApplyTo(ModelRenderer);
				}
			}
			else if (entry.Index == 17 && entry is MetadataByte metaByte)
			{
				IsLeftHanded = metaByte.Value == 0;
			}
		}

		public void UpdateGamemode(Gamemode gamemode)
		{
			Gamemode = gamemode;
		}

		public override void Update(IUpdateArgs args)
		{
			base.Update(args);
			//	DistanceMoved = 0f;
		}

		/// <inheritdoc />
		protected override void OnModelUpdated()
		{
			base.OnModelUpdated();

			var modelRenderer = ModelRenderer;
			if (modelRenderer != null && SkinFlags != null)
				SkinFlags.ApplyTo(modelRenderer);
		}

		private bool ValidModel { get; set; }
		internal void UpdateSkin(PooledTexture2D skinTexture)
		{
			if (skinTexture == null)
			{
				skinTexture = TextureUtils.BitmapToTexture2D(Alex.Instance.GraphicsDevice, Alex.PlayerTexture);
			}
			
			//if (skinSlim)
			{
				var gotModel = ModelFactory.TryGetModel(GeometryName,
					out EntityModel m);
				
				ValidModel = gotModel;
				if (gotModel || ModelFactory.TryGetModel("geometry.humanoid.customSlim", out m))
				{
					if (!gotModel)
					{
						
					}
					_model = m;
					ModelRenderer = new EntityModelRenderer(_model, skinTexture);
					//UpdateModelParts();
				}
			}
			/*else
			{
				if (ModelFactory.TryGetModel("geometry.humanoid.custom",
					out EntityModel m))
				{
					_model = m;
					ModelRenderer = new EntityModelRenderer(_model, skinTexture);
					UpdateModelParts();
				}
			}*/
		}

		/*public override string ToString()
		{
			return
				$"Valid: {ModelRenderer.Valid} | {ModelRenderer.Texture.Height} x {ModelRenderer.Texture.Width} | Height: {Height} | Model: {GeometryName} -- {ValidModel}";
		}
	*/
	}

	public class PlayerSkinFlags
	{
		public byte Value { get; set; } = 0;

		public bool CapeEnabled => (Value & 0x01) != 0;
		public bool JacketEnabled => (Value & 0x02) != 0;
		public bool LeftSleeveEnabled => (Value & 0x04) != 0;
		public bool RightSleeveEnabled => (Value & 0x08) != 0;
		public bool LeftPantsEnabled => (Value & 0x10) != 0;
		public bool RightPantsEnabled => (Value & 0x20) != 0;
		public bool HatEnabled => (Value & 0x40) != 0;
		
		public void ApplyTo(EntityModelRenderer renderer)
		{
			Set(renderer, "cape", CapeEnabled);
			Set(renderer, "jacket", JacketEnabled);
			Set(renderer, "leftSleeve", LeftSleeveEnabled);
			Set(renderer, "rightSleeve", RightSleeveEnabled);
			Set(renderer, "leftPants", LeftPantsEnabled);
			Set(renderer, "rightPants", RightPantsEnabled);
			Set(renderer, "hat", HatEnabled);
		}

		private void Set(EntityModelRenderer renderer, string bone, bool value)
		{
			if (renderer.GetBone(bone, out var boneValue))
			{
				boneValue.Rendered = value;
			}
		}
	}
}
