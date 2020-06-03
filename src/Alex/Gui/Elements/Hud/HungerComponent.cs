using System;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Alex.Entities;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gui.Elements
{
    public class HungerComponent : GuiStackContainer
    {
        private Player Player { get; }
        private HungerTexture[] Hungers { get; }
        
        private int Hunger { get; set; }
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
                AddChild(Hungers[i] = new HungerTexture()
                {
                   // Margin = new Thickness(0, 0, (i * 8), 0),
                    Anchor = Alignment.BottomRight
                });
            }
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            if (Player.HealthManager.Hunger != Hunger)
            {
                Hunger = Player.HealthManager.Hunger;
                
                var hearts = Player.HealthManager.Hunger * (10d / Player.HealthManager.MaxHunger);
                bool isRounded = (hearts % 1 == 0);
                
                var ceil = isRounded ? (int)hearts : (int)Math.Ceiling(hearts);
                
                for (int i = 0; i < Hungers.Length; i++)
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
                    
                    Hungers[^(i + 1)].Set(value);
                }
            }
            
            base.OnUpdate(gameTime);
        }
        
        public class HungerTexture : GuiControl
        {
            private GuiTextureElement Texture { get; set; }

            //private 
            public HungerTexture()
            {
                Width = 9;
                Height = 9;
            
                AddChild(Texture = new GuiTextureElement()
                {
                    Anchor = Alignment.TopRight,

                    Height = 9,
                    Width = 9,
                    //Margin = new Thickness(4, 4)
                });
            }
        
            protected override void OnInit(IGuiRenderer renderer)
            {
                Background = renderer.GetTexture(GuiTextures.HungerPlaceholder);
                Texture.Texture = renderer.GetTexture(GuiTextures.HungerFull);
            }

            public void Set(HeartValue value)
            {
                Texture.IsVisible = true;
            
                switch (value)
                {
                    case HeartValue.Full:
                        Texture.Texture = GuiRenderer.GetTexture(GuiTextures.HungerFull);
                        break;
                    case HeartValue.Half:
                        Texture.Texture = GuiRenderer.GetTexture(GuiTextures.HungerHalf);
                        break;
                    case HeartValue.None:
                        Texture.IsVisible = false;
                        break;
                }
            }
        }
    }
}