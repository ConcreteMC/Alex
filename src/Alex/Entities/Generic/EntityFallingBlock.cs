using Alex.Common.Utils.Vectors;
using Alex.Entities.Projectiles;
using Alex.Items;
using Alex.Networking.Java.Packets.Play;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Worlds;
using Alex.Worlds.Multiplayer.Bedrock;
using Microsoft.Xna.Framework;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Utils.Metadata;
using NLog;

namespace Alex.Entities.Generic
{
	public class EntityFallingBlock : ItemEntity
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(EntityFallingBlock));
		
		/// <inheritdoc />
		public EntityFallingBlock(World level) : base(level)
		{
			Height = Width = 0.98;
			
			DoRotation = false;
			
			Gravity = 0.04;
			Drag = 0.02;
			
			HasPhysics = true;
			IsAffectedByGravity = true;
			NoAi = false;
		}

		/// <inheritdoc />
		public override PlayerLocation KnownPosition
		{
			get
			{
				return base.KnownPosition + new Vector3(-0.5f, 0f, -0.5f);
			}
			set
			{
				base.KnownPosition = value;
			}
		}

		/// <inheritdoc />
		protected override float GetScale()
		{
			return 1f;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 7 && entry is MetadataPosition position)
			{
				RenderLocation.X = KnownPosition.X = position.Position.X;
				RenderLocation.Y = KnownPosition.Y = position.Position.Y;
				RenderLocation.Z = KnownPosition.Z = position.Position.Z;
			}
		}

		/// <inheritdoc />
		protected override bool HandleMetadata(MiNET.Entities.Entity.MetadataFlags flag, MetadataEntry entry)
		{
			if (flag == MiNET.Entities.Entity.MetadataFlags.Variant && entry is MetadataInt mdi)
			{
				var blockState = ChunkProcessor.Instance.GetBlockState((uint) mdi.Value);

				if (blockState == null)
				{
					Log.Warn($"Could not find block! Lookup={(uint)mdi.Value} Original={mdi.Value}");
					return true;
				}

				if (ItemFactory.TryGetItem(blockState.Name, out var item))
				{
					SetItem(item);
				}
				else
				{
					Log.Info($"Could not get item: {blockState.Name}");
				}

				return true;
			}

			return base.HandleMetadata(flag, entry);
		}

		/// <inheritdoc />
		public override void SetItem(Item item)
		{
			base.SetItem(item);

			if (ItemRenderer != null)
			{
				ItemRenderer.DisplayPosition = DisplayPosition.Undefined;
			}
		}

		/// <inheritdoc />
		public override float CollidedWithWorld(Vector3 direction, Vector3 position, float impactVelocity)
		{
			if (StopOnImpact)
			{
				if (direction == Vector3.Down)
				{
					Velocity = new Vector3(Velocity.X, 0f, Velocity.Z);
				}
				else if (direction == Vector3.Left || Velocity == Vector3.Right)
				{
					Velocity = new Vector3(0, Velocity.Y, Velocity.Z);
				}
				else if (direction == Vector3.Forward || Velocity == Vector3.Backward)
				{
					Velocity = new Vector3(Velocity.X, Velocity.Y, 0);
				}

				return 0f;
			}

			return base.CollidedWithWorld(direction, position, impactVelocity);
		}
	}
}