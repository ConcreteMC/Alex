using Alex.Entities.Projectiles;
using Alex.Items;
using Alex.Networking.Java.Packets.Play;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Generic
{
	public class EntityFallingBlock : ItemEntity
	{
		/// <inheritdoc />
		public EntityFallingBlock(World level) : base(level)
		{
			Height = Width = 0.98;
			
			DoRotation = false;
			
			Gravity = 0.04;
			Drag = 0.02;
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 7 && entry is MetadataPosition position)
			{
				KnownPosition.X = position.Position.X;
				KnownPosition.Y = position.Position.Y;
				KnownPosition.Z = position.Position.Z;
			}
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
		public override void CollidedWithWorld(Vector3 direction, Vector3 position)
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
		}
	}
}