using Alex.API.Input;
using Alex.API.Utils;
using Alex.Entities;
using Microsoft.Xna.Framework.Input;
using NLog;

namespace Alex.Items
{
	public class ItemBow : Item, ITickableItem
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ItemBow));
		
		private static readonly string[] Textures = new string[]
		{
			"default",
			"bow_pulling_0",
			"bow_pulling_1",
			"bow_pulling_2",
		};
		
		private bool _itemInUse = false;
		public ItemBow()
		{
			base.Material = ItemMaterial.Wood;
			base.ItemType = ItemType.AnyTool;
		}

		private int _pullForce = 0;

		private void SetForce(string value)
		{
			var renderer = Renderer;

			if (renderer == null)
				return;

			//TODO: Change model & texture.
			Log.Info($"Set force: {_pullForce} ({value})");
			//TryUpdateTexture("minecraft:bow", value);
		}
		
		private void ResetDefault()
		{
			_pullForce = 0;
			SetForce(Textures[0]);
		}

		private void IncreasePullForce()
		{
			if (_pullForce < Textures.Length)
			{
				_pullForce++;
				SetForce(Textures[_pullForce]);
			}
		}

		/// <inheritdoc />
		public void Tick(Entity entity)
		{
			/*if (_ticks % 10 == 0)
			{
				IncreasePullForce();
			}
			
			_ticks++;*/
		}

		/// <inheritdoc />
		public bool RequiresTick()
		{
			return _itemInUse;
		}

		private void Released(Player player)
		{
			ResetDefault();
		}

	/*	private int _ticks = 0;
		/// <inheritdoc />
		public override bool UseItem(Player player, MouseButton button, ButtonState state)
		{
			if (state == ButtonState.Pressed)
			{
				_itemInUse = true;
				_ticks = 0;
			}
			else if (state == ButtonState.Released)
			{
				_itemInUse = false;
				Released(player);
			}
			return true;
		}*/
	}
}