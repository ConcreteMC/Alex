using System;
using System.Collections;
using Alex.API.Entities;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.Graphics.Models;
using Alex.Graphics.Models.Entity;
using Alex.Rendering;
using Alex.Rendering.Camera;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Entities
{
	public class Entity : IEntity
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
		public Vector3 Velocity { get; set; }
		public float PositionOffset { get; set; }

		//public HealthManager HealthManager { get; set; }

		public string NameTag { get; set; }
		
		public bool NoAi { get; set; }
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
		public double Drag { get; set; } = 0.02;
		public double Gravity { get; set; } = 0.08;

		public int Data { get; set; }
		public UUID UUID { get; set; } = new UUID(Guid.Empty.ToByteArray());

		public double MovementSpeed { get; set; } = 0.699999988079071;
		public double FlyingSpeed { get; set; } = 0.4000000059604645;

		public Entity(int entityTypeId, World level)
		{
			EntityId = -1;
			Level = level;
			EntityTypeId = entityTypeId;
			KnownPosition = new PlayerLocation();
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

		public enum DataFlags
		{
			OnFire = 0,
			Sneaking,
			Riding,
			Sprinting,
			UsingItem,
			Invisible,
			Tempted,
			InLove,

			Saddled,
			Powered,
			Ignited,
			Baby,
			Converting,
			Critcal,
			ShowName,
			AlwaysShowName,

			NoAi,
			Silent,
			WallClimbing,
			Resting,
			Sitting,
			Angry,
			Interested,
			Charged,

			Tamed,
			Leashed,
			Sheared,
			FlagAllFlying,
			Elder,
			Moving,
			Breathing,
			Chested,

			Stackable,
		}

		protected virtual BitArray GetFlags()
		{
			IsFlagAllFlying = false;

			BitArray bits = new BitArray(64);
			//bits[(int)DataFlags.OnFire] = HealthManager.IsOnFire;
			bits[(int)DataFlags.Sneaking] = IsSneaking;
			bits[(int)DataFlags.Riding] = IsRiding;
			bits[(int)DataFlags.Sprinting] = IsSprinting;
			bits[(int)DataFlags.UsingItem] = IsUsingItem;
			bits[(int)DataFlags.Invisible] = IsInvisible;
			bits[(int)DataFlags.Tempted] = IsTempted;
			bits[(int)DataFlags.InLove] = IsInLove;
			bits[(int)DataFlags.Saddled] = IsSaddled;
			bits[(int)DataFlags.Powered] = IsPowered;
			bits[(int)DataFlags.Ignited] = IsIgnited;
			bits[(int)DataFlags.Baby] = IsBaby;
			bits[(int)DataFlags.Converting] = IsConverting;
			bits[(int)DataFlags.Critcal] = IsCritical;
			bits[(int)DataFlags.ShowName] = IsShowName;
			bits[(int)DataFlags.AlwaysShowName] = IsAlwaysShowName;
			bits[(int)DataFlags.NoAi] = IsNoAi;
			bits[(int)DataFlags.Silent] = IsSilent;
			bits[(int)DataFlags.WallClimbing] = IsWallClimbing;
			bits[(int)DataFlags.Resting] = IsResting;
			bits[(int)DataFlags.Sitting] = IsSitting;
			bits[(int)DataFlags.Angry] = IsAngry;
			bits[(int)DataFlags.Interested] = IsInterested;
			bits[(int)DataFlags.Charged] = IsCharged;
			bits[(int)DataFlags.Tamed] = IsTamed;
			bits[(int)DataFlags.Leashed] = IsLeashed;
			bits[(int)DataFlags.Sheared] = IsSheared;
			bits[(int)DataFlags.FlagAllFlying] = IsFlagAllFlying;
			bits[(int)DataFlags.Elder] = IsElder;
			bits[(int)DataFlags.Moving] = IsMoving;
			bits[(int)DataFlags.Breathing] = IsBreathing;
			bits[(int)DataFlags.Chested] = IsChested;
			bits[(int)DataFlags.Stackable] = IsStackable;

			return bits;
		}

		public void Render(IRenderArgs renderArgs)
		{
			ModelRenderer.Render(renderArgs, KnownPosition);

		}

		public virtual void Update(IUpdateArgs args)
		{
			ModelRenderer.Update(args, KnownPosition);
		}

		public virtual void OnTick()
		{
			Age++;

			//HealthManager.OnTick();
		}

		private void CheckBlockCollisions()
		{
			// Check all blocks within entity BB
		}

		public virtual void SpawnEntity()
		{
			//Level.AddEntity(this);

			IsSpawned = true;
		}

		public virtual void DespawnEntity()
		{
			//Level.RemoveEntity(this);
			IsSpawned = false;
		}

		public BoundingBox GetBoundingBox()
		{
			var pos = KnownPosition;
			double halfWidth = Width / 2;

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

			float s = 0.5f;
			var scale = new Vector2(s, s);

			string clean = NameTag.StripIllegalCharacters();

			var stringCenter = Alex.Font.MeasureString(clean) * s;
			var c = new Point((int)stringCenter.X, (int)stringCenter.Y);

			textPosition.X = (int)(textPosition.X - c.X);
			textPosition.Y = (int)(textPosition.Y - c.Y);

			renderArgs.SpriteBatch.FillRectangle(new Rectangle(textPosition.ToPoint(), c), new Color(Color.Black, 128));
			renderArgs.SpriteBatch.DrawString(Alex.Font, clean, textPosition, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
		}
	}
}