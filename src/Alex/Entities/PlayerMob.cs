using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.Graphics.Models.Entity;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Entities
{
	public class PlayerMob : Mob
	{
		public UUID Uuid { get; private set; }
		public Skin Skin { get; set; }

		public short Boots { get; set; }
		public short Leggings { get; set; }
		public short Chest { get; set; }
		public short Helmet { get; set; }

		public Gamemode Gamemode { get; set; }

		//public Item ItemInHand { get; set; }

		private EntityModel _model;

		private EntityModelBone _leftArmModel;
		private EntityModelBone _rightArmModel;
		private EntityModelBone _leftLegModel;
		private EntityModelBone _rightLegModel;

		public PlayerMob(string name, World level, INetworkProvider network, Texture2D skinTexture, bool skinSlim = true) : base(63, level, network)
		{
			Uuid = new UUID(Guid.NewGuid().ToByteArray());

			Width = 0.6;
			Length = 0.6;
			Height = 1.80;

			IsSpawned = false;

			NameTag = name;
			Skin = new Skin { Slim = false, SkinData = Encoding.Default.GetBytes(new string('Z', 8192)) };

		//	ItemInHand = new ItemAir();

			HideNameTag = false;
			IsAlwaysShowName = true;

			IsInWater = false;
			NoAi = true;
			//HealthManager.IsOnFire = false;
			Velocity = Vector3.Zero;
			PositionOffset = 1.62f;

			if(skinSlim){
				if (Alex.Instance.Resources.BedrockResourcePack.EntityModels.TryGetValue("geometry.humanoid.customSlim",
					out EntityModel m))
				{
					_model = m;
					UpdateModelParts();
					ModelRenderer = new EntityModelRenderer(_model, skinTexture);
				}
			}
			else
			{
				if (Alex.Instance.Resources.BedrockResourcePack.EntityModels.TryGetValue("geometry.humanoid.custom",
					out EntityModel m))
				{
					_model        = m;
					UpdateModelParts();
					ModelRenderer = new EntityModelRenderer(_model, skinTexture);
				}
			}
		}

		private void UpdateModelParts()
		{
			foreach (var bone in _model.Bones)
			{
				if (bone.Name.Contains("rightArm"))
				{
					_rightArmModel = bone;
				}
				else if (bone.Name.Contains("leftArm"))
				{
					_leftArmModel = bone;
				}
				else if (bone.Name.Contains("leftLeg"))
				{
					_leftLegModel = bone;
				}
				else if (bone.Name.Contains("rightLeg"))
				{
					_rightLegModel = bone;
				}
			}
		}

		private double _armRotation;
		public override void Update(IUpdateArgs args)
		{
			base.Update(args);

			var dt = args.GameTime.ElapsedGameTime.TotalSeconds / 20.0d;

			// Test arm rotations
			if (_leftArmModel != null && _rightArmModel != null)
			{
				_armRotation += 0.2f * Math.Sin(dt * 10.0f);

				_leftArmModel.Rotation = new Vector3((float)Math.Sin(_armRotation), (float)Math.Cos(_armRotation), 0f);

				_rightArmModel.Rotation = new Vector3((float)Math.Sin(-_armRotation), (float)Math.Cos(-_armRotation), 0f);
			}

		}
	}
}
