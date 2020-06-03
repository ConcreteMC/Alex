using System;
using Alex.API.Graphics;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.Graphics.Models.Entity;
using Alex.Net;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MathF = Alex.API.Utils.MathF;

namespace Alex.Entities
{
	public class PlayerMob : Mob
	{
		public UUID Uuid { get; private set; }

		public Gamemode Gamemode { get; private set; }

		private EntityModel _model;

		public string Name { get; }
		public string GeometryName { get; set; }
		public PlayerMob(string name, World level, NetworkProvider network, PooledTexture2D skinTexture, string geometry = "geometry.humanoid.customSlim") : base(63, level, network)
		{
			//DoRotationCalculations = false;
			Name = name;
			Uuid = new UUID(Guid.NewGuid().ToByteArray());

			Width = 0.6;
			Length = 0.6;
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

		public void UpdateGamemode(Gamemode gamemode)
		{
			Gamemode = gamemode;
		}

		public override void Update(IUpdateArgs args)
		{
			base.Update(args);
			//	DistanceMoved = 0f;
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
					UpdateModelParts();
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

		public override string ToString()
		{
			return
				$"Valid: {ModelRenderer.Valid} | {ModelRenderer.Texture.Height} x {ModelRenderer.Texture.Width} | Height: {Height} | Model: {GeometryName} -- {ValidModel}";
		}
	}
}
