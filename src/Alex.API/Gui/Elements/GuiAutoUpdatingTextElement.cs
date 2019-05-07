using System;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements
{
    public class GuiAutoUpdatingTextElement : GuiTextElement
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