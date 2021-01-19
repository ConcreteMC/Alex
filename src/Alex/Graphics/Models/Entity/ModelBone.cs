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
			internal List<IAttached> Children { get; set; } = new List<IAttached>();
			
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

			public void Update(IUpdateArgs args,
				MCMatrix characterMatrix)
			{
				if (_disposed) return;

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
					

					if (Definition.Pivot.HasValue)
					{
						var pivot = (Definition.Pivot ?? Vector3.Zero);
						WorldMatrix = MCMatrix.CreateTranslation(-pivot) 
						              * MCMatrix.CreateRotationDegrees(BindingRotation + Rotation)
						              * MCMatrix.CreateTranslation(pivot)
						              * characterMatrix;
					}
					else
					{
						WorldMatrix = MCMatrix.CreateRotationDegrees(BindingRotation + Rotation)
						              * characterMatrix;
					}

					var children = Children.ToArray();

					if (children.Length > 0)
					{
						foreach (var child in children)
						{
							child.Update(args, WorldMatrix);
						}
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
			public void Dispose()
			{
				_disposed = true;
			}

			public void AddChild(IAttached modelBone)
			{
				if (!Children.Contains(modelBone) && modelBone.Parent == null)
				{
					modelBone.Parent = this;
					Children.Add(modelBone);
				}
				else
				{
					Log.Warn($"Could not add {modelBone.Name} as child of {Definition.Name}");
				}
			}

			public void Remove(IAttached modelBone)
			{
				if (Children.Contains(modelBone))
				{
					if (modelBone.Parent == this)
						modelBone.Parent = null;
					
					Children.Remove(modelBone);
				}
			}

			public string        Name   => Definition.Name;
		}
	}
}
