using System;
using System.Linq;
using Alex.API;
using Alex.API.Entities;
using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Items;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using MiNET.Utils;
using NLog;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using MathF = System.MathF;
using PlayerLocation = Alex.API.Utils.PlayerLocation;
using UUID = Alex.API.Utils.UUID;

namespace Alex.Entities
{
	public class Entity : IEntity, IPhysicsEntity
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Entity));

		internal EntityModelRenderer ModelRenderer { get; set; }

		public World Level { get; set; }

		public int JavaEntityId { get; protected set; }
		public int EntityTypeId { get; private set; }
		public long EntityId { get; set; }
		public bool IsSpawned { get; set; }

		public DateTime LastUpdatedTime { get; set; }
		public PlayerLocation KnownPosition { get; set; }

		public Vector3 Velocity { get; set; } = Vector3.Zero;
		private float _posOffset = 0;
		private bool _posOffsetSet = false;

		public float PositionOffset
		{
			get
			{
				if (!_posOffsetSet)
					return (float) Height;
				return _posOffset;
			}
			set
			{
				_posOffsetSet = true;
				_posOffset = value;
			}
		}

		//public HealthManager HealthManager { get; set; }

		public string NameTag { get; set; }

		public bool NoAi { get; set; } = true;
		public bool HideNameTag { get; set; } = true;
		public bool Silent { get; set; }
		public bool IsInWater { get; set; } = false;
		public bool IsInLava { get; set; } = false;
		public bool IsOutOfWater => !IsInWater;
		public bool Invulnerable { get; set; } = false;

		public long Age { get; set; }
		public float Scale { get; set; } = 1.0f;
		public double Height { get; set; } = 1;
		public double Width { get; set; } = 1;
		public double Length { get; set; } = 1;
		public double Drag { get; set; } = 0.6f;
		public double Gravity { get; set; } = 16.8f; //9.81f; //1.6f;
		public float TerminalVelocity { get; set; } = 78.4f;

		public double MovementSpeed { get; set; } = 0.1F;
		public double FlyingSpeed { get; set; } = 0.4F;
		
		public int Data { get; set; }
		public UUID UUID { get; set; } = new UUID(Guid.Empty.ToByteArray());

		public bool CanFly { get; set; } = false;
		public bool IsFlying { get; set; } = false;

		public bool IsCollidingWithWorld { get; set; } = false;

		public INetworkProvider Network { get; set; }
		public Inventory Inventory { get; protected set; }
		private IItemRenderer ItemRenderer { get; set; } = null;
		public Entity(int entityTypeId, World level, INetworkProvider network)
		{
			Network = network;

			EntityId = -1;
			Level = level;
			EntityTypeId = entityTypeId;
			KnownPosition = new PlayerLocation();
			Inventory = new Inventory(46);
			//	HealthManager = new HealthManager(this);
			
			Inventory.SlotChanged += OnInventorySlotChanged;
			Inventory.SelectedHotbarSlotChanged += InventoryOnSelectedHotbarSlotChanged;

			HideNameTag = true;
		}

		private void CheckHeldItem()
		{
			var inHand = Inventory[Inventory.SelectedSlot];
			//Log.Info($"Inventory slot changed.");
			
			if ((inHand == null || inHand.Count == 0 || inHand.Id <= 0) && ItemRenderer != null)
			{
				if (ModelRenderer.GetBone("rightItem", out EntityModelRenderer.ModelBone bone))
				{
					bone.Detach(ItemRenderer);
				}
				
				ItemRenderer = null;
				return;
			}

			if (inHand != null)
			{
				if (!string.IsNullOrWhiteSpace(inHand.Name))
				{
					/*var itemModel = Alex.Instance.Resources.ResourcePack.ItemModels.FirstOrDefault(x =>
						x.Key.Contains(inHand.Name, StringComparison.InvariantCultureIgnoreCase));
					
					ItemRenderer = new ItemModelRenderer(itemModel.Value, Alex.Instance.Resources.ResourcePack);*/

					var renderer = inHand.Renderer;
					if (renderer == null)
					{
						Log.Warn($"No renderer for item: {inHand.Name}");
						return;
					}

					if (renderer == ItemRenderer)
						return;
					
					var itemModel = renderer.Model;

					ItemRenderer = renderer;
					
					if (this is Player)
					{
						if (itemModel.Display.TryGetValue("firstperson_righthand", out var value))
						{
							ItemRenderer.Rotation = value.Rotation;
							ItemRenderer.Translation = value.Translation;
							ItemRenderer.Scale = value.Scale;
							
						/*	if (ModelRenderer.GetBone("rightItem", out EntityModelRenderer.ModelBone bone))
							{
						//		Log.Info($"First Person item model rendering ready.");

								//bone.Attach(ItemRenderer);
							}
							else
							{
								Log.Warn($"Bone not found: rightItem");
							}*/
						}
						else
						{
							Log.Warn($"Failed to get item model display element!");
						}
					}
					else
					{
						if (itemModel.Display.TryGetValue("thirdperson_righthand", out var value))
						{
							ItemRenderer.Rotation = value.Rotation;
							ItemRenderer.Translation = value.Translation;
							ItemRenderer.Scale = value.Scale;
							
							if (ModelRenderer.GetBone("rightItem", out EntityModelRenderer.ModelBone bone))
							{
						//		Log.Info($"Third Person item model rendering ready.");

								//bone.Attach(ItemRenderer);
							}
						}
						else
						{
							Log.Warn($"Failed to get item model display element!");
						}
					}
				}
			}
			else
			{
				if (ItemRenderer != null)
				{
					if (ModelRenderer.GetBone("rightItem", out EntityModelRenderer.ModelBone bone))
					{
						bone.Detach(ItemRenderer);
					}

					ItemRenderer = null;
				}
			}
		}
		
		private void InventoryOnSelectedHotbarSlotChanged(object? sender, SelectedSlotChangedEventArgs e)
		{
			CheckHeldItem();
		}

		private void OnInventorySlotChanged(object sender, SlotChangedEventArgs e)
		{
			CheckHeldItem();
		}

		public bool IsSneaking { get; set; }
		public bool IsRiding { get; set; }
		public bool IsSprinting { get; set; }
		public bool IsUsingItem { get; set; }
		public bool IsInvisible { get; set; }
		public bool IsTempted { get; set; }
		public bool IsInLove { get; set; }
		public bool IsSaddled { get; set; }
		public bool
			IsPowered
		{ get; set; }
		public bool IsIgnited { get; set; }
		public bool IsBaby { get; set; }
		public bool IsConverting { get; set; }
		public bool IsCritical { get; set; }
		public bool IsShowName => !HideNameTag;
		public bool IsAlwaysShowName { get; set; }
		public bool IsNoAi => NoAi;
		public bool IsSilent { get; set; }
		public bool IsWallClimbing { get; set; }
		public bool IsResting { get; set; }
		public bool IsSitting { get; set; }
		public bool IsAngry { get; set; }
		public bool IsInterested { get; set; }
		public bool IsCharged { get; set; }
		public bool IsTamed { get; set; }
		public bool IsLeashed { get; set; }
		public bool IsSheared { get; set; }
		public bool IsFlagAllFlying { get; set; }
		public bool IsElder { get; set; }
		public bool IsMoving { get; set; }
		public bool IsBreathing => !IsInWater;
		public bool IsChested { get; set; }
		public bool IsStackable { get; set; }

		public bool RenderEntity { get; set; } = true;
		public bool ShowItemInHand { get; set; } = false;

		public void HandleMetadata(MetadataDictionary metadata)
		{
			foreach (var meta in metadata._entries)
			{
				switch ((MiNET.Entities.Entity.MetadataFlags) meta.Key)
				{
					case MiNET.Entities.Entity.MetadataFlags.CollisionBoxHeight:
					{
						if (meta.Value is MetadataFloat flt)
						{
							Height = flt.Value;
						}
					} break;
					case MiNET.Entities.Entity.MetadataFlags.CollisionBoxWidth:
					{
						if (meta.Value is MetadataFloat fltw)
						{
							Width = fltw.Value;
							Length = fltw.Value;
						}
					} break;
					case MiNET.Entities.Entity.MetadataFlags.Scale:
					{
						if (meta.Value is MetadataFloat flt)
						{
							Scale = flt.Value;
						}
					} break;
					case MiNET.Entities.Entity.MetadataFlags.EntityFlags:
					{
						if (meta.Value is MetadataLong lng)
						{
							BitArray bits = new BitArray(BitConverter.GetBytes(lng.Value));
							IsInvisible = bits[(int) MiNET.Entities.Entity.DataFlags.Invisible];
						}
					}
						break;
					case MiNET.Entities.Entity.MetadataFlags.NameTag:
					{
						if (meta.Value is MetadataString nametag)
						{
							NameTag = nametag.Value;
						}
					}
						break;
					default:
						Log.Debug($"Unknown flag: {(MiNET.Entities.Entity.MetadataFlags) meta.Key}");
						break;
				}
			}
		}
		
		public virtual void Render(IRenderArgs renderArgs)
		{
			if (RenderEntity || ShowItemInHand)
			{
				ModelRenderer.Render(renderArgs, KnownPosition, !RenderEntity);
			}
			if (ShowItemInHand)
			{
				ItemRenderer?.Render(renderArgs);
			}

		}

		public virtual void Update(IUpdateArgs args)
		{
			var now = DateTime.UtcNow;

			if (RenderEntity || ShowItemInHand)
			{
				ModelRenderer.Scale = Scale;
				ModelRenderer.Update(args, KnownPosition);
				
				if (ShowItemInHand)
				{
					//CheckHeldItem();
					
					//Matrix.CreateRotationY(MathUtils.ToRadians((-KnownPosition.HeadYaw)))
		//			ItemRenderer?.Update(Matrix.CreateRotationY(MathUtils.ToRadians(180f - KnownPosition.HeadYaw)) * Matrix.CreateTranslation(KnownPosition));
		ItemRenderer?.Update(
		                     Matrix.CreateRotationY(MathUtils.ToRadians((180f - KnownPosition.HeadYaw))) *
		                     Matrix.CreateTranslation(KnownPosition));
		
					//ItemRenderer?.World = 
					ItemRenderer?.Update(args.GraphicsDevice, args.Camera);
				}
			}
			
			if (now.Subtract(LastUpdatedTime).TotalMilliseconds >= 50)
			{
				LastUpdatedTime = now;
				try
				{
					OnTick();
				}
				catch(Exception e)
				{
					Log.Warn(e, $"Exception while trying to tick entity!");
				}
			}
		}

		public void UpdateHeadYaw(float rotation)
		{
			KnownPosition.HeadYaw = rotation;
		}

		protected bool DoRotationCalculations = true;

		public virtual void OnTick()
		{
			Age++;
			
			if (DoRotationCalculations)
				UpdateRotations();

			_previousPosition = KnownPosition;

			if (IsNoAi) return;
		//	IsMoving = Velocity.LengthSquared() > 0f;

			var feetBlock = Level?.GetBlock(new BlockCoordinates(KnownPosition));
			var headBlock = Level?.GetBlock(KnownPosition.GetCoordinates3D() + new BlockCoordinates(0, 1, 0));

			bool headInWater = false;
			bool feetInWater = false;
			
			if (headBlock != null)
			{
				if (headBlock.BlockMaterial == Material.Water || headBlock.IsWater)
				{
					headInWater = true;
				}
			}
			
			if (feetBlock != null)
			{
				if (feetBlock.BlockMaterial == Material.Water || feetBlock.IsWater)
				{
					feetInWater = true;
				}

				if (feetBlock.BlockMaterial == Material.Lava)
				{
					IsInLava = true;
				}
				else
				{
					IsInLava = false;
				}
				
				if (!feetBlock.Solid)
				{
					if (KnownPosition.OnGround)
					{
						KnownPosition.OnGround = false;
					}
					else
					{
					}
				}
				else
				{
					//KnownPosition.OnGround = true;
				}
			}

			if (headInWater || feetInWater)
			{
				IsInWater = true;
			}
			else
			{
				IsInWater = false;
			}

			/*
			if (headBlock != null)
			{
				if (headBlock.IsWater)
				{
					IsInWater = true;
				}
				else
				{
					IsInWater = false;
				}
			}*/

			//HealthManager.OnTick();
		}

		private float HeadRenderOffset = 0;
		private int turnTicks;
		private int turnTicksLimit = 10;
		private float lastRotationYawHead = 0f;
		private Vector3 _previousPosition = Vector3.Zero;
		protected bool SnapHeadYawRotationOnMovement { get; set; } = true;
		private void UpdateRotations()
		{
			double deltaX = KnownPosition.X - _previousPosition.X;
			double deltaZ = KnownPosition.Z - _previousPosition.Z;
			double distSQ = deltaX * deltaX + deltaZ * deltaZ;

			IsMoving = distSQ > 0f || Velocity.LengthSquared() > 0f;

			float maximumHeadBodyAngleDifference = 75f;
			const float MOVEMENT_THRESHOLD_SQ = 2.5f;
			// if moving:
			// 1) snap the body yaw (renderYawOffset) to the movement direction (rotationYaw)
			// 2) constrain the head yaw (rotationYawHead) to be within +/- 90 of the body yaw (renderYawOffset)
			if (distSQ > MOVEMENT_THRESHOLD_SQ)
			{
				//dragon.renderYawOffset = dragon.rotationYaw;
				float newRotationYawHead = MathUtils.ConstrainAngle(KnownPosition.Yaw, KnownPosition.HeadYaw,
					maximumHeadBodyAngleDifference);
				
				if (SnapHeadYawRotationOnMovement)
				{
					KnownPosition.HeadYaw = newRotationYawHead;
				}

				lastRotationYawHead = newRotationYawHead;
				turnTicks = 0;
			}
			else
			{
				var changeInHeadYaw = Math.Abs(KnownPosition.HeadYaw - lastRotationYawHead);
				if (changeInHeadYaw > 15f)
				{
					turnTicks = 0;
					lastRotationYawHead = KnownPosition.HeadYaw;
				}
				else
				{
					turnTicks++;
					if (turnTicks > turnTicksLimit)
					{
						maximumHeadBodyAngleDifference =
							Math.Max(1f - (float) ((float)(turnTicks - turnTicksLimit) / turnTicksLimit), 0f) *
							maximumHeadBodyAngleDifference;
					}
				}

				KnownPosition.Yaw = MathUtils.ConstrainAngle(KnownPosition.Yaw, KnownPosition.HeadYaw,
					maximumHeadBodyAngleDifference);
			}
		}

		public BoundingBox BoundingBox => GetBoundingBox();
		public BoundingBox GetBoundingBox()
		{
			var pos = KnownPosition;
			return GetBoundingBox(pos);
		}
		
		public BoundingBox GetBoundingBox(Vector3 pos)
		{
			double halfWidth = (Width * Scale) / 2D;

			return new BoundingBox(new Vector3((float)(pos.X - halfWidth), pos.Y, (float)(pos.Z - halfWidth)), new Vector3((float)(pos.X + halfWidth), (float)(pos.Y + (Height * Scale)), (float)(pos.Z + halfWidth)));
		}

		public void RenderNametag(IRenderArgs renderArgs)
		{
			var boundingBox = GetBoundingBox(Vector3.Zero);
			var halfWidth = -((((float) Width) * Scale));
			
			var maxDistance = (renderArgs.Camera.FarDistance) / 2f;
			//maxDistance = renderArgs.Camera.FarDistance;
			
			Vector3 posOffset = new Vector3(0, 0.25f, 0);

			if (RenderEntity && ModelRenderer != null && ModelRenderer.Valid && !IsInvisible && !ModelRenderer.Texture.IsFullyTransparent)
			{
				posOffset.Y += (float) (Height * Scale);
			}

			var cameraPosition = new Vector3(renderArgs.Camera.Position.X, 0, renderArgs.Camera.Position.Z);
			
			var rotation = new Vector3(KnownPosition.X, 0, KnownPosition.Z) - cameraPosition;
			rotation.Normalize();
			
			
			var pos = KnownPosition + posOffset + (rotation * halfWidth);
			//pos.Y = 0;
			
			var distance = Vector3.DistanceSquared(pos, renderArgs.Camera.Position);
			if (distance >= maxDistance)
			{
				return;
			}
			
			//s = MathF.Round(s, 2, MidpointRounding.ToEven);
			//float s = 1f;
			Vector2 textPosition;

			var screenSpace = renderArgs.GraphicsDevice.Viewport.Project(pos, 
				renderArgs.Camera.ProjectionMatrix,
				renderArgs.Camera.ViewMatrix,
				Matrix.Identity);

			float s = 1f - ((distance) * (1f / maxDistance));

			textPosition.X = screenSpace.X;
			textPosition.Y = screenSpace.Y;

			var scale = new Vector2(s, s);
			scale *= Alex.Instance.GuiRenderer.ScaledResolution.ElementScale;
	
			string clean = NameTag;

			var stringCenter = Alex.Font.MeasureString(clean, scale);
			var c = new Point((int)stringCenter.X, (int)stringCenter.Y);

			textPosition.X = (int)(textPosition.X - (c.X / 2d));
			textPosition.Y = (int)(textPosition.Y - (c.Y / 2d));
			
			renderArgs.SpriteBatch.FillRectangle(new Rectangle(textPosition.ToPoint(), c), new Color(Color.Black, 128), screenSpace.Z);
			Alex.Font.DrawString(renderArgs.SpriteBatch, clean, textPosition, TextColor.White, FontStyle.None, scale, layerDepth: screenSpace.Z);
		}

		public virtual void TerrainCollision(Vector3 collisionPoint, Vector3 direction)
		{
			if (direction.Y < 0) //Collided with the ground
			{
				KnownPosition.OnGround = true;
			}
		}

		public void Dispose()
		{
			ModelRenderer?.Dispose();
		}
	}
}