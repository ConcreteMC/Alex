using System.Drawing;
using Alex.Entities.Generic;
using Alex.Items;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Projectiles
{
	public class PrimedTntEntity : EntityFallingBlock
	{
		private int _fuse = 80;

		/// <inheritdoc />
		public PrimedTntEntity(World level) : base(level)
		{
			
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
			if (entry.Index == 7 && entry is MetadataVarInt metadataVarInt)
			{
				Fuse = metadataVarInt.Value;

				return;
			}
			
			base.HandleJavaMeta(entry);
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
					//Explode!
				}
			}
		}
	}
}