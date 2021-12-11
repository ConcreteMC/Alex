using System;
using Alex.Common.Gui.Graphics;
using Alex.Entities;
using Microsoft.Xna.Framework;
using MiNET.Effects;
using RocketUI;

namespace Alex.Gui.Elements.Hud
{
	 public class AirComponent : StackContainer
    {
        private Player Player { get; }
        private AirTexture[] AirBubbles { get; }
        
        public AirComponent(Player player)
        {
           // Hunger = player.Hunger;
            Player = player;

            ChildAnchor = Alignment.BottomLeft;
            Orientation = Orientation.Horizontal;
            
            Height = 10;
            //Width = 10 * 8;
            AirBubbles = new AirTexture[10];
            for (int i = 0; i < 10; i++)
            {
                AddChild(AirBubbles[i] = new AirTexture()
                {
                   // Margin = new Thickness(0, 0, (i * 8), 0),
                    Anchor = Alignment.BottomRight
                });
            }
            
            player.HealthManager.OnAvailableAirChanged += (s, e) =>
            {
                UpdateAir(e.AirAvailable, e.MaxAirAvailable);
            };
        }

        private void UpdateAir(int availableAir, int maxAir)
        {
            var hearts = availableAir * (10d / maxAir);
            bool isRounded = (hearts % 1 == 0);
                
            var ceil = isRounded ? (int)hearts : (int)Math.Ceiling(hearts);
                
            for (int i = 0; i < AirBubbles.Length; i++)
            {
                HeartValue value = HeartValue.Full;
                    
                if ((i + 1) <= ceil)
                {
                    value = HeartValue.Full;
                        
                    if (!isRounded && (i + 1) == ceil)
                    {
                        value = HeartValue.Half;
                    }
                }
                else
                {
                    value = HeartValue.None;
                }
                    
                AirBubbles[^(i + 1)].Set(value);
            }
        }
        
        protected override void OnUpdate(GameTime gameTime)
        {
            if (Player.HeadInWater)
            {
                IsVisible = true;
            }
            else if (IsVisible)
            {
                IsVisible = false;
            }

            base.OnUpdate(gameTime);
        }
        
        public class AirTexture : RocketControl
        {
            private TextureElement Texture { get; set; }

            //private 
            public AirTexture()
            {
                Width = 9;
                Height = 9;
            
                AddChild(Texture = new TextureElement()
                {
                    Anchor = Alignment.TopRight,

                    Height = 9,
                    Width = 9,
                    //Margin = new Thickness(4, 4)
                });
            }
        
            protected override void OnInit(IGuiRenderer renderer)
            {
                //Background = renderer.GetTexture(AlexGuiTextures.HungerPlaceholder);
                Texture.Texture = renderer.GetTexture(AlexGuiTextures.AirBubble);
                Set(_previousValue);
            }

            private double _animationTimeElapsed = -1;

            private const double AnimationTime = 200d;
            /// <inheritdoc />
            protected override void OnUpdate(GameTime gameTime)
            {
                base.OnUpdate(gameTime);

                if (_animationTimeElapsed >= 0d)
                {
                    if (_animationTimeElapsed <= AnimationTime)
                    {
                        _animationTimeElapsed += gameTime.ElapsedGameTime.TotalMilliseconds;
                    }

                    if (_animationTimeElapsed >= AnimationTime)
                    {
                        Texture.IsVisible = false;
                        _animationTimeElapsed = -1d;
                    }
                }
            }

            private HeartValue _previousValue = HeartValue.Full;

            public void Set(HeartValue value)
            {

                switch (value)
                {
                    case HeartValue.Full:
                        Texture.IsVisible = true;
                        
                        if (GuiRenderer != null)
                            Texture.Texture = GuiRenderer.GetTexture(AlexGuiTextures.AirBubble);

                        break;

                    case HeartValue.Half:
                        Texture.IsVisible = true;

                        break;

                    case HeartValue.None:
                        if (_previousValue != HeartValue.None)
                        {
                            Texture.IsVisible = true;
                            if (GuiRenderer != null)
                                Texture.Texture = GuiRenderer.GetTexture(AlexGuiTextures.PoppedAirBubble);
                            
                            _animationTimeElapsed = 0d;
                        }

                        break;
                }

                _previousValue = value;
            }
        }
    }
}