using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Common.Graphics;
using Alex.Common.Utils;
using Alex.Graphics.Effect;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Graphics.Models.Entity
{
	public class TextureBinding
	{
		public TextureBinding(Texture2D texture, Vector2 size)
		{
			Texture = texture;
			Size = size;
			Effect = new EntityEffect();
			Effect.Texture = texture;
			Effect.TextureScale = Vector2.One / size;
		}

		public Vector2 Size { get; }
		public Texture2D Texture { get; }
		public EntityEffect Effect { get; }
	}
	public class ModelRenderer : IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ModelRenderer));
		private Vector3 _entityColor = Color.White.ToVector3();
		private Vector3 _diffuseColor = Color.White.ToVector3();
		internal Model Model { get; set; }

		public double VisibleBoundsWidth { get; set; } = 0;
		public double VisibleBoundsHeight { get; set; } = 0;

		private Vector2? _textureSize = null;
		public Vector2 TextureSize
		{
			get
			{
				var backupSize = new Vector2(64, 32);// Vector2.One;
				return _textureSize.GetValueOrDefault(backupSize);
			}
			set
			{
				if (value.LengthSquared() < 1f)
					return;
				_textureSize = value;

				if (Effect != null)
				{
					UpdateScale();
				}
			}
		}

		public Vector3 EntityColor
		{
			get => _entityColor;
			set
			{
				_entityColor = value;
				var effect = Effect;

				if (effect != null)
					effect.DiffuseColor = _diffuseColor * _entityColor;
			}
		}

		public Vector3 DiffuseColor
		{
			get => _diffuseColor;
			set
			{
				_diffuseColor = value;
				var effect = Effect;

				if (effect != null)
					effect.DiffuseColor = _diffuseColor * _entityColor;
			}
		}

		private EntityEffect Effect { get; set; }
		private Texture2D _texture;
		public Texture2D Texture
		{
			get
			{
				return  _texture;
			}
			set
			{
				_texture = value;

				if (value != null && !_textureSize.HasValue)
				{
					_textureSize = new Vector2(value.Width, value.Height);
				}

				var effect = Effect;
				if (value != null && effect != null)
				{
					effect.Texture = value;
				}
				
				UpdateScale();
			}
		}

		public ModelRenderer(Model model, EntityEffect effect)
		{
			Model = model;
			Effect = effect;
		}
		
		private void UpdateScale()
		{
			var effect = Effect;

			if (effect == null)
				return;
			
			effect.TextureScale = Vector2.One / TextureSize;
		}

		///  <summary>
		/// 		Renders the entity model
		///  </summary>
		///  <param name="args"></param>
		///  <param name="worldMatrix">The world matrix</param>
		///  <returns>The amount of GraphicsDevice.Draw calls made</returns>
		public int Render(IRenderArgs args, Matrix worldMatrix)
		{
			var modelInstance = Model;

			if (modelInstance == null)
				return 0;
			
			return modelInstance.Draw(worldMatrix, args.Camera.ViewMatrix, args.Camera.ProjectionMatrix);
		}

		public virtual void Update(IUpdateArgs args)
		{
			var model = Model;
			if (model?.Bones == null) return;
		
			foreach (var bone in model.Bones.ImmutableArray)
			{
				bone.Update(args);
			}
		}
		
		public bool GetBone(string name, out ModelBone bone)
		{
			var model = Model;

			if (model?.Bones == null)
			{
				bone = null;
				return false;
			}
			
			if (model.Bones.TryGetValue(name, out bone))
			{
				return true;
			}

			return false;
		}
		
		public void SetVisibility(string bone, bool visible)
		{
			if (GetBone(bone, out var boneValue))
			{
				boneValue.Visible = visible;
			}
		}

		public void Dispose()
		{
			Model?.Dispose();
			Model = null;

			Effect?.Dispose();
			Effect = null;
			
			Texture?.Dispose();
			Texture = null;
			//_texture?.Dispose();
			//_texture = null;
		}

		public void ApplyPending()
		{
			var modelInstance = Model;

			if (modelInstance == null)
				return;
			
			foreach (var b in modelInstance.Bones)
			{
				b?.ApplyMovement();
			}
		}
	}
}
