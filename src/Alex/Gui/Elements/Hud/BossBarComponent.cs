using System;
using System.Collections.Concurrent;
using Alex.Common.Gui.Graphics;
using Alex.Networking.Java.Packets.Play;
using Microsoft.Xna.Framework;
using RocketUI;
using UUID = MiNET.Utils.UUID;

namespace Alex.Gui.Elements.Hud
{
	public class BossBarContainer : StackContainer
	{
		private ConcurrentDictionary<MiNET.Utils.UUID, BossBar> _bossBars;
		public BossBarContainer()
		{
			_bossBars = new ConcurrentDictionary<UUID, BossBar>();
			
			base.Orientation = Orientation.Vertical;
			Anchor = Alignment.TopCenter;
			
			//AddChild(new BossBar());
		}

		public bool Add(UUID uuid,
			string title,
			float health,
			BossBarPacket.BossBarColor color,
			BossBarPacket.BossBarDivisions divisions,
			byte flags,
			Color? customColor = null)
		{
			var bossbar = new BossBar()
			{
				Color = color,
				Text = title,
				Health = health,
				Divisions = divisions,
				Flags = flags
			};

			if (customColor != null)
			{
				bossbar.CustomColor = customColor;
			}
			
			if (_bossBars.TryAdd(
				uuid, bossbar))
			{
				AddChild(bossbar);
				return true;
			}

			return false;
		}

		public void UpdateTitle(UUID uuid, string title)
		{
			if (_bossBars.TryGetValue(uuid, out var bar))
			{
				bar.Text = title;
			}
		}
		
		public void UpdateStyle(UUID uuid, BossBarPacket.BossBarColor color, BossBarPacket.BossBarDivisions divisions)
		{
			if (_bossBars.TryGetValue(uuid, out var bar))
			{
				bar.Color = color;
				bar.Divisions = divisions;
			}
		}
		
		public void UpdateHealth(UUID uuid, float health)
		{
			if (_bossBars.TryGetValue(uuid, out var bar))
			{
				bar.Health = health;
			}
		}

		public bool Remove(UUID uuid)
		{
			if (_bossBars.TryRemove(uuid, out var bar))
			{
				RemoveChild(bar);

				return true;
			}

			return false;
		}

		public void Reset()
		{
			foreach (var bar in _bossBars)
			{
				RemoveChild(bar.Value);
			}
			
			_bossBars.Clear();
		}
	}

	public class BossBar : Container
	{
		private BossBarPacket.BossBarColor _color;

		private TextureElement _textureElement;
		private TextElement _textElement;
		private float _health;

		private const int MaxWidth = 182;
		public BossBar()
		{
			_textElement = new TextElement(){
				Anchor = Alignment.TopCenter,
				FontStyle = FontStyle.DropShadow
			};
			_textureElement = new TextureElement()
			{
				Anchor = Alignment.BottomCenter,
				Height = 5,
				MinHeight = 5,
				Width = MaxWidth,
				MinWidth = MaxWidth,
				MaxWidth = MaxWidth
			};

			//_textureElement.Width = _textureElement.MinWidth = 182;
			
			AddChild(_textElement);
			AddChild(_textureElement);

			Width = MaxWidth;
			Height = 18;
			Color = BossBarPacket.BossBarColor.Pink;
			base.MaxWidth = MaxWidth;
		}

		private bool _divisionDirty = false;
		public BossBarPacket.BossBarDivisions Divisions
		{
			get => _divisions;
			set
			{
				_divisions = value;
				_divisionDirty = true;
			}
		}

		public byte Flags { get; set; }
		
		public string Text
		{
			get
			{
				return _textElement.Text;
			}
			set
			{
				_textElement.Text = value;
			}
		}

		public float Health
		{
			get => _health;
			set
			{
				_health = value;
				_colorDirty = true;
				//_textureElement.Width = Math.Min((int) (Math.Ceiling((1f / MaxWidth) * value)), MaxWidth);
			}
		}

		private bool _colorDirty = true;
		private GuiTextures _guiTexture;
		private BossBarPacket.BossBarDivisions _divisions;
		private Color? _customColor = null;

		public Color? CustomColor
		{
			get => _customColor;
			set
			{
				_customColor = value;

				Color = BossBarPacket.BossBarColor.White;
				if (value != null)
				{
					_textureElement.Background.Mask = value.Value;
					_textureElement.BackgroundOverlay.Mask = value.Value;
					//_textureElement.Background.Mask = ColorHelper.HexToColor("#ec00b8");
					//_textureElement.BackgroundOverlay.Mask = ColorHelper.HexToColor("#490039");
				}
			}
		}

		public BossBarPacket.BossBarColor Color
		{
			get => _color;
			set
			{
				_color = value;
				_colorDirty = true;
				
				switch (_color)
				{
					case BossBarPacket.BossBarColor.Pink:
						_textureElement.Background = AlexGuiTextures.BossbarBackgroundPink;
						_textureElement.BackgroundOverlay = AlexGuiTextures.BossbarProgressPink;
						break;

					case BossBarPacket.BossBarColor.Blue:
						_textureElement.Background = AlexGuiTextures.BossbarBackgroundBlue;
						_textureElement.BackgroundOverlay = AlexGuiTextures.BossbarProgressBlue;
						break;

					case BossBarPacket.BossBarColor.Red:
						_textureElement.Background = AlexGuiTextures.BossbarBackgroundRed;
						_textureElement.BackgroundOverlay = AlexGuiTextures.BossbarProgressRed;
						break;

					case BossBarPacket.BossBarColor.Green:
						_textureElement.Background = AlexGuiTextures.BossbarBackgroundGreen;
						_textureElement.BackgroundOverlay = AlexGuiTextures.BossbarProgressGreen;
						break;

					case BossBarPacket.BossBarColor.Yellow:
						_textureElement.Background = AlexGuiTextures.BossbarBackgroundYellow;
						_textureElement.BackgroundOverlay = AlexGuiTextures.BossbarProgressYellow;
						break;

					case BossBarPacket.BossBarColor.Purple:
						_textureElement.Background = AlexGuiTextures.BossbarBackgroundPurple;
						_textureElement.BackgroundOverlay = AlexGuiTextures.BossbarProgressPurple;
						break;

					case BossBarPacket.BossBarColor.White:
						_textureElement.Background = AlexGuiTextures.BossbarBackgroundWhite;
						_textureElement.BackgroundOverlay = AlexGuiTextures.BossbarProgressWhite;
						break;
				}

				UpdateDirty(true);
			}
		}

		private void UpdateDirty(bool force = false)
		{
			_textureElement.Background.Scale = new Vector2(182f / _textureElement.Background.Width, 5f / _textureElement.Background.Height);
			
			if (_colorDirty || force)
			{
				_colorDirty = false;
				_textureElement.BackgroundOverlay.Scale = new Vector2(_health, 1f);
				_textureElement.BackgroundOverlay.Scale = new Vector2(182f / _textureElement.BackgroundOverlay.Width, 5f / _textureElement.BackgroundOverlay.Height);
			}
			
			if ((_divisionDirty || force) && GuiRenderer != null)
			{
				_divisionDirty = false;
			
				switch (_divisions)
				{
					case BossBarPacket.BossBarDivisions.Six:
						_textureElement.Texture = GuiRenderer.GetTexture(AlexGuiTextures.BossbarDivider6);
						break;
					case BossBarPacket.BossBarDivisions.Ten:
						_textureElement.Texture = GuiRenderer.GetTexture(AlexGuiTextures.BossbarDivider10);
						break;
					case BossBarPacket.BossBarDivisions.Twelve:
						_textureElement.Texture = GuiRenderer.GetTexture(AlexGuiTextures.BossbarDivider12);
						break;
					case BossBarPacket.BossBarDivisions.Twenty:
						_textureElement.Texture = GuiRenderer.GetTexture(AlexGuiTextures.BossbarDivider20);
						break;
					default:
						_textureElement.Texture = null;
						break;
				}
				//_textureElement.Texture = GuiRenderer.GetTexture(_guiTexture);
			}
		}
		
		/// <inheritdoc />
		protected override void OnInit(IGuiRenderer renderer)
		{
			base.OnInit(renderer);
			
		}

		/// <inheritdoc />
		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);

			UpdateDirty();
		}
	}
}