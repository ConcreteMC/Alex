using System;
using Microsoft.Xna.Framework;

namespace RocketUI.Elements
{
    public class GuiAutoUpdatingTextElement : TextBlock
    {
        private readonly Func<string> _updateFunc;

        public GuiAutoUpdatingTextElement(Func<string> updateFunc, bool hasBackground = false) : base(hasBackground)
        {
            _updateFunc = updateFunc;
            Text        = _updateFunc();
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            Text = _updateFunc();
        }
    }
}