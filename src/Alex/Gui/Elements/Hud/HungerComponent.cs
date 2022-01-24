using System;
using Alex.Common.Gui.Graphics;
using Alex.Entities;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gui.Elements.Hud
{
	public class HungerComponent : StackContainer
	{
		private Player Player { get; }
		private HungerTexture[] Hungers { get; }

		public HungerComponent(Player player)
		{
			// Hunger = player.Hunger;
			Player = player;

			ChildAnchor = Alignment.BottomLeft;
			Orientation = Orientation.Horizontal;

			Height = 10;
			//Width = 10 * 8;
			Hungers = new HungerTexture[10];

			for (int i = 0; i < 10; i++)
			{
				AddChild(
					Hungers[i] = new HungerTexture()
					{
						// Margin = new Thickness(0, 0, (i * 8), 0),
						Anchor = Alignment.BottomRight
					});
			}

			player.HealthManager.OnHungerChanged += (sender, e) => { Update(e.Hunger, e.MaxHunger); };
			//Update(player.HealthManager.Hunger, player.HealthManager.MaxHunger);
		}

		private void Update(int hunger, int maxHunger)
		{
			var hearts = hunger * (10d / maxHunger);
			bool isRounded = (hearts % 1 == 0);

			var ceil = isRounded ? (int)hearts : (int)Math.Ceiling(hearts);

			for (int i = 0; i < Hungers.Length; i++)
			{
				HeartValue value = HeartValue.Full;

				if (i < hearts)
				{
					value = HeartValue.Full;
				}
				else if (i == ceil)
				{
					value = HeartValue.Half;
				}
				else
				{
					value = HeartValue.None;
				}

				Hungers[^(i + 1)].Set(value);
			}
		}

		public class HungerTexture : RocketControl
		{
			private TextureElement Texture { get; set; }

			//private 
			public HungerTexture()
			{
				Width = 9;
				Height = 9;

				AddChild(
					Texture = new TextureElement()
					{
						Anchor = Alignment.TopRight, Height = 9, Width = 9,
						//Margin = new Thickness(4, 4)
					});
			}

			protected override void OnInit(IGuiRenderer renderer)
			{
				Background = renderer.GetTexture(AlexGuiTextures.HungerPlaceholder);
				Texture.Texture = renderer.GetTexture(AlexGuiTextures.HungerFull);
				Set(_value);
			}

			private HeartValue _value;

			public void Set(HeartValue value)
			{
				_value = value;
				Texture.IsVisible = true;

				switch (value)
				{
					case HeartValue.Full:
						if (GuiRenderer != null)
							Texture.Texture = GuiRenderer.GetTexture(AlexGuiTextures.HungerFull);

						break;

					case HeartValue.Half:
						if (GuiRenderer != null)
							Texture.Texture = GuiRenderer.GetTexture(AlexGuiTextures.HungerHalf);

						break;

					case HeartValue.None:
						Texture.IsVisible = false;

						break;
				}
			}
		}
	}
}