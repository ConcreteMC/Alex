using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Graphics.Models.Items;
using Alex.Networking.Java.Packets.Play;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Converters;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Utils;
using Alex.Utils.Skins;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using MiNET.Net;
using MiNET.Utils.Metadata;
using MiNET.Worlds;
using Newtonsoft.Json;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = Microsoft.Xna.Framework.Color;
using LogManager = NLog.LogManager;
using MetadataByte = Alex.Networking.Java.Packets.Play.MetadataByte;
using MetadataFloat = Alex.Networking.Java.Packets.Play.MetadataFloat;
using ModelData = Alex.Graphics.Models.ModelBone;
using Point = System.Drawing.Point;
using Skin = MiNET.Utils.Skins.Skin;

namespace Alex.Entities
{
	public class RemotePlayer : LivingEntity
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(RemotePlayer));
		public Interfaces.GameMode Gamemode { get; private set; }

		public PlayerSkinFlags SkinFlags { get; } = PlayerSkinFlags.Default;

		public int Score { get; set; } = 0;
		public int Latency { get; set; } = 0;

		public BlockCoordinates BedPosition { get; set; } = BlockCoordinates.Zero;
		public Color PotionColor { get; set; } = Color.White;

		public ActionPermissions ActionPermissions { get; set; }
		public CommandPermission CommandPermissions { get; set; }
		public PermissionLevel PermissionLevel { get; set; }
		public uint CustomStoredPermissions { get; set; }

		public bool CanSprint => HealthManager.Hunger > 6;

		//private PooledTexture2D _texture;
		public RemotePlayer(World level, string geometry = "geometry.humanoid.customSlim", Skin skin = null) : base(
			level)
		{
			Type = "minecraft:player";
			//Name = name;

			Width = 0.6;
			Height = 1.80;
			//ShowItemInHand = true;
			IsInWater = false;

			Velocity = Vector3.Zero;

			//GeometryName = geometry;

			MovementSpeed = 0.1f; //0000000149011612f;//0000000149011612f;
			FlyingSpeed = 0.4f;
			AttackSpeed = 4.0;

			if (skin != null)
			{
				SetSkin(skin);
				//Skin = skin;
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

		private Skin _skin = null;
		private bool _skinDirty = false;

		public Skin Skin
		{
			get
			{
				return _skin;
			}
		}

		public void SetSkin(Skin skin, bool force = false)
		{
			if (skin == null || skin == _skin)
				return;

			_skin = skin;

			if (force)
			{
				World.BackgroundWorker.Enqueue(
					() =>
					{
						try
						{
							LoadSkin(skin);
						}
						catch (Exception ex)
						{
							Log.Warn(ex, $"Failed to load skin");
						}
					});

				return;
			}
			OnSkinValueChanged(skin);
		}
		
		public virtual void SetSprinting(bool sprinting)
		{
			if (sprinting)
			{
				if (!IsSprinting && CanSprint)
				{
					//  Player.Network?.EntityAction((int) Player.EntityId, EntityAction.StartSprinting);
					base.IsSprinting = true;
				}
			}
			else
			{
				if (IsSprinting)
				{
					base.IsSprinting = false;
					//  Player.Network?.EntityAction((int) Player.EntityId, EntityAction.StopSprinting);
				}
			}
		}

		public virtual void SetFlying(bool flying)
		{
			//Log.Info($"Tried to toggle fly. CanFly={CanFly} IsFlying={IsFlying} Target={flying}");

			if (!CanFly)
			{
				IsFlying = false;

				return;
			}

			if (!IsFlying && flying)
			{
				IsFlying = true;
			}
			else if (IsFlying && !flying)
			{
				IsFlying = false;
			}
		}

		public virtual void SetSwimming(bool swimming)
		{
			IsSwimming = swimming;
		}

		protected virtual void OnSkinValueChanged(Skin newSkin)
		{
			_skinDirty = true;

			if (IsSpawned)
			{
				QueueSkinProcessing(newSkin);
			}
		}

		/// <inheritdoc />
		public override void OnSpawn()
		{
			base.OnSpawn();

			if (_skinDirty)
			{
				QueueSkinProcessing(_skin);
			}

			if (!AnimationController.Initialized
			    && Alex.Instance.Resources.TryGetEntityDefinition(Type, out var description, out var source))
			{
				Description = description;
				AnimationController.UpdateEntityDefinition(source, source, description);
			}
		}

		/// <inheritdoc />
		public override void OnDespawn()
		{
			base.OnDespawn();
		}

		private int _skinQueuedCount = 0;

		private void QueueSkinProcessing(Skin skin)
		{
			if (Interlocked.CompareExchange(ref _skinQueuedCount, 1, 0) != 0)
			{
				Log.Warn($"Tried loading skin twice!");

				return;
			}

			//if (Level?.BackgroundWorker == null)
			//{
			//	ProcessSkin();
			//}
			//else
			{
				World.BackgroundWorker.Enqueue(
					() =>
					{
						try
						{
							LoadSkin(skin);
						}
						finally
						{
							_skinQueuedCount = 0;
							_skinDirty = false;
						}
					});

				//	Level.BackgroundWorker.Enqueue(ProcessSkin);
			}
		}

		private void LoadSkin(Skin skin, EntityModel model = null)
		{
			try
			{
				string nametag = NameTag ?? "n/a";
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
								Log.Debug($"Resourcepatch null for player {nametag}");
							}
							else
							{
								var resourcePatch = JsonConvert.DeserializeObject<SkinResourcePatch>(
									skin.ResourcePatch, GeometrySerializationSettings);

								if (resourcePatch?.Geometry != null)
								{
									//if (!Directory.Exists("playerSkins"))
									//File.WriteAllText(Path.Combine("playerskins", $"{resourcePatch.Geometry.Default}.json"), skin.GeometryData);

									Dictionary<string, EntityModel> models =
										new Dictionary<string, EntityModel>(StringComparer.OrdinalIgnoreCase);

									MCBedrockResourcePack.LoadEntityModel(skin.GeometryData, models);

									int preProcessed = models.Count;

									models = MCBedrockResourcePack.ProcessEntityModels(
										models, s =>
										{
											if (Alex.Instance.Resources.TryGetEntityModel(s, out var eModel))
											{
												return eModel;
											}

											Log.Debug($"Failed to resolve model: {s}");

											return null;
										});

									if (models == null || !models.TryGetValue(
										    resourcePatch.Geometry.Default, out model))
									{
										Log.Debug(
											$"Invalid geometry: \'{resourcePatch.Geometry.Default}\' for player \'{nametag.Replace("\n", "")}\'. Pre-Processing: {preProcessed}, post: {models?.Count ?? 0}");
									}
								}
								else
								{
									Log.Debug(
										$"Resourcepatch geometry was null for player {nametag.Replace("\n", "")}");
								}
							}
						}
						catch (Exception ex)
						{
							Log.Debug(
								ex,
								$"Could not create geometry: {ex.ToString()} for player {nametag.Replace("\n", "")}");
						}
					}
				}

				if (model != null && model.Bones.All(x => x.Cubes == null || x.Cubes.Length == 0))
				{
					model = null;
				}

				if (model == null)
				{
					if (!ModelFactory.TryGetModel(
						    slim ? "geometry.humanoid.customSlim" : "geometry.humanoid.custom", out model))
					{
						Log.Debug($"Invalid model for player {nametag.Replace("\n", "")}");

						return;
					}
				}


				Image<Rgba32> skinBitmap = null;

				try
				{
					if (skin == null || !skin.TryGetBitmap(model, out skinBitmap))
					{
						if (slim)
						{
							if (_alex == null)
							{
								if (Alex.Instance.Resources.TryGetBitmap("entity/alex", out var rawTexture))
								{
									_alex = rawTexture.Clone();
									skinBitmap = rawTexture;
								}
							}
							else
							{
								skinBitmap = _alex.Clone();
							}
						}
						else
						{
							if (_steve == null)
							{
								if (Alex.Instance.Resources.TryGetBitmap("entity/steve", out var rawTexture))
								{
									_steve = rawTexture.Clone();
									skinBitmap = rawTexture;
								}
							}
							else
							{
								skinBitmap = _steve.Clone();
							}
						}
					}

					if (model.TryGetRenderer(out var renderer))
					{
						ModelRenderer = renderer;
					}
					else
					{
						Log.Debug(
							$"No renderer for model: \"{model.Description?.Identifier ?? "N/A"}\" for player \'{nametag.Replace("\n", "")}\' (Disposing)");
					}

					if (skinBitmap != null)
					{
						var modelTextureSize = new Point(
							(int)model.Description.TextureWidth, (int)model.Description.TextureHeight);

						var textureSize = new Point(skinBitmap.Width, skinBitmap.Height);

						if (modelTextureSize != textureSize)
						{
							if (modelTextureSize.Y > textureSize.Y)
							{
								skinBitmap = SkinUtils.ConvertSkin(skinBitmap, modelTextureSize.X, modelTextureSize.Y);
							}
						}

						TextureUtils.BitmapToTexture2DAsync(
							this, Alex.Instance.GraphicsDevice, skinBitmap, texture2D =>
							{
								try
								{
									if (texture2D == null)
									{
										Log.Warn($"Null texture for entity: {NameTag}");

										return;
									}

									Texture = texture2D;
								}
								finally
								{
									skinBitmap?.Dispose();
								}
							},
							$"Skin Texture, Player={(NameTag?.Replace("\n", "") ?? EntityId.ToString())}, Width={textureSize.X} Height={textureSize.Y}");
					}
				}
				finally
				{
					//skinBitmap?.Dispose();
				}
			}
			catch (Exception ex)
			{
				Log.Warn(ex, $"Error while handling player skin.");
			}
		}

		/// <inheritdoc />
		protected override void UpdateItemPosition(IItemRenderer oldValue, IItemRenderer renderer)
		{
			//var primaryArm = GetPrimaryArm();
			//if (oldValue != renderer)
			{
				//if (oldValue != null)
				//primaryArm?.Remove(oldValue);
			}

			if (renderer == null || renderer.Model == null)
				return;

			//oldValue?.Parent?.Remove(oldValue);
			DisplayPosition pos;

			if (IsFirstPersonMode)
			{
				pos = DisplayPosition.FirstPerson;
			}
			else
			{
				pos = DisplayPosition.ThirdPerson;
			}

			//if (pos.HasFlag(DisplayPosition.FirstPerson) || pos.HasFlag(DisplayPosition.ThirdPerson))
			{
				if (IsLeftHanded)
				{
					if (!pos.HasFlag(DisplayPosition.LeftHand))
					{
						//pos = (pos & ~(DisplayPosition.LeftHand | DisplayPosition.RightHand));
						pos |= DisplayPosition.LeftHand;
					}
				}
				else
				{
					if (!pos.HasFlag(DisplayPosition.RightHand))
					{
						//pos = (pos & ~(DisplayPosition.LeftHand | DisplayPosition.RightHand));
						pos |= DisplayPosition.RightHand;
					}
				}

				/*if (IsFirstPersonMode)
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
				}*/

				renderer.DisplayPosition = pos;
			}

			//	if (oldValue != renderer)
			{
				//if (renderer != null)
				//	primaryArm?.AddChild(renderer);
			}
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 15 && entry is MetadataFloat flt)
			{
				//Additional hearts
				//HealthManager.MaxHealth 
			}
			else if (entry.Index == 16 && entry is MetadataVarInt score)
			{
				Score = score.Value;
			}
			else if (entry.Index == 17 && entry is MetadataByte data)
			{
				SkinFlags.Value = data.Value;

				if (ModelRenderer != null)
				{
					SkinFlags.ApplyTo(ModelRenderer);
				}
			}
			else if (entry.Index == 18 && entry is MetadataByte metaByte)
			{
				IsLeftHanded = metaByte.Value == 0;
			}
		}

		/// <inheritdoc />
		protected override bool HandleMetadata(MiNET.Entities.Entity.MetadataFlags flag, MetadataEntry entry)
		{
			if (flag == MiNET.Entities.Entity.MetadataFlags.BedPosition && entry is MetadataIntCoordinates mic)
			{
				BedPosition = new BlockCoordinates(mic.Value.X, mic.Value.Y, mic.Value.Z);

				return true;
			}

			if (flag == MiNET.Entities.Entity.MetadataFlags.PotionColor && entry is MetadataInt potionColor)
			{
				//int a = 255;
				//int r = 255;
				//int g = 255;
				//int b = 255;

				PotionColor = new Color((uint)potionColor.Value);

				//PotionColor = 
				return true;
			}

			return base.HandleMetadata(flag, entry);
		}

		public void UpdateGamemode(Interfaces.GameMode gamemode)
		{
			Gamemode = gamemode;

			if (Gamemode == Interfaces.GameMode.Creative)
			{
				CanFly = true;
			}
		}

		/// <inheritdoc />
		protected override void OnModelUpdated()
		{
			base.OnModelUpdated();

			var modelRenderer = ModelRenderer;

			if (modelRenderer != null)
			{
				SkinFlags?.ApplyTo(modelRenderer);
			}
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
		protected override ModelData GetPrimaryArm()
		{
			var modelRenderer = ModelRenderer;

			if (modelRenderer == null)
				return null;

			ModelData arm = null;

			if (IsLeftHanded)
			{
				if (modelRenderer.GetBone("leftItem", out arm) || modelRenderer.GetBone("leftArm", out arm))
				{
					return arm;
				}
			}
			else
			{
				if (modelRenderer.GetBone("rightItem", out arm) || modelRenderer.GetBone("rightArm", out arm))
				{
					return arm;
				}
			}

			return arm;
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