using System.Drawing;
using Alex.Entities.Generic;
using Alex.Items;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using MiNET.Utils;
using MiNET.Utils.Metadata;

namespace Alex.Entities.Projectiles
{
	public class PrimedTntEntity : EntityFallingBlock
	{
		private int _fuse = 80;

		/// <inheritdoc />
		public PrimedTntEntity(World level) : base(level)
		{
			Width = 0.98f;
			Height = 0.98f;
			Gravity = 0.04f;
			Drag = 0.02f;

			HasPhysics = true;
			IsAffectedByGravity = true;
			HasCollision = true;
			NoAi = false;
			IsIgnited = true;
		}

		private int Fuse
		{
			get
			{
				return _fuse;
			}
			set
			{
				_fuse = value;

				var modelRenderer = ModelRenderer;

				if (modelRenderer != null)
				{
					if (_fuse % 20 == 0)
					{
						modelRenderer.DiffuseColor = new Vector3(0f);
					}
					else if (_fuse % 20 == 10)
					{
						modelRenderer.DiffuseColor = new Vector3(1f);
					}
				}
			}
		}

		/// <inheritdoc />
		public override void OnSpawn()
		{
			base.OnSpawn();

			if (ItemFactory.TryGetItem("minecraft:tnt", out var tnt))
			{
				SetItem(tnt);
			}
		}

		/// <inheritdoc />
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			if (entry.Index == 8 && entry is MetadataVarInt metadataVarInt)
			{
				Fuse = metadataVarInt.Value;

				return;
			}

			base.HandleJavaMeta(entry);
		}

		/// <inheritdoc />
		protected override bool HandleMetadata(MiNET.Entities.Entity.MetadataFlags flag, MetadataEntry entry)
		{
			if (flag == MiNET.Entities.Entity.MetadataFlags.DataFuseLength && entry is MetadataInt fuseData)
			{
				Fuse = fuseData.Value;

				return true;
			}

			return base.HandleMetadata(flag, entry);
		}

		/// <inheritdoc />
		public override void OnTick()
		{
			if (!IsSpawned)
				return;

			base.OnTick();

			if (Fuse > 0)
			{
				Fuse--;

				if (Fuse == 0)
				{
					Alex.Instance.AudioEngine.PlaySound("random.explode", KnownPosition.ToVector3(), 1f, 1f);
					//Level.
					//Explode!
				}
			}
		}
	}
}