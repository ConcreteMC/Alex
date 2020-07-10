using System;
using Alex.API.Graphics;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.Entities.Properties;
using Alex.Graphics.Models.Entity;
using Alex.Net;
using Alex.Networking.Java.Packets.Play;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET;
using MathF = Alex.API.Utils.MathF;

namespace Alex.Entities
{
	public class RemotePlayer : LivingEntity
	{
		public UUID Uuid { get; private set; }

		public Gamemode Gamemode { get; private set; }

		private EntityModel _model;

		public string Name { get; }
		public string GeometryName { get; set; }
		
		public bool IsFirstPersonMode { get; set; } = false;
		public PlayerSkinFlags SkinFlags { get; }

		public int Score { get; set; } = 0;
		
		public RemotePlayer(string name, World level, NetworkProvider network, PooledTexture2D skinTexture, string geometry = "geometry.humanoid.customSlim") : base(63, level, network)
		{
			//DoRotationCalculations = false;
			Name = name;
			Uuid = new UUID(Guid.NewGuid().ToByteArray());

			Width = 0.6;
			//Length = 0.6;
			Height = 1.80;

			IsSpawned = false;

			NameTag = name;
			//Skin = new Skin { Slim = false, SkinData = Encoding.Default.GetBytes(new string('Z', 8192)) };

		//	ItemInHand = new ItemAir();

			HideNameTag = false;
			IsAlwaysShowName = true;
			ShowItemInHand = true;
			
			IsInWater = false;
			NoAi = true;
			//HealthManager.IsOnFire = false;
			Velocity = Vector3.Zero;
			PositionOffset = 1.62f;

			GeometryName = geometry;
			UpdateSkin(skinTexture);
			//Inventory = new Inventory(46);
			
			MovementSpeed = 0.1f;
			FlyingSpeed = 0.4f;
			
			SkinFlags = new PlayerSkinFlags()
			{
				Value = 0
			};
		}

		/// <inheritdoc />
		public override bool NoAi {
			get
			{
				return true;
			}
			set
			{
				
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

		public void UpdateGamemode(Gamemode gamemode)
		{
			Gamemode = gamemode;
		}

		public override void Update(IUpdateArgs args)
		{
			base.Update(args);
			//	DistanceMoved = 0f;
		}

		/// <inheritdoc />
		protected override void OnModelUpdated()
		{
			base.OnModelUpdated();

			var modelRenderer = ModelRenderer;
			if (modelRenderer != null && SkinFlags != null)
				SkinFlags.ApplyTo(modelRenderer);
		}

		private bool ValidModel { get; set; }
		internal void UpdateSkin(PooledTexture2D skinTexture)
		{
			//if (skinSlim)
			{
				var gotModel = ModelFactory.TryGetModel(GeometryName,
					out EntityModel m);
				ValidModel = gotModel;
				if (gotModel || ModelFactory.TryGetModel("geometry.humanoid.customSlim", out m))
				{
					if (!gotModel)
					{
						
					}
					_model = m;
					ModelRenderer = new EntityModelRenderer(_model, skinTexture);
					//UpdateModelParts();
				}
			}
			/*else
			{
				if (ModelFactory.TryGetModel("geometry.humanoid.custom",
					out EntityModel m))
				{
					_model = m;
					ModelRenderer = new EntityModelRenderer(_model, skinTexture);
					UpdateModelParts();
				}
			}*/
		}

		/*public override string ToString()
		{
			return
				$"Valid: {ModelRenderer.Valid} | {ModelRenderer.Texture.Height} x {ModelRenderer.Texture.Width} | Height: {Height} | Model: {GeometryName} -- {ValidModel}";
		}
	*/
	}

	public class PlayerSkinFlags
	{
		public byte Value { get; set; } = 0;

		public bool CapeEnabled => (Value & 0x01) != 0;
		public bool JacketEnabled => (Value & 0x02) != 0;
		public bool LeftSleeveEnabled => (Value & 0x04) != 0;
		public bool RightSleeveEnabled => (Value & 0x08) != 0;
		public bool LeftPantsEnabled => (Value & 0x10) != 0;
		public bool RightPantsEnabled => (Value & 0x20) != 0;
		public bool HatEnabled => (Value & 0x40) != 0;
		
		public void ApplyTo(EntityModelRenderer renderer)
		{
			Set(renderer, "cape", CapeEnabled);
			Set(renderer, "jacket", JacketEnabled);
			Set(renderer, "leftSleeve", LeftSleeveEnabled);
			Set(renderer, "rightSleeve", RightSleeveEnabled);
			Set(renderer, "leftPants", LeftPantsEnabled);
			Set(renderer, "rightPants", RightPantsEnabled);
			Set(renderer, "hat", HatEnabled);
		}

		private void Set(EntityModelRenderer renderer, string bone, bool value)
		{
			if (renderer.GetBone(bone, out var boneValue))
			{
				boneValue.Rendered = value;
			}
		}
	}
}
