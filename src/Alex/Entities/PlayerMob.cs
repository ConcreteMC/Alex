using System;
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

		public Gamemode Gamemode { get; private set; }

		private EntityModel _model;

		private EntityModelRenderer.ModelBone _leftArmModel;
		private EntityModelRenderer.ModelBone _rightArmModel;

		private EntityModelRenderer.ModelBone _leftLegModel;
		private EntityModelRenderer.ModelBone _rightLegModel;

		private EntityModelRenderer.ModelBone _leftSleeveModel;
		private EntityModelRenderer.ModelBone _rightSleeveModel;

		private EntityModelRenderer.ModelBone _leftPantsModel;
		private EntityModelRenderer.ModelBone _rightPantsModel;

		private EntityModelRenderer.ModelBone _jacketModel;
		private EntityModelRenderer.ModelBone _body;
		private EntityModelRenderer.ModelBone _head;

		public string Name { get; }
		public PlayerMob(string name, World level, INetworkProvider network, Texture2D skinTexture, bool skinSlim = true) : base(63, level, network)
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

			UpdateSkin(skinTexture, skinSlim);
			
			//Inventory = new Inventory(46);
		}

		public void UpdateGamemode(Gamemode gamemode)
		{
			Gamemode = gamemode;
		}

		private void UpdateModelParts()
		{
			if (ModelRenderer == null)
				return;
			
			ModelRenderer.GetBone("body", out _body);
			ModelRenderer.GetBone("rightArm", out _rightArmModel);
			ModelRenderer.GetBone("leftArm", out _leftArmModel);

			ModelRenderer.GetBone("leftLeg", out _leftLegModel);
			ModelRenderer.GetBone("rightLeg", out _rightLegModel);

			ModelRenderer.GetBone("leftSleeve", out _leftSleeveModel);
			ModelRenderer.GetBone("rightSleeve", out _rightSleeveModel);

			ModelRenderer.GetBone("leftPants", out _leftPantsModel);
			ModelRenderer.GetBone("rightPants", out _rightPantsModel);

			ModelRenderer.GetBone("jacket", out _jacketModel);
			ModelRenderer.GetBone("head", out _head);

			if (ModelRenderer.GetBone("hat", out EntityModelRenderer.ModelBone hat))
			{
				foreach (var c in hat.Parts)
				{
					c.ApplyHeadYaw = true;
					c.ApplyYaw = false;
					c.ApplyPitch = true;
				}
			}
		}

		private Vector3 _prevUpdatePosition = Vector3.Zero;
		private float _armRotation = 0f;
		private float _legRotation = 0f;
		private float DistanceMoved { get; set; } = 0;
		public override void Update(IUpdateArgs args)
		{
			base.Update(args);

			var dt = (float)args.GameTime.ElapsedGameTime.TotalSeconds;

			if (IsSneaking)
			{
				_body.Rotation = new Vector3(-35f, _body.Rotation.Y, _body.Rotation.Z);
				_body.Position = Vector3.Forward * 7.5f;
				//_head.Position = new Vector3(_body.Position.X, 0.25f, 0f);
				
				_leftArmModel.Rotation = new Vector3(72f, 0f,0f);
				_rightArmModel.Rotation = new Vector3(72f, 0f,0f);

			}
			else
			{
				_body.Position = Vector3.Zero;
				_body.Rotation = new Vector3(0f);


				var moveSpeed = MovementSpeed;
				var tcos0 = (float) (Math.Cos(DistanceMoved * 38.17) * moveSpeed) * 57.3f;
				var tcos1 = -tcos0;

				var pos = KnownPosition.ToVector3();
				float deltaX = pos.X - _prevUpdatePosition.X;
				float deltaZ = pos.Z - _prevUpdatePosition.Z;
				float distSQ = deltaX * deltaX + deltaZ * deltaZ;

				//_armRotation = _armRotation;

				// Test arm rotations
				if (_leftArmModel != null && _rightArmModel != null)
				{
					//var lArmRot = new Vector3((0.5f + MathF.Sin(_armRotation)) * 7.5f, 0f,
					//	0.1f + (MathF.Cos(_armRotation) * 1.5f));
					Vector3 rArmRot = Vector3.Zero;
					var lArmRot = new Vector3(tcos0, 0, 0);
					if (distSQ > 0f)
					{
						_armRotation += (float) ((new Vector3(Velocity.X, 0, Velocity.Z).Length()) + distSQ) * dt;

						rArmRot = new Vector3((0.5f + MathF.Cos(_armRotation)) * 24.5f, 0, 0);
					}
					else
					{
						_armRotation += dt;
						rArmRot = new Vector3((0.5f + MathF.Cos(_armRotation)) * -7.5f, 0f,
							0.1f + (MathF.Sin(_armRotation) * -1.5f));
					}

					_leftArmModel.Rotation = rArmRot;
					_rightArmModel.Rotation = -rArmRot;
					_rightSleeveModel.Rotation = -rArmRot;
					_leftSleeveModel.Rotation = rArmRot;
				}


				if (_leftLegModel != null && _rightLegModel != null)
				{
					Vector3 lLegRot = Vector3.Zero;
					Vector3 rLegRot = Vector3.Zero;

					if (distSQ > 0f)
					{
						_legRotation += (float) ((new Vector3(Velocity.X, 0, Velocity.Z).Length()) + distSQ) * dt;

						lLegRot = new Vector3(MathF.Sin(_legRotation) * 34.5f, 0f, 0f);
						rLegRot = new Vector3(-MathF.Sin(_legRotation) * 34.5f, 0f, 0f);

						_prevUpdatePosition = pos;
					}
					else
					{
						_legRotation = 0f;
					}

					_leftLegModel.Rotation = lLegRot;
					_rightLegModel.Rotation = rLegRot;
					_leftPantsModel.Rotation = lLegRot;
					_rightPantsModel.Rotation = rLegRot;
				}
			}
		}

		public override void OnTick()
		{
			base.OnTick();

			
		}

		internal void UpdateSkin(Texture2D skinTexture, bool skinSlim)
		{
			if (skinSlim)
			{
				if (ModelFactory.TryGetModel("geometry.humanoid.customSlim",
					out EntityModel m))
				{
					_model = m;
					ModelRenderer = new EntityModelRenderer(_model, skinTexture);
					UpdateModelParts();
				}
			}
			else
			{
				if (ModelFactory.TryGetModel("geometry.humanoid.custom",
					out EntityModel m))
				{
					_model = m;
					ModelRenderer = new EntityModelRenderer(_model, skinTexture);
					UpdateModelParts();
				}
			}
		}
	}
}
