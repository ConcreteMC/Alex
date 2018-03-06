using Alex.Graphics.UI.Input.Listeners;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.UI.Input
{
    public class UiInputManager : IInputManager
    {

        public IMouseListener MouseListener { get; private set; }

        public UiInputManager(UiManager uiManager)
        {
            MouseListener = new MouseListener(uiManager);
        }
        

        public void Update(GameTime gameTime)
        {
            MouseListener.Update(gameTime);
        }

    }
}
