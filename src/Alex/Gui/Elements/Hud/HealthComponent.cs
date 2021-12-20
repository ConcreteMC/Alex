using System;
using Alex.Common.Gui.Graphics;
using Alex.Entities;
using Alex.Entities.Meta;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gui.Elements.Hud
{
    public enum HeartValue
    {
        Full,
        Half,
        None
    }
    
    public class HealthComponent : Container
    {
        private Player Player { get; }
        private HealthBarHeart[] Hearts { get; }
        
        public HealthComponent(Player player)
        {
            Player = player;

            Height = 10;

            Hearts = new HealthBarHeart[10];
            for (int i = 0; i < 10; i++)
            {
                AddChild(Hearts[i] = new HealthBarHeart()
                {
                    Margin = new Thickness((i * 8), 0, 0, 0),
                    Anchor = Alignment.BottomLeft
                });
            }
            
            player.HealthManager.OnHealthChanged += (sender, e) =>
            {
                Update(e.Health, e.MaxHealth);
            };
            //Update(player.HealthManager.Health, player.HealthManager.MaxHealth);
        }

        private void Update(float health, float max)
        {
            var hearts = health * (10d / max);
            var ceil = (int)Math.Ceiling(hearts);
            for (int i = 0; i < Hearts.Length; i++)
            {
                HeartValue value = HeartValue.Full;
                if (i <= hearts)
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
                    
                Hearts[i].Set(value);
            }
        }

        public class HealthBarHeart : RocketControl
        {
            private TextureElement Texture { get; set; }

            //private 
            public HealthBarHeart()
            {
                Width = 9;
                Height = 9;
            
                AddChild(Texture = new TextureElement()
                {
                    Anchor = Alignment.TopLeft,

                    Height = 9,
                    Width = 9,
                    //Margin = new Thickness(4, 4)
                });
            }
        
            protected override void OnInit(IGuiRenderer renderer)
            {
                Background = renderer.GetTexture(AlexGuiTextures.HealthPlaceholder);
                Texture.Texture = renderer.GetTexture(AlexGuiTextures.HealthHeart);
                
                Set(_previousValue);
            }

            /// <inheritdoc />
            protected override void OnUpdate(GameTime gameTime)
            {
                base.OnUpdate(gameTime);
            }

            private void Shake()
            {
                
            }

            private HeartValue _previousValue = HeartValue.Full;
            public void Set(HeartValue value)
            {
                Texture.IsVisible = true;

                if (value != _previousValue)
                {
                    Shake();
                }

                _previousValue = value;
                
                switch (value)
                {
                    case HeartValue.Full:
                        if (GuiRenderer != null)
                            Texture.Texture = GuiRenderer.GetTexture(AlexGuiTextures.HealthHeart);
                        break;
                    case HeartValue.Half:
                        if (GuiRenderer != null) 
                            Texture.Texture = GuiRenderer.GetTexture(AlexGuiTextures.HealthHalfHeart);
                        break;
                    case HeartValue.None:
                        Texture.IsVisible = false;
                        break;
                }
            }
        }
    }
}