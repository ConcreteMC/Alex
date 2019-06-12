using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Graphics.Models.Entity
{
	public partial class EntityModelRenderer : Model, IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(EntityModelRenderer));

		private EntityModel Model { get; }
		private IReadOnlyDictionary<string, ModelBone> Bones { get; }
		public Texture2D Texture { get; set; }
		public EntityModelRenderer(EntityModel model, Texture2D texture)
		{
			Model = model;
			Texture = texture;

			if (texture == null)
			{
				Log.Warn($"No texture set for rendererer for {model.Name}!");
				return;
			}

			var cubes = new Dictionary<string, ModelBone>();
			Cache(cubes);

			Bones = cubes;
		}

		private void Cache(Dictionary<string, ModelBone> modelBones)
		{
			List<EntityModelBone> headBones = new List<EntityModelBone>();

			var headBone =
				Model.Bones.FirstOrDefault(x => x.Name.Contains("head", StringComparison.InvariantCultureIgnoreCase));
			if (headBone != null)
			{
				headBones.Add(headBone);
				foreach (var bone in Model.Bones)
				{
					if (bone == headBone) continue;
					if (bone.Parent.Equals(headBone.Name))
					{
						headBones.Add(bone);
					}
				}
			}

			foreach (var bone in Model.Bones)
			{
				if (bone == null) continue;
				if (bone.NeverRender) continue;
				bool partOfHead = headBones.Contains(bone);

				if (bone.Cubes != null)
				{
					List<ModelBoneCube> c = new List<ModelBoneCube>();
					ModelBone modelBone;
					foreach (var cube in bone.Cubes)
					{
						if (cube == null)
						{
							Log.Warn("Cube was null!");
							continue;
						}

						var size = cube.Size;
						var origin = cube.Origin;
						var pivot = bone.Pivot;
						var rotation = bone.Rotation;

						VertexPositionNormalTexture[] vertices;
						Cube built = new Cube(size, new Vector2(Texture.Width, Texture.Height));
						built.Mirrored = bone.Mirror;
						built.BuildCube(cube.Uv);

						vertices = built.Front.Concat(built.Back).Concat(built.Top).Concat(built.Bottom).Concat(built.Left)
							.Concat(built.Right).ToArray();

						var part = new ModelBoneCube(vertices, Texture, rotation, pivot, origin);

						part.Mirror = bone.Mirror;
						if (partOfHead)
						{
							part.ApplyHeadYaw = true;
							part.ApplyYaw = false;
						}
						else
						{
							part.ApplyPitch = false;
							part.ApplyYaw = true;
							part.ApplyHeadYaw = false;
						}

						c.Add(part);
					}

					modelBone = new ModelBone(c.ToArray());
					if (!modelBones.TryAdd(bone.Name, modelBone))
					{
						Log.Warn($"Failed to add bone! {Model.Name}:{bone.Name}");
					}
				}
			}
		}

		public void Render(IRenderArgs args, PlayerLocation position)
		{
			foreach (var bone in Bones)
			{
				bone.Value.Render(args, position);
			}
		}

		public Vector3 DiffuseColor { get; set; } = Color.White.ToVector3();

		public void Update(IUpdateArgs args, PlayerLocation position)
		{
			foreach (var bone in Bones)
			{
				bone.Value.Update(args, position, DiffuseColor);
			}
		}

		public bool GetBone(string name, out ModelBone bone)
		{
			return Bones.TryGetValue(name, out bone);
		}

		public override string ToString()
		{
			return Model.Name;
		}

		public void Dispose()
		{
			if (Bones != null && Bones.Any())
			{
				foreach (var bone in Bones.ToArray())
				{
					bone.Value.Dispose();
				}
			}
			
			Texture?.Dispose();
		}
	}
}
