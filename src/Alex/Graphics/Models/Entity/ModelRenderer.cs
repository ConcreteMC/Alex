using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Alex.Common.Graphics;
using Alex.Common.Utils;
using Alex.Entities;
using Alex.Graphics.Effect;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Graphics.Models.Entity
{
	public class ModelRenderer : ModelMatrixHolder, IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ModelRenderer));
		private Vector3 _entityColor = Color.White.ToVector3();
		private Vector3 _diffuseColor = Color.White.ToVector3();
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
				
				if (value != null && Effect != null)
				{
					Effect.Texture = value;
				}
				
				UpdateScale();
			}
		}

		public ModelRenderer(IModel model, EntityEffect effect)
		{
			Model = model;
			Effect = effect;
		}

		private void UpdateScale()
		{
			Effect.TextureScale = Vector2.One / TextureSize;
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

			var matrices = GetTransforms();

			if (matrices == null)
				return 0;
			
			return modelInstance.Draw(worldMatrix, args.Camera.ViewMatrix, args.Camera.ProjectionMatrix, matrices, Effect);
		}

		public bool GetBone(string name, out ModelBone bone)
		{
			if (Model.Bones.TryGetValue(name, out bone))
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

		protected void Dispose(bool disposing)
		{
			var model = Model;
			
			if (model != null)
			{
				model?.Dispose();
				Model = null;
			}
			
			Effect?.Dispose();
			Effect = null;

			var texture = _texture;
			if (texture != null)
			{
				_texture = null;
				if (texture.Tag is Guid guid)
				{
					if (guid == EntityFactory.PooledTagIdentifier)
						return;
				}
			//	texture?.Dispose();
			}
		}

		public override void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void ApplyPending()
		{
			var modelInstance = Model;

			if (modelInstance == null)
				return;

			ApplyMovement();
		}

		~ModelRenderer()
		{
			Dispose(false);
		}
	}
}
