using Alex.API.Graphics;
using Microsoft.Xna.Framework;

namespace Alex.API.GameStates
{
    public interface IGameState
    {
        IGameState ParentState { get; set; }

        void Load(IRenderArgs args);
        void Unload();

        void Draw(IRenderArgs args);

        void Update(GameTime gameTime);

        void Show();
        void Hide();
    }
}
