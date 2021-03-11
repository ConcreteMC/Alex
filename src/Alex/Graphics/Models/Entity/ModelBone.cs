using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.Api;
using Alex.API;
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
		public class ModelBone : IAttached, IDisposable
		{
			internal ThreadSafeList<IAttached> Children { get; set; } = new ThreadSafeList<IAttached>();

			public Vector3 Position
			{
				get => _position;
				set => _targetPosition = _position = value;
			}

			private Vector3 _rotation = Vector3.Zero;

			public Vector3 Rotation
			{
				get { return _rotation; }
				set { _targetRotation = _rotation = value; }
			}
			
			private Vector3 _scale = Vector3.One;

			public Vector3 Scale
			{
				get { return _scale; }
				set { _targetScale = _scale = value; }
			}

			private Vector3 _bindingRotation = Vector3.Zero;

			public Vector3 BindingRotation
			{
				get { return _bindingRotation; }
				set { _bindingRotation = value; }
			}

			public bool Rendered { get; set; } = true;

			public IAttached Parent { get; set; } = null;
			
			public Queue<ModelBoneAnimation> Animations { get; }
			private ModelBoneAnimation CurrentAnim { get; set; } = null;
			public bool IsAnimating => CurrentAnim != null || Animations.Count > 0;
			internal EntityModelBone Definition { get; }
			
			public int StartIndex   { get; }
			public int ElementCount { get; }
			public ModelBone(EntityModelBone bone, int startIndex, int elementCount)
			{
				Definition = bone;
				Animations = new Queue<ModelBoneAnimation>();

				StartIndex = startIndex;
				ElementCount = elementCount;
			}
			
			public void ClearAnimations()
			{
				if (_disposed) return;
				var anim = CurrentAnim;

				if (anim != null)
				{
					anim.Reset();
					CurrentAnim = null;
				}
			}

			private MCMatrix WorldMatrix   { get; set; } = MCMatrix.Identity;

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
			
			public void MoveOverTime(Vector3 targetPosition, Vector3 targetRotation, Vector3 targetScale, TimeSpan time, bool overrideOthers = false, float blendWeight = 1f)
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
			
			public void Update(IUpdateArgs args,
				MCMatrix characterMatrix, Vector3 parentScale)
			{
				if (_disposed) return;

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

					var scale = parentScale * _scale;// (parentScale / _scale) * (_scale * parentScale);
					var rotation = _rotation;
					var translation = _position;
					
					/*if (characterMatrix.Decompose(
						out var parentScale, out var parentRotation, out var parentTranslation))
					{
						if (parentScale.X != 0.0 && parentScale.Y != 0.0 & parentScale.Z != 0.0)
						{
							scale = scale * (16f * parentScale);
						}
					}*/
					
					MCMatrix matrix;
					
					if (Definition.Pivot.HasValue)
					{
						var pivot = (Definition.Pivot ?? Vector3.Zero);
						matrix = MCMatrix.CreateTranslation(-pivot) 
						                                     * MCMatrix.CreateRotationDegrees(rotation)
						                                     * MCMatrix.CreateTranslation(pivot)
						                                     * MCMatrix.CreateTranslation(translation)
							                                     * characterMatrix;
					}
					else
					{
						matrix =MCMatrix.CreateRotationDegrees(rotation)
						                                     * MCMatrix.CreateTranslation(translation)
						                                     * characterMatrix;
					}

					var children = Children.ToArray();

					if (children.Length > 0)
					{
						foreach (var child in children)
						{
							child.Update(args, matrix, parentScale * _scale);
						}
					}

					var bindingRotation = _bindingRotation;
					if (Definition.Pivot.HasValue)
					{
						var pivot = (Definition.Pivot ?? Vector3.Zero);
						WorldMatrix = MCMatrix.CreateTranslation(-pivot) 
						                                          * MCMatrix.CreateRotationDegrees(bindingRotation)
						                                          * MCMatrix.CreateTranslation(pivot)
						                                          * matrix;
					}
					else
					{
						WorldMatrix = MCMatrix.CreateRotationDegrees(bindingRotation)  * matrix;
					}
				}
				finally
				{
				//	Monitor.Exit(_disposeLock);
				}
			}

			public void Render(IRenderArgs args, Microsoft.Xna.Framework.Graphics.Effect effect)
			{
				var count = ElementCount;

				if (!Definition.NeverRender && Rendered && count > 0)
				{
					((IEffectMatrices)effect).World = WorldMatrix;

					foreach (var pass in effect.CurrentTechnique.Passes)
					{
						pass?.Apply();

						args.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, StartIndex, count / 3);
					}
				}
			
				foreach (var child in Children)
				{
					child.Render(args, effect);
				}
			}
			
			private bool _disposed = false;
			private Vector3 _position;

			public void Dispose()
			{
				_disposed = true;
			}

			public void AddChild(IAttached modelBone)
			{
				if (modelBone.Parent == null && Children.TryAdd(modelBone))
				{
					modelBone.Parent = this;
					//Children.Add(modelBone);
				}
				else
				{
					Log.Warn($"Could not add {modelBone.Name} as child of {Definition.Name}");
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

			public string        Name   => Definition.Name;
		}
	}
}
