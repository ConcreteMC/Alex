using System;
using Microsoft.Xna.Framework;
using RocketUI.Graphics;

namespace Alex.API.Gui.Elements
{
    public class GuiFpsCounter : GuiMCTextElement
    {
        private int _framesPerSecond = 0; 
            
        private int _frames;
        private TimeSpan _lastGameTime;

        public GuiFpsCounter() : base()
        {

        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            Text = $"FPS: {_framesPerSecond:D3}";
        }

        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            _frames++;
            _lastGameTime += gameTime.ElapsedGameTime;

            if (_lastGameTime.TotalSeconds >= 1)
            {
                _framesPerSecond = _frames;
                _lastGameTime = TimeSpan.Zero;
                _frames = 0;
            }

            base.OnDraw(graphics, gameTime);
        }
    }
}
