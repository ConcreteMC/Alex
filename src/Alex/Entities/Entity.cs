using System;
using Alex.API.Entities;
using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.Graphics.Models.Entity;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using NLog;

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
		public float PositionOffset { get; set; }

		//public HealthManager HealthManager { get; set; }

		public string NameTag { get; set; }

		public bool NoAi { get; set; } = true;
		public bool HideNameTag { get; set; } = true;
		public bool Silent { get; set; }
		public bool IsInWater { get; set; } = false;
		public bool IsOutOfWater => !IsInWater;
		public bool Invulnerable { get; set; } = false;

		public long Age { get; set; }
		public double Scale { get; set; } = 1.0;
		public double Height { get; set; } = 1;
		public double Width { get; set; } = 1;
		public double Length { get; set; } = 1;
		public double Drag { get; set; } = 0.4f;
		public double Gravity { get; set; } = 1.6f;
		public float TerminalVelocity { get; set; } = 78.4f;

		public double MovementSpeed { get; set; } = 0.1F;
		public double FlyingSpeed { get; set; } = 0.4F;
		
		public int Data { get; set; }
		public UUID UUID { get; set; } = new UUID(Guid.Empty.ToByteArray());

		public bool CanFly { get; set; } = false;
		public bool IsFlying { get; set; } = false;

		public INetworkProvider Network { get; set; }
		public Inventory Inventory { get; protected set; }
		public Entity(int entityTypeId, World level, INetworkProvider network)
		{
			Network = network;

			EntityId = -1;
			Level = level;
			EntityTypeId = entityTypeId;
			KnownPosition = new PlayerLocation();
			Inventory = new Inventory(0);
		//	HealthManager = new HealthManager(this);
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
		public void Render(IRenderArgs renderArgs)
		{
			if (RenderEntity)
				ModelRenderer.Render(renderArgs, KnownPosition);
		}

		public virtual void Update(IUpdateArgs args)
		{
			var now = DateTime.UtcNow;

			ModelRenderer.Update(args, KnownPosition);

			if (now.Subtract(LastUpdatedTime).TotalMilliseconds >= 50)
			{
				LastUpdatedTime = now;
				try
				{
					OnTick();
				}
				catch(Exception e)
				{
					Log.Warn($"Exception while trying to tick entity!", e);
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
			if (feetBlock != null)
			{
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

			var headBlock = Level?.GetBlock(KnownPosition.GetCoordinates3D() + new BlockCoordinates(0, 1, 0));
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
			}

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
			double halfWidth = Width / 2D;

			return new BoundingBox(new Vector3((float)(pos.X - halfWidth), pos.Y, (float)(pos.Z - halfWidth)), new Vector3((float)(pos.X + halfWidth), (float)(pos.Y + Height), (float)(pos.Z + halfWidth)));
		}

		public byte GetDirection()
		{
			return DirectionByRotationFlat(KnownPosition.Yaw);
		}

		public static byte DirectionByRotationFlat(float yaw)
		{
			byte direction = (byte)((int)Math.Floor((yaw * 4F) / 360F + 0.5D) & 0x03);
			switch (direction)
			{
				case 0:
					return 1; // West
				case 1:
					return 2; // North
				case 2:
					return 3; // East
				case 3:
					return 0; // South 
			}
			return 0;
		}

		public virtual void Knockback(Vector3 velocity)
		{
			Velocity += velocity;
		}


		/*public virtual Item[] GetDrops()
		{
			return new Item[] { };
		}*/

		public virtual void DoInteraction(byte actionId, Player player)
		{
		}

		public virtual void DoMouseOverInteraction(byte actionId, Player player)
		{
		}

		public void RenderNametag(IRenderArgs renderArgs)
		{
			Vector2 textPosition;

			// calculate screenspace of text3d space position
			var screenSpace = renderArgs.GraphicsDevice.Viewport.Project(Vector3.Zero,
				renderArgs.Camera.ProjectionMatrix,
				renderArgs.Camera.ViewMatrix,
				Matrix.CreateTranslation(KnownPosition + new Vector3(0, (float)Height, 0)));


			// get 2D position from screenspace vector
			textPosition.X = screenSpace.X;
			textPosition.Y = screenSpace.Y;

			float s = 1f;
			var scale = new Vector2(s, s);
	
			string clean = NameTag;

			var stringCenter = Alex.Font.MeasureString(clean, s);
			var c = new Point((int)stringCenter.X, (int)stringCenter.Y);

			textPosition.X = (int)(textPosition.X - c.X);
			textPosition.Y = (int)(textPosition.Y - c.Y);

			renderArgs.SpriteBatch.FillRectangle(new Rectangle(textPosition.ToPoint(), c), new Color(Color.Black, 128));
			renderArgs.SpriteBatch.DrawString(Alex.Font, clean, textPosition, TextColor.White, FontStyle.None, 0f, Vector2.Zero, scale);
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