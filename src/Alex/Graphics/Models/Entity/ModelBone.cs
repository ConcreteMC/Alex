using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Entities;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models.Entity
{
	public partial class EntityModelRenderer
	{
		public class ModelBone : IDisposable
		{
			private PooledIndexBuffer Buffer { get; set; }
			public ModelBoneCube[] Cubes { get; }
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
			
			private List<IAttachable> Attachables { get; } = new List<IAttachable>();

			private string OriginalBone { get; }
			public string Parent => OriginalBone;
			
			public Queue<ModelBoneAnimation> Animations { get; }
			private ModelBoneAnimation CurrentAnim { get; set; } = null;
			public bool IsAnimating => CurrentAnim != null;
			internal EntityModelBone EntityModelBone { get; }
			public ModelBone(ModelBoneCube[] cubes, string parent, EntityModelBone bone)
			{
				EntityModelBone = bone;
				Cubes = cubes;
				OriginalBone = parent;

				Animations = new Queue<ModelBoneAnimation>();
			}

			private bool _isDirty = true;

			public Matrix RotationMatrix = Matrix.Identity;
			public bool UpdateRotationMatrix = true;
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
			
			public void Render(IRenderArgs args, PlayerLocation position, Matrix characterMatrix, bool mock)
			{
				if (Buffer == null)
					return;
				
				args.GraphicsDevice.Indices = Buffer;

				//var headYaw = MathUtils.ToRadians(-(position.HeadYaw - position.Yaw));
			//	var pitch = MathUtils.ToRadians(position.Pitch);
			
			var bindPoseMatrix = Matrix.CreateTranslation(-EntityModelBone.Pivot)
			                 * Matrix.CreateRotationX(MathUtils.ToRadians(-EntityModelBone.BindPoseRotation.X))
			                 * Matrix.CreateRotationY(MathUtils.ToRadians(-EntityModelBone.BindPoseRotation.Y))
			                 * Matrix.CreateRotationZ(MathUtils.ToRadians(-EntityModelBone.BindPoseRotation.Z))
			                 * Matrix.CreateTranslation(EntityModelBone.Pivot);
			
			var boneMatrix = Matrix.Identity 
			                 * Matrix.CreateTranslation(-EntityModelBone.Pivot)
			                 * Matrix.CreateFromAxisAngle(Vector3.Right, MathUtils.ToRadians(-EntityModelBone.Rotation.X ))
			                 * Matrix.CreateFromAxisAngle(Vector3.Backward, MathUtils.ToRadians(-EntityModelBone.Rotation.Z))
			                 * Matrix.CreateFromAxisAngle(Vector3.Up, MathUtils.ToRadians(-EntityModelBone.Rotation.Y))
			                 * Matrix.CreateTranslation(EntityModelBone.Pivot)
			                 * Matrix.CreateTranslation(_position);
				
				var headYaw = ApplyHeadYaw ? MathUtils.ToRadians(-(position.HeadYaw - position.Yaw)) : 0f;
				var pitch   = ApplyPitch ? MathUtils.ToRadians(position.Pitch) : 0f;				
				
				int idx = 0;
				for (var index = 0; index < Cubes.Length; index++)
				{
					var cube = Cubes[index];

					var effect = cube.Effect;
					if (effect == null) continue;

					Matrix cubeRotationMatrix = Matrix.CreateTranslation(-cube.Pivot)
					                            * Matrix.CreateFromAxisAngle(Vector3.Right, MathUtils.ToRadians(-cube.Rotation.X))
					                            * Matrix.CreateFromAxisAngle(Vector3.Backward, MathUtils.ToRadians(-cube.Rotation.Z))
					                            * Matrix.CreateFromAxisAngle(Vector3.Up, MathUtils.ToRadians(-cube.Rotation.Y))
					                            /* Matrix.CreateRotationX(MathUtils.ToRadians(part.Rotation.X))
					                            * Matrix.CreateRotationY(MathUtils.ToRadians(part.Rotation.Y))
					                            * Matrix.CreateRotationZ(MathUtils.ToRadians(part.Rotation.Z))*/
					                            * Matrix.CreateTranslation(cube.Pivot);


					var rotMatrix2 = Matrix.CreateTranslation(-EntityModelBone.Pivot) *
						Matrix.CreateFromYawPitchRoll(headYaw, pitch, 0f) *
					                 Matrix.CreateTranslation(EntityModelBone.Pivot);

					var rotMatrix3 = Matrix.CreateTranslation(-EntityModelBone.Pivot)
					                 * Matrix.CreateRotationX(MathUtils.ToRadians(-Rotation.X))
					                 * Matrix.CreateRotationY(MathUtils.ToRadians(-Rotation.Y))
					                 * Matrix.CreateRotationZ(MathUtils.ToRadians(-Rotation.Z))
					                 * Matrix.CreateTranslation(EntityModelBone.Pivot);
					
					var cubeMatrix = (cubeRotationMatrix) * Matrix.CreateTranslation(cube.Origin);
					
					RotationMatrix = cubeMatrix * boneMatrix * characterMatrix;

					effect.World = cubeMatrix * rotMatrix2 * rotMatrix3 * bindPoseMatrix * boneMatrix * characterMatrix;
					effect.View = args.Camera.ViewMatrix;
					effect.Projection = args.Camera.ProjectionMatrix;

					if (!mock && !cube.IsInvisible)
					{
						foreach (var pass in effect.CurrentTechnique.Passes)
						{
							pass.Apply();
						}

						args.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, idx,
							cube.Indexes.Length / 3);
					}

					idx += cube.Indexes.Length;
				}

				foreach (var attach in Attachables.ToArray())
				{
					attach.Render(args);
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

			public void Update(IUpdateArgs args, Matrix characterMatrix, Vector3 diffuseColor)
			{
				if (CurrentAnim == null && Animations.TryDequeue(out var animation))
				{
					animation.Setup();
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

				CharacterMatrix = characterMatrix;
				foreach (var part in Cubes)
				{
					if (part.Effect != null)
					{
						part.Effect.DiffuseColor = diffuseColor;
					}

					if (!part.IsDirty)
						continue;

					_isDirty = true;
					part.Update(args);
				}

				foreach (var attachable in Attachables.ToArray())
				{
					//attachable.R
					//attachable.Update(RotationMatrix);
				}

				if (_isDirty)
				{
					UpdateVertexBuffer(args.GraphicsDevice);
				}
			}

			private void UpdateVertexBuffer(GraphicsDevice device)
			{
				var indices = Cubes.SelectMany(x => x.Indexes).ToArray();

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

			public void Attach(IAttachable attachable)
			{
				if (!Attachables.Contains(attachable))
					Attachables.Add(attachable);
			}

			public void Detach(IAttachable attachable)
			{
				if (Attachables.Contains(attachable))
					Attachables.Remove(attachable);
			}
			
			public void Dispose()
			{
				Buffer?.MarkForDisposal();
			}
		}
	}
}
