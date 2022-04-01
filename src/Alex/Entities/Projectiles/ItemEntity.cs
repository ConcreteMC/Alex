using Alex.Common.Graphics;
using Alex.Items;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Entities.Projectiles
{
	public class ItemEntity : ItemBaseEntity
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

		public ItemEntity(World level) : base(level)
		{
			Width = 0.25;
			Height = 0.25;
			//  Length = 0.25;

			Gravity = 0.04;
			Drag = 0.02;
		}

		private float _rotation = 0;
		protected bool DoRotation { get; set; } = true;

		private bool IsBlock { get; set; } = false;
		public override void Update(IUpdateArgs args)
		{
			if (CanRender)
			{
				if (DoRotation)
				{
					ItemRenderer.Update(args);
				}
				else
				{
					ItemRenderer.Update(args);
				}
			}

			if (DoRotation)
			{
				_rotation += 45f * Alex.DeltaTime;
			}
		}


		/// <inheritdoc />
		public override void SetItem(Item item)
		{
			base.SetItem(item);

			if (item is ItemBlock)
				IsBlock = true;
		}

		protected virtual float GetScale()
		{
			return 1f / 16f;
		}

		public override int Render(IRenderArgs renderArgs, bool useCulling)
		{
			if (!CanRender)
				return 0;

			var itemRenderer = ItemRenderer;

			if (itemRenderer == null)
				return 0;

			var knownPos = KnownPosition.ToVector3();
			float scale = GetScale();
			Matrix worldMatrix;
			var offset = new Vector3((float)Width / 2f, 0f, (float)Width / 2f);

			if (DoRotation)
			{
				worldMatrix = Matrix.CreateScale(scale) * Matrix.CreateTranslation(-offset)
				                                        * Matrix.CreateRotationY(MathHelper.ToRadians(_rotation))
				                                        * Matrix.CreateTranslation(offset)
				                                        * Matrix.CreateTranslation(knownPos);
			}
			else
			{
				worldMatrix = Matrix.CreateScale(scale) * Matrix.CreateTranslation(-offset)
				                                        * Matrix.CreateRotationY(
					                                        MathHelper.ToRadians(KnownPosition.Yaw))
				                                        * Matrix.CreateTranslation(offset)
				                                        * Matrix.CreateTranslation(knownPos);
			}

			return itemRenderer.Render(renderArgs, worldMatrix);
		}
	}
}