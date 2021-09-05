using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Gamestates;
using Alex.Graphics.Camera;
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
using Alex.Utils.Skins;
using Alex.Worlds;
using Alex.Worlds.Multiplayer.Bedrock;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET;
using MiNET.Utils.Metadata;
using MiNET.Worlds;
using Newtonsoft.Json;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = Microsoft.Xna.Framework.Color;
using LogManager = NLog.LogManager;
using MetadataByte = Alex.Networking.Java.Packets.Play.MetadataByte;
using MetadataFloat = Alex.Networking.Java.Packets.Play.MetadataFloat;
using ModelData = Alex.Graphics.Models.Entity.ModelBone;
using Point = System.Drawing.Point;
using Skin = MiNET.Utils.Skins.Skin;

namespace Alex.Entities
{
	public class RemotePlayer : LivingEntity
	{
		private static readonly Logger   Log = LogManager.GetCurrentClassLogger(typeof(RemotePlayer));
		public  GameMode Gamemode { get; private set; }

		public PlayerSkinFlags SkinFlags { get; }

		public int Score   { get; set; } = 0;
		public int Latency { get; set; } = 0;

		public BlockCoordinates BedPosition { get; set; } = BlockCoordinates.Zero;
		public Color PotionColor { get; set; } = Color.White;
		
		public  bool CanSprint    => HealthManager.Hunger > 6;
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
			//ShowItemInHand = true;
			IsInWater = false;

			Velocity = Vector3.Zero;

			//GeometryName = geometry;
			
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
				if (value == null || value == _skin || value.IsPersonaSkin)
					return;
				
				_skin = value;

				OnSkinValueChanged(value);
			}
		}
		
		public void SetSprinting(bool sprinting)
		{
			if (sprinting)
			{
				if (!IsSprinting && CanSprint)
				{
					//  Player.Network?.EntityAction((int) Player.EntityId, EntityAction.StartSprinting);
					IsSprinting = true;
				}
			}
			else
			{
				if (IsSprinting)
				{
					IsSprinting = false;
					//  Player.Network?.EntityAction((int) Player.EntityId, EntityAction.StopSprinting);
				}
			}
		}

		public void SetFlying(bool flying)
		{
			Log.Info($"Tried to toggle fly. CanFly={CanFly} IsFlying={IsFlying} Target={flying}");

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

		protected virtual void OnSkinValueChanged(Skin newSkin)
		{
			_skinDirty = true;

			if (IsSpawned)
			{
				QueueSkinProcessing();
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
			if (Interlocked.CompareExchange(ref _skinQueuedCount, 1, 0) == 0)
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
					LoadSkin(null, null);
				}
				else
				{
					LoadSkin(_skin);
				}
				
				if (!AnimationController.Initialized &&
				    Alex.Instance.Resources.TryGetEntityDefinition(
					"minecraft:player", out var description, out var source))
				{
					AnimationController.UpdateEntityDefinition(source, description);
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
									
									Dictionary<string, EntityModel> models = new Dictionary<string, EntityModel>();
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
									
									if (models == null || !models.TryGetValue(resourcePatch.Geometry.Default, out model))
									{
										Log.Debug(
											$"Invalid geometry: \'{resourcePatch.Geometry.Default}\' for player \'{nametag.Replace("\n", "")}\'. Pre-Processing: {preProcessed}, post: {models?.Count ?? 0}");
									}
								}
								else
								{
									Log.Debug($"Resourcepatch geometry was null for player {nametag.Replace("\n", "")}");
								}
							}
						}
						catch (Exception ex)
						{
							Log.Debug(ex, $"Could not create geometry: {ex.ToString()} for player {nametag.Replace("\n", "")}");
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
						slim ? "geometry.humanoid.custom" : "geometry.humanoid.customSlim", out model))
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

					/*if (skinBitmap != null)
					{
						var modelTextureSize = new Point(
							(int) model.Description.TextureWidth, (int) model.Description.TextureHeight);

						var textureSize = new Point(skinBitmap.Width, skinBitmap.Height);

						if (modelTextureSize != textureSize)
						{
							if (modelTextureSize.Y > textureSize.Y)
							{
								skinBitmap = SkinUtils.ConvertSkin(skinBitmap, modelTextureSize.X, modelTextureSize.Y);
							}
						}
					}*/

					if (EntityModelRenderer.TryGetRenderer(model, out var renderer))
					{
						if (skinBitmap != null)
						{
							Alex.Instance.UiTaskManager.Enqueue(
								(state) =>
								{
									var img = (Image<Rgba32>)state;
									Texture = TextureUtils.BitmapToTexture2D(
										this, Alex.Instance.GraphicsDevice, img);
									img.Dispose();
									
								}, skinBitmap);
						}
						ModelRenderer = renderer;
					}
					else
					{
						Log.Debug(
							$"No renderer for model: \"{model.Description?.Identifier ?? "N/A"}\" for player \'{nametag.Replace("\n", "")}\' (Disposing)");
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
			var primaryArm = GetPrimaryArm();
			//if (oldValue != renderer)
			{
				primaryArm?.Remove(oldValue);
			}

			if (renderer == null)
				return;

			//oldValue?.Parent?.Remove(oldValue);
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

			//	if (oldValue != renderer)
			{
				renderer.Update(
					new UpdateArgs()
					{
						Camera = new Camera(),
						GameTime = new GameTime(),
						GraphicsDevice = Alex.Instance.GraphicsDevice
					});

				primaryArm?.AddChild(renderer);
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

				PotionColor = new Color((uint) potionColor.Value);
				
				//PotionColor = 
				return true;
			}

			return base.HandleMetadata(flag, entry);
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
			ModelData arm = null;

			if (IsLeftHanded)
			{
				arm = _leftItemModel;
			}
			else
			{
				arm = _rightItemModel;
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
