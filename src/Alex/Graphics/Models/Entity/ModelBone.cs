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
		public class ModelBone : IDisposable
		{
			internal List<ModelBone> Children { get; set; } = new List<ModelBone>();
			
			private Vector3 _rotation = Vector3.Zero;

			public Vector3 Rotation
			{
				get { return _rotation; }
				set { _rotation = value; }
			}

			public bool Rendered { get; set; } = true;

			public ModelBone Parent { get; set; } = null;
			
			public Queue<ModelBoneAnimation> Animations { get; }
			private ModelBoneAnimation CurrentAnim { get; set; } = null;
			public bool IsAnimating => CurrentAnim != null || Animations.Count > 0;
			internal EntityModelBone Definition { get; }
			
			public int StartIndex   { get; }
			public int ElementCount { get; }
			public ModelBone(EntityModelBone bone, int startIndex, int elementCount)
			{
				Definition = bone;
				//Indices = indices;
				Animations = new Queue<ModelBoneAnimation>();

				StartIndex = startIndex;
				ElementCount = elementCount;
			}

			//private bool _isDirty = true;

			//private object _disposeLock = new object();
			/*public void Render(IRenderArgs args, bool mock, out int vertices)
			{
				vertices = 0;
				
				if (_disposed) return;
				if (!Monitor.TryEnter(_disposeLock, 0))
					return;

				try
				{
					if (!Definition.NeverRender && Rendered)
					{
						var effect = Effect;

						if (!(buffer == null || effect == null || effect.Texture == null || effect.IsDisposed
						      || buffer.MarkedForDisposal))
						{
							if (buffer.IndexCount == 0)
							{
								Log.Warn($"Bone indexcount = 0 || {Definition.Name}");
							}

							args.GraphicsDevice.Indices = buffer;

							effect.View = args.Camera.ViewMatrix;
							effect.Projection = args.Camera.ProjectionMatrix;

							if (!mock && buffer.IndexCount > 0)
							{
								if (effect.CurrentTechnique != null && !Definition.NeverRender)
								{
									//	foreach (var technique in effect.Techniques)
									{
										foreach (var pass in effect.CurrentTechnique.Passes)
										{
											pass?.Apply();

											args.GraphicsDevice.DrawIndexedPrimitives(
												PrimitiveType.TriangleList, 0, 0, buffer.IndexCount / 3);
										}
									}
								}

								//else
								{
									//	Log.Warn($"Current");
								}

								vertices += buffer.IndexCount / 3;
							}
							else if (!mock && buffer.IndexCount == 0)
							{
								Log.Warn($"Index count = 0");
							}
						}
					}

					var children = Children.ToArray();

					if (children.Length > 0)
					{
						foreach (var child in children)
						{
							child.Render(args, mock, out int childVertices);
							vertices += childVertices;
						}
					}
				}
				catch (Exception ex)
				{
					Log.Warn(ex, $"An error occured rendering bone {Name}");
				}
				finally
				{
					Monitor.Exit(_disposeLock);
				}
			}*/

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
					
					var pivot        = (Definition.Pivot ?? Vector3.Zero);
					WorldMatrix = MCMatrix.CreateTranslation(-pivot)
					              * MCMatrix.CreateRotationDegrees(Rotation)
					              * MCMatrix.CreateTranslation(pivot) * characterMatrix;

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

			private bool _disposed = false;
			public void Dispose()
			{
				_disposed = true;
			}

			public void AddChild(ModelBone modelBone)
			{
				if (!Children.Contains(modelBone))
				{
					Children.Add(modelBone);
				}
				else
				{
					Log.Warn($"Could not add {modelBone.Name} as child of {Definition.Name}");
				}
			}

			public void Remove(ModelBone modelBone)
			{
				if (Children.Contains(modelBone))
				{
					Children.Remove(modelBone);
				}
			}

			public string Name => Definition.Name;
		}
	}
}
