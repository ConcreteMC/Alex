using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.Api;
using Alex.API;
using Alex.API.Graphics;
using Alex.API.Utils;
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
				set => _position = value;
			}

			private Vector3 _rotation = Vector3.Zero;

			public Vector3 Rotation
			{
				get { return _rotation; }
				set { _rotation = value; }
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
			
			public  MCMatrix WorldMatrix   { get; set; } = MCMatrix.Identity;
			public Vector3 TargetRotation => _targetRotation;
			public Vector3 TargetPosition => _targetPosition;
			
			private Vector3 _startRotation = Vector3.Zero;
			private Vector3 _targetRotation = Vector3.Zero;
			
			private Vector3 _startPosition = Vector3.Zero;
			private Vector3 _targetPosition = Vector3.Zero;
			
			private double _accumulator = 1d;
			private double _target = 1d;
			public void MoveOverTime(Vector3 targetPosition, Vector3 targetRotation, TimeSpan time)
			{
				_startPosition = _position;
				_targetPosition = targetPosition;
				
				_startRotation = _rotation;
				_targetRotation = targetRotation;
				
				_accumulator = _target = time.TotalSeconds;
				_accumulator = 0d;
			}
			
			public void Update(IUpdateArgs args,
				MCMatrix characterMatrix)
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
					
					//var rotationDifference = targetRotation - startRotation;
					_rotation = startRotation + ((targetRotation - startRotation) * progress);
					_position = startPosition + ((targetPosition - startPosition) * progress);
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

					MCMatrix matrix;
					
					if (Definition.Pivot.HasValue)
					{
						var pivot = (Definition.Pivot ?? Vector3.Zero);
						matrix = MCMatrix.CreateTranslation(-pivot) 
						         * MCMatrix.CreateRotationDegrees(_rotation)
						         * MCMatrix.CreateTranslation(pivot)
						         * MCMatrix.CreateTranslation(_position)
						         * characterMatrix;
					}
					else
					{
						matrix = MCMatrix.CreateRotationDegrees(_rotation)
						         * MCMatrix.CreateTranslation(_position)
						         * characterMatrix;
					}

					var children = Children.ToArray();

					if (children.Length > 0)
					{
						foreach (var child in children)
						{
							child.Update(args, matrix);
						}
					}

					if (Definition.Pivot.HasValue)
					{
						var pivot = (Definition.Pivot ?? Vector3.Zero);
						WorldMatrix = MCMatrix.CreateTranslation(-pivot) 
						              * MCMatrix.CreateRotationDegrees(BindingRotation)
						              * MCMatrix.CreateTranslation(pivot)
						              * matrix;
					}
					else
					{
						WorldMatrix = MCMatrix.CreateRotationDegrees(BindingRotation)
						              * matrix;
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
