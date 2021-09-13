using Alex.Common.Graphics;
using Microsoft.Xna.Framework;

namespace Alex.Common.GameStates
{
    public interface IGameState
    {
        string Identifier { get; set; }
        
        IGameState ParentState { get; set; }

        void Load(IRenderArgs args);
        void Unload();

        void Draw(IRenderArgs args);

        void Update(GameTime gameTime);

        void Show();
        void Hide();
    }
}
