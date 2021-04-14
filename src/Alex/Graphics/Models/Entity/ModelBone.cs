using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.Utils.Collections;
using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models.Entity
{
	public partial class EntityModelRenderer
	{
		public class ModelBone : IAttached
		{
			internal ThreadSafeList<IAttached> Children { get; set; } = new ThreadSafeList<IAttached>();

			public Vector3 Position
			{
				get => _position;
				set
				{
					_targetPosition = _position = value;
					UpdateTransform();
				}
			}

			private Vector3 _rotation = Vector3.Zero;

			public Vector3 Rotation
			{
				get { return _rotation; }
				set
				{
					_targetRotation = _rotation = value;
					
					UpdateTransform();
				}
			}

			private Vector3 _scale = Vector3.One;

			public Vector3 Scale
			{
				get { return _scale; }
				set
				{
					_targetScale = _scale = value;
					UpdateTransform();
				}
			}

			private Vector3 _bindingRotation = Vector3.Zero;

			public Vector3 BindingRotation
			{
				get { return _bindingRotation; }
				set
				{
					_bindingRotation = value;
					UpdateBindingMatrix();
				}
			}

			private Matrix Transform { get; set; } = Matrix.Identity;

			private void UpdateTransform()
			{
				Matrix matrix;
				if (Pivot.HasValue)
				{
					var pivot = (Pivot ?? Vector3.Zero);

					matrix = Matrix.CreateTranslation(-pivot) * MatrixHelper.CreateRotationDegrees(_rotation)
					                                          * Matrix.CreateTranslation(pivot)
					                                          * Matrix.CreateTranslation(_position);
				}
				else
				{
					matrix = MatrixHelper.CreateRotationDegrees(_rotation) * Matrix.CreateTranslation(_position);
				}

				Transform = matrix;
			}
			
			private Matrix BindingMatrix { get; set; } = Matrix.Identity;

			private void UpdateBindingMatrix()
			{
				Matrix matrix;
				var bindingRotation = _bindingRotation;

				if (Pivot.HasValue)
				{
					var pivot = (Pivot ?? Vector3.Zero);

					matrix = Matrix.CreateTranslation(-pivot)
					              * MatrixHelper.CreateRotationDegrees(bindingRotation)
					              * Matrix.CreateTranslation(pivot);
				}
				else
				{
					matrix = MatrixHelper.CreateRotationDegrees(bindingRotation);
				}

				BindingMatrix = matrix;
			}
			
			public bool Rendered { get; set; } = true;

			public IAttached Parent { get; set; } = null;

			public Queue<ModelBoneAnimation> Animations { get; }
			private ModelBoneAnimation CurrentAnim { get; set; } = null;

			public bool IsAnimating => CurrentAnim != null || Animations.Count > 0;
			//internal EntityModelBone Definition { get; }

			//public int StartIndex { get; }
			//public int ElementCount { get; }

			public Vector3? Pivot
			{
				get => _pivot;
				set
				{
					_pivot = value;
					
					UpdateBindingMatrix();
				}
			}

			private List<ModelMesh> _modelMeshes = new List<ModelMesh>();
			public ModelBone()
			{
				//Definition = bone;
				Animations = new Queue<ModelBoneAnimation>();

				//StartIndex = startIndex;
				//ElementCount = elementCount;
			}

			public void AddMesh(ModelMesh mesh)
			{
				_modelMeshes.Add(mesh);
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

			//private Matrix WorldMatrix { get; set; } = Matrix.Identity;

			private Vector3 _startRotation = Vector3.Zero;
			private Vector3 _targetRotation = Vector3.Zero;
			public Vector3 TargetRotation => _targetRotation;

			//private Vector3 _tempStartRotation = Vector3.Zero;
			private Vector3 _tempTargetRotation = Vector3.Zero;

			private Vector3 _startPosition = Vector3.Zero;
			private Vector3 _targetPosition = Vector3.Zero;

			//private Vector3 _tempStartPosition = Vector3.Zero;
			private Vector3 _tempTargetPosition = Vector3.Zero;

			private Vector3 _startScale = Vector3.One;
			private Vector3 _targetScale = Vector3.One;

			//private Vector3 _tempStartScale = Vector3.Zero;
			private Vector3 _tempTargetScale = Vector3.One;

			private double _accumulator = 1d;
			private double _target = 1d;

			private double _tempTarget = 1d;

			public void ApplyMovement()
			{
				_startPosition = _position;
				_targetPosition = FixInvalidVector(_tempTargetPosition);
				_tempTargetPosition = Vector3.Zero;

				_startRotation = _rotation;
				_targetRotation = FixInvalidVector(_tempTargetRotation);
				_tempTargetRotation = Vector3.Zero;

				_startScale = _scale;
				_targetScale = FixInvalidVector(_tempTargetScale);
				_tempTargetScale = Vector3.One;

				_accumulator = 0;
				_target = _tempTarget;

				//Console.WriteLine($"{Definition.Name}.Rotation = {_targetRotation}");
			}

			private Vector3 FixInvalidVector(Vector3 vector)
			{
				vector.X = float.IsNaN(vector.X) ? 0f : vector.X;
				vector.Y = float.IsNaN(vector.Y) ? 0f : vector.Y;
				vector.Z = float.IsNaN(vector.Z) ? 0f : vector.Z;

				return vector;
			}

			public void MoveOverTime(Vector3 targetPosition,
				Vector3 targetRotation,
				Vector3 targetScale,
				TimeSpan time,
				bool overrideOthers = false,
				float blendWeight = 1f)
			{
				if (overrideOthers)
				{
					//_startPosition = _position;
					_tempTargetPosition = targetPosition; // - _tempTargetPosition;

					//_startRotation = _rotation;
					_tempTargetRotation = targetRotation; // - _targetRotation;

					//_startScale = _scale;
					_tempTargetScale = targetScale; // - _targetScale;
				}
				else
				{
					//_startPosition = _position;
					_tempTargetPosition += FixInvalidVector(targetPosition); // - _tempTargetPosition;

					//_startRotation = _rotation;
					_tempTargetRotation += FixInvalidVector(targetRotation); // - _targetRotation;

					//_startScale = _scale;
					_tempTargetScale += FixInvalidVector(targetScale); // - _targetScale;
				}

				_tempTarget = time.TotalSeconds;
			}

			public void MoveOverTime(Vector3 targetPosition, Vector3 targetRotation, TimeSpan time)
			{
				MoveOverTime(targetPosition, targetRotation, _targetScale, time);
			}

			public void MoveOverTime(Vector3 targetPosition, TimeSpan time)
			{
				MoveOverTime(targetPosition, _targetRotation, _targetScale, time);
			}

			public void Update(IUpdateArgs args, Vector3 parentScale)
			{
				if (_accumulator < _target)
				{
					_accumulator += args.GameTime.ElapsedGameTime.TotalSeconds;
					float progress = (float) ((1f / _target) * _accumulator);

					var targetRotation = _targetRotation;
					var startRotation = _startRotation;

					var targetPosition = _targetPosition;
					var startPosition = _startPosition;

					var targetScale = _targetScale;
					var startScale = _startScale;

					//var rotationDifference = targetRotation - startRotation;
					_rotation = startRotation + ((targetRotation - startRotation) * progress);
					_position = startPosition + ((targetPosition - startPosition) * progress);
					_scale = startScale + ((targetScale - startScale) * progress);
					
					UpdateTransform();
				}
				//if (!Monitor.TryEnter(_disposeLock, 0))
				//	return;

				try
				{
					//var device = args.GraphicsDevice;

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

					//var scale = parentScale * _scale; // (parentScale / _scale) * (_scale * parentScale);
				
					Children.ForEach(
						child =>
						{
							child.Update(args, parentScale * _scale);
						}, false, false);
				}
				finally
				{
					//	Monitor.Exit(_disposeLock);
				}
			}

			public int Render(IRenderArgs args, Microsoft.Xna.Framework.Graphics.Effect effect, Matrix worldMatrix)
			{
				var renderCount = 0;
				//var count = ElementCount;

			//	worldMatrix = WorldMatrix * worldMatrix;
				Matrix childMatrix =  Transform * worldMatrix;
				if (Rendered)
				{
					var meshes = _modelMeshes;

					if (meshes.Count > 0)
					{
						
						((IEffectMatrices) effect).World = BindingMatrix * childMatrix;

						foreach (var mesh in meshes)
						{
							if (mesh == null || mesh.ElementCount == 0)
								continue;

							foreach (var pass in effect.CurrentTechnique.Passes)
							{
								pass?.Apply();

								args.GraphicsDevice.DrawIndexedPrimitives(
									PrimitiveType.TriangleList, 0, mesh.StartIndex, mesh.ElementCount);

								renderCount++;
							}
						}
					}
				}

				Children.ForEach(
					(child) =>
					{
						renderCount += child.Render(args, effect, childMatrix);
					}, false, false);
				
				return renderCount;
			}
			
			private Vector3 _position;
			private Vector3? _pivot;

			public void AddChild(IAttached modelBone)
			{
				if (modelBone.Parent == null && Children.TryAdd(modelBone))
				{
					modelBone.Parent = this;
					//Children.Add(modelBone);
				}
				else
				{
					Log.Warn($"Could not add {modelBone.Name} as child of {Name}");
				}
			}

			public void Remove(IAttached modelBone)
			{
				if (Children.Remove(modelBone))
				{
					if (modelBone.Parent == this)
						modelBone.Parent = null;

					//Children.Remove(modelBone);
				}
			}

			public string Name { get; set; }
		}
	}
}
