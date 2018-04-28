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
using MathF = Alex.API.Utils.MathF;

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

		private EntityModelRenderer.ModelBone _leftArmModel;
		private EntityModelRenderer.ModelBone _rightArmModel;
		private EntityModelRenderer.ModelBone _leftLegModel;
		private EntityModelRenderer.ModelBone _rightLegModel;

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
					ModelRenderer = new EntityModelRenderer(_model, skinTexture);
					UpdateModelParts();
				}
			}
			else
			{
				if (Alex.Instance.Resources.BedrockResourcePack.EntityModels.TryGetValue("geometry.humanoid.custom",
					out EntityModel m))
				{
					_model        = m;
					ModelRenderer = new EntityModelRenderer(_model, skinTexture);
					UpdateModelParts();
				}
			}
		}

		private void UpdateModelParts()
		{
			ModelRenderer.GetBone("rightArm", out _rightArmModel);
			ModelRenderer.GetBone("leftArm", out _leftArmModel);
			ModelRenderer.GetBone("leftLeg", out _leftLegModel);
			ModelRenderer.GetBone("rightLeg", out _rightLegModel);

			/*foreach (var bone in _model.Bones)
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
			}*/
		}

		private float _armRotation = 0f;
		private float _legRotation = 0f;
		public override void Update(IUpdateArgs args)
		{
			base.Update(args);

			var dt = (float)args.GameTime.ElapsedGameTime.TotalSeconds;
			
			_armRotation += dt;
			//_armRotation = _armRotation;

			// Test arm rotations
			if (_leftArmModel != null && _rightArmModel != null)
			{
				_leftArmModel.Rotation = new Vector3((0.5f + MathF.Sin(_armRotation)) / 7.5f, 0f, 0.1f + (MathF.Cos(_armRotation) / 7.5f));
				_rightArmModel.Rotation = new Vector3((0.5f + MathF.Sin(_armRotation)) / -7.5f, 0f, -0.1f + (MathF.Cos(_armRotation) / -7.5f));
			}

			if (_leftLegModel != null && _rightLegModel != null)
			{
				if (IsMoving)
				{
					_legRotation += (MathF.Sqrt(Velocity.X * Velocity.X + Velocity.Z * Velocity.Z) * 0.23f) * dt;

					_leftLegModel.Rotation = new Vector3(MathF.Sin(_legRotation), 0f, 0f);
					_rightLegModel.Rotation = new Vector3(-MathF.Sin(_legRotation), 0f, 0f);
				}
				else
				{
					_legRotation = 0;
					_leftLegModel.Rotation = Vector3.Zero;
					_rightLegModel.Rotation = Vector3.Zero;
				}
			}

		}
	}
}
