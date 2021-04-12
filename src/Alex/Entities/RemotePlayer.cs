using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Items;
using Alex.Net;
using Alex.Networking.Java.Packets.Play;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Converters;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Utils;
using Alex.Worlds;
using Alex.Worlds.Multiplayer.Bedrock;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET;
using MiNET.Worlds;
using Newtonsoft.Json;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using LogManager = NLog.LogManager;
using Point = System.Drawing.Point;
using Skin = MiNET.Utils.Skins.Skin;

namespace Alex.Entities
{
	public class RemotePlayer : LivingEntity
	{
		private static readonly Logger   Log = LogManager.GetCurrentClassLogger(typeof(RemotePlayer));
		public  GameMode Gamemode { get; private set; }

		public string GeometryName { get; set; }
		
		public PlayerSkinFlags SkinFlags { get; }

		public int Score   { get; set; } = 0;
		public int Latency { get; set; } = 0;

		//private PooledTexture2D _texture;
		public RemotePlayer(World level, string geometry = "geometry.humanoid.customSlim", Skin skin = null) : base(level)
		{
			SkinFlags = new PlayerSkinFlags()
			{
				Value = 0xff
			};
			
			//Name = name;

			Width = 0.6;
			Height = 1.80;
			ShowItemInHand = true;
			IsInWater = false;

			Velocity = Vector3.Zero;

			GeometryName = geometry;
			
			MovementSpeed = 0.1f;//0000000149011612f;//0000000149011612f;
			FlyingSpeed = 0.4f;
			
			if (skin != null)
			{
				Skin = skin;
			}
			else
			{
				if (geometry != null)
					_skinDirty = true;
			}

			base.NoAi = true;
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

		private Skin _skin      = null;
		private bool _skinDirty = false;
		public Skin Skin
		{
			get
			{
				return _skin;
			}
			set
			{
				_skin = value;
				_skinDirty = true;

				if (IsSpawned)
				{
					QueueSkinProcessing();
				}
			}
		}

		/// <inheritdoc />
		public override void OnSpawn()
		{
			base.OnSpawn();

			if (_skinDirty)
			{
				QueueSkinProcessing();
			}
		}

		/// <inheritdoc />
		public override void OnDespawn()
		{
			base.OnDespawn();
		}

		private int _skinQueuedCount = 0;
		private void QueueSkinProcessing()
		{
			//if (Interlocked.CompareExchange(ref _skinQueuedCount, 1, 0) == 0)
			{
				if (Level?.BackgroundWorker == null)
				{
					ProcessSkin();
				}
				else
				{
					Level.BackgroundWorker.Enqueue(ProcessSkin);
				}
			}
		}

		private void ProcessSkin()
		{
			try
			{
				if (_skin == null)
				{
					if (ModelFactory.TryGetModel(GeometryName, out var entityModel))
					{
						//var skin = entityModel.ToSkin();
						//	PooledTexture2D texture2D = skinTexture ?? _alex;
						//if (texture2D != null)
						//	skin.UpdateTexture(texture2D);
				
						//_skin = skin;
						LoadSkin(null, entityModel);
					}
				}
				else
				{
					LoadSkin(_skin);
				}
				
				if (!AnimationController.Initialized &&
				    Alex.Instance.Resources.BedrockResourcePack.EntityDefinitions.TryGetValue(
					"minecraft:player", out var description))
				{
					AnimationController.UpdateEntityDefinition( Alex.Instance.Resources.BedrockResourcePack, description);
				}
			}
			finally
			{
				_skinQueuedCount = 0;
				_skinDirty = false;
			}
		}

		private void LoadSkin(Skin skin, EntityModel model = null)
		{
			try
			{
//				EntityModel model = null;
				bool slim = skin?.Slim ?? false;
				if (skin != null && model == null)
				{
					if (!string.IsNullOrWhiteSpace(skin.GeometryData) && !skin.GeometryData.Equals(
						"null", StringComparison.InvariantCultureIgnoreCase))
					{
						try
						{
							if (string.IsNullOrWhiteSpace(skin.ResourcePatch) || skin.ResourcePatch == "null")
							{
								Log.Debug($"Resourcepatch null for player {NameTag}");
							}
							else
							{
								var resourcePatch = JsonConvert.DeserializeObject<SkinResourcePatch>(
									skin.ResourcePatch, GeometrySerializationSettings);
								
								Dictionary<string, EntityModel> models = new Dictionary<string, EntityModel>();
								BedrockResourcePack.LoadEntityModel(skin.GeometryData, models);

								var processedModels = BedrockResourcePack.ProcessEntityModels(models);

								if (processedModels == null || processedModels.Count == 0)
								{
									//		Log.Warn($"!! Model count was 0 for player {NameTag} !!");
									/*
																		if (!Directory.Exists("failed"))
																			Directory.CreateDirectory("failed");
									
																		File.WriteAllText(
																			Path.Combine(
																				"failed",
																				$"{Environment.TickCount64}-{resourcePatch.Geometry.Default}.json"),
																			skin.GeometryData);*/
								}
								else
								{
									if (resourcePatch?.Geometry != null)
									{
										if (!processedModels.TryGetValue(resourcePatch.Geometry.Default, out model))
										{
											Log.Debug(
												$"Invalid geometry: {resourcePatch.Geometry.Default} for player {NameTag}");
										}

									}
									else
									{
										Log.Debug($"Resourcepatch geometry was null for player {NameTag}");
									}
								}
							}
						}
						catch (Exception ex)
						{
							string name = "N/A";
							Log.Debug(ex, $"Could not create geometry ({name}): {ex.ToString()} for player {NameTag}");
						}
					}
					else
					{
						//Log.Debug($"Geometry data null for player {Name}");
					}
				}

				//if (!skin.IsPersonaSkin)
				//{
					
				//}

				if (model == null)
				{
					ModelFactory.TryGetModel(
						skin.Slim ? "geometry.humanoid.custom" : "geometry.humanoid.customSlim", out model);

					/*model = skin.Slim ? (EntityModel) new Models.HumanoidCustomslimModel() :
						(EntityModel) new HumanoidModel();*/ // new Models.HumanoidCustomGeometryHumanoidModel();
				}

				if (model != null)
				{
					Image<Rgba32> skinBitmap = null;

					try
					{
						if (!skin.TryGetBitmap(model, out skinBitmap))
						{
							//Log.Warn($"No custom skin data for player {NameTag}");

							if (slim)
							{
								if (_alex == null && Alex.Instance.Resources.TryGetBitmap(
									"entity/alex", out var rawTexture))
								{
									_alex = rawTexture.Clone();
									skinBitmap = rawTexture;
								}
								else
								{
									skinBitmap = _alex.Clone();
								}
							}
							else
							{
								if (_steve == null && Alex.Instance.Resources.TryGetBitmap(
									"entity/steve", out var rawTexture))
								{
									_steve = rawTexture.Clone();
									skinBitmap = rawTexture;
								}
								else
								{
									skinBitmap = _steve.Clone();
								}
							}
						}

						var modelTextureSize = new Point(
							(int) model.Description.TextureWidth, (int) model.Description.TextureHeight);

						var textureSize = new Point(skinBitmap.Width, skinBitmap.Height);

						if (modelTextureSize != textureSize)
						{
							if (modelTextureSize.Y > textureSize.Y)
							{
								skinBitmap = SkinUtils.ConvertSkin(skinBitmap, modelTextureSize.X, modelTextureSize.Y);
							}

							/*var bitmap = skinBitmap;
							bitmap.Mutate<Rgba32>(xx =>
							{
								xx.Resize(modelTextureSize.X, modelTextureSize.Y);
							//	xx.Flip(FlipMode.Horizontal);
							});
		
							skinBitmap = bitmap;*/
						}

						GeometryName = model.Description.Identifier;

						var modelRenderer = new EntityModelRenderer(
							model, TextureUtils.BitmapToTexture2D(this, Alex.Instance.GraphicsDevice, skinBitmap));

						if (modelRenderer.Valid)
						{
							ModelRenderer = modelRenderer;
						}
						else
						{
							modelRenderer.Dispose();
							Log.Debug($"Invalid model: for player {NameTag} (Disposing)");
						}
					}
					finally
					{
						skinBitmap?.Dispose();
					}
				}
				else
				{
					Log.Debug($"Invalid model for player {NameTag}");
				}
			}
			catch (Exception ex)
			{
				Log.Warn(ex, $"Error while handling player skin.");
			}
		}

		/// <inheritdoc />
		protected override void UpdateItemPosition(IItemRenderer renderer)
		{
			if (renderer == null)
				return;
			
			var pos = renderer.DisplayPosition;
			//if (pos.HasFlag(DisplayPosition.FirstPerson) || pos.HasFlag(DisplayPosition.ThirdPerson))
			{
				if (IsLeftHanded)
				{
					if (!pos.HasFlag(DisplayPosition.LeftHand))
					{
						pos = (pos & ~(DisplayPosition.LeftHand | DisplayPosition.RightHand));
						pos |= DisplayPosition.LeftHand;
					}
				}
				else
				{
					if (!pos.HasFlag(DisplayPosition.RightHand))
					{
						pos = (pos & ~(DisplayPosition.LeftHand | DisplayPosition.RightHand));
						pos |= DisplayPosition.RightHand;
					}
				}

				if (IsFirstPersonMode)
				{
					if (!pos.HasFlag(DisplayPosition.FirstPerson))
					{
						pos = (pos & ~(DisplayPosition.FirstPerson | DisplayPosition.ThirdPerson));
						pos |= DisplayPosition.FirstPerson;
					}
				}
				else
				{
					if (!pos.HasFlag(DisplayPosition.ThirdPerson))
					{
						pos = (pos & ~(DisplayPosition.FirstPerson | DisplayPosition.ThirdPerson));
						pos |= DisplayPosition.ThirdPerson;
					}
				}

				renderer.DisplayPosition = pos;
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

		public void UpdateGamemode(GameMode gamemode)
		{
			Gamemode = gamemode;
		}

		/// <inheritdoc />
		protected override void OnModelUpdated()
		{
			base.OnModelUpdated();

			var modelRenderer = ModelRenderer;
			if (modelRenderer != null && SkinFlags != null)
				SkinFlags.ApplyTo(modelRenderer);
		}

		/// <inheritdoc />
		public override void HandleEntityStatus(byte status)
		{
			if (status == 9) //Marks item use as finished
			{
				IsUsingItem = false;
				IsEating = false;
				IsBlocking = false;
				return;
			}
			
			base.HandleEntityStatus(status);
		}

		/// <inheritdoc />
		public override void EntityHurt()
		{
			base.EntityHurt();
			Alex.Instance.AudioEngine.PlaySound("game.player.hurt", RenderLocation, 1f, 1f);
		}

		/// <inheritdoc />
		public override void EntityDied()
		{
			base.EntityDied();
			Alex.Instance.AudioEngine.PlaySound("game.player.die", RenderLocation, 1f, 1f);
		}

		/// <inheritdoc />
		protected override void OnDispose()
		{
			base.OnDispose();
			_skin = null;
		}

		private static Image<Rgba32> _steve;
		private static Image<Rgba32> _alex;
	}
}
