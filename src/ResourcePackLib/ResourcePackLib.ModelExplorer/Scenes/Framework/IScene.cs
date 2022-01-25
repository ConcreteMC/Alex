using Microsoft.Xna.Framework;

namespace ResourcePackLib.ModelExplorer.Scenes
{
    public interface IScene
    {
        GameComponentCollection Components { get; }
        
        bool Initialized { get; }
        
        void Initialize();
        void Show();
        void Hide();
        void Update(GameTime gameTime);
        void Draw(GameTime gameTime);
    }
}