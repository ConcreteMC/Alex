using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Entities;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models.Entity
{
	public partial class EntityModelRenderer
	{
		public class ModelBone : IDisposable
		{
			private Texture2D Texture { get; set; }
			private PooledIndexBuffer Buffer { get; set; }
			public ModelBone[] Children { get; internal set; } = new ModelBone[0];
			
			private Vector3 _rotation = Vector3.Zero;

			public Vector3 Rotation
			{
				get { return _rotation; }
				set { _rotation = value; }
			}

			private Vector3 _position = Vector3.Zero;
			public Vector3 Position
			{
				get { return _position; }
				set { _position = value; }
			}

			public bool Rendered { get; set; } = true;

			private string OriginalBone { get; }
			public string ParentName => OriginalBone;

			public ModelBone Parent { get; set; } = null;
			
			public Queue<ModelBoneAnimation> Animations { get; }
			private ModelBoneAnimation CurrentAnim { get; set; } = null;
			public bool IsAnimating => CurrentAnim != null || Animations.Count > 0;
			internal EntityModelBone EntityModelBone { get; }
			
			private short[] Indices { get; }
			public ModelBone(Texture2D texture, short[] indices, string parent, EntityModelBone bone, Matrix defaultMatrix)
			{
				Texture = texture;
				EntityModelBone = bone;
				Indices = indices;
				OriginalBone = parent;

				Animations = new Queue<ModelBoneAnimation>();

				DefaultMatrix = defaultMatrix;
			}

			private bool _isDirty = true;
			private Matrix CharacterMatrix { get; set; }

			private bool _applyHeadYaw = false;
			private bool _applyPitch = false;

			public bool ApplyHeadYaw
			{
				get
				{
					return _applyHeadYaw;
				}
				set
				{
					_applyHeadYaw = value;
					
					foreach (var child in Children)
					{
						child.ApplyHeadYaw = value;
					}
				}
			}

			public bool ApplyPitch 
			{
				get
				{
					return _applyPitch;
				}
				set
				{
					_applyPitch = value;

					foreach (var child in Children)
					{
						child.ApplyPitch = value;
					}
				}
			}
			
			public AlphaTestEffect Effect { get; private set; }

			public void Render(IRenderArgs args, bool mock)
			{
				if (Buffer == null || Effect == null || Effect.Texture == null)
					return;

				var effect = Effect;
				args.GraphicsDevice.Indices = Buffer;

				effect.View = args.Camera.ViewMatrix;
				effect.Projection = args.Camera.ProjectionMatrix;
				
				if (!mock && Rendered && !EntityModelBone.NeverRender)
				{
					foreach (var pass in effect.CurrentTechnique.Passes)
					{
						pass.Apply();
					}

					args.GraphicsDevice.DrawIndexedPrimitives(
						PrimitiveType.TriangleList, 0, 0, Indices.Length / 3);
				}
									
				var children = Children;

				if (children.Length > 0)
				{
					foreach (var child in children)
					{
						child.Render(args, mock);
					}
				}
			}

			public void ClearAnimations()
			{
				var anim = CurrentAnim;

				if (anim != null)
				{
					anim.Reset();
					CurrentAnim = null;
				}
			}

			private Matrix DefaultMatrix { get; set; } = Matrix.Identity;

			public void Update(IUpdateArgs args,
				Matrix characterMatrix,
				Vector3 diffuseColor,
				PlayerLocation modelLocation)
			{
				var device = args.GraphicsDevice;

				if (Effect == null)
				{
					Effect = new AlphaTestEffect(device);
					Effect.Texture = Texture;
				}
				else
				{

					if (CurrentAnim == null && Animations.TryDequeue(out var animation))
					{
						animation.Setup();
						animation.Start();

						CurrentAnim = animation;
					}

					if (CurrentAnim != null)
					{
						CurrentAnim.Update(args.GameTime);

						if (CurrentAnim.IsFinished())
						{
							CurrentAnim.Reset();
							CurrentAnim = null;
						}
					}



					Matrix yawPitchMatrix = Matrix.Identity;

					if (ApplyHeadYaw || ApplyPitch)
					{
						var headYaw = ApplyHeadYaw ? MathUtils.ToRadians(-(modelLocation.HeadYaw - modelLocation.Yaw)) :
							0f;

						var pitch = ApplyPitch ? MathUtils.ToRadians(modelLocation.Pitch) : 0f;

						yawPitchMatrix = Matrix.CreateTranslation(-EntityModelBone.Pivot)
						                 * Matrix.CreateFromYawPitchRoll(headYaw, pitch, 0f)
						                 * Matrix.CreateTranslation(EntityModelBone.Pivot);
					}

					var userRotationMatrix = Matrix.CreateTranslation(-EntityModelBone.Pivot)
					                         * Matrix.CreateRotationX(MathUtils.ToRadians(Rotation.X))
					                         * Matrix.CreateRotationY(MathUtils.ToRadians(Rotation.Y))
					                         * Matrix.CreateRotationZ(MathUtils.ToRadians(Rotation.Z))
					                         * Matrix.CreateTranslation(EntityModelBone.Pivot);

					Effect.World = yawPitchMatrix * userRotationMatrix * DefaultMatrix
					               * Matrix.CreateTranslation(_position) * characterMatrix;

					Effect.DiffuseColor = diffuseColor;
					var children = Children;

					if (children.Length > 0)
					{
						foreach (var child in children)
						{
							child.Update(args, userRotationMatrix * characterMatrix, diffuseColor, modelLocation);
						}
					}

					if (_isDirty)
					{
						UpdateVertexBuffer(args.GraphicsDevice);
					}
				}
			}

			private void UpdateVertexBuffer(GraphicsDevice device)
			{
				var indices = Indices;

				PooledIndexBuffer currentBuffer = Buffer;

				if (indices.Length > 0 && (Buffer == null || currentBuffer.IndexCount != indices.Length))
				{
					PooledIndexBuffer buffer = GpuResourceManager.GetIndexBuffer(this, device, IndexElementSize.SixteenBits,
						indices.Length, BufferUsage.None);

					buffer.SetData(indices);
					Buffer = buffer;
					
					currentBuffer?.MarkForDisposal();
				}
			}

			internal void SetTexture(PooledTexture2D texture)
			{
				if (Effect != null)
				{
					Effect.Texture = texture;
				}

				Texture = texture;
			}

			public void Dispose()
			{
				Effect?.Dispose();
				Buffer?.MarkForDisposal();
			}
		}
	}
}
