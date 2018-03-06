using Alex.Graphics.UI.Input.Listeners;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.UI.Input
{
    public interface IInputManager
    {

        IMouseListener MouseListener { get; }

        void Update(GameTime gameTime);
    }
}
