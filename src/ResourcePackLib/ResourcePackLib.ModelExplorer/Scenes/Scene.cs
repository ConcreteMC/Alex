using System.Collections;
using Microsoft.Xna.Framework;
using ResourcePackLib.ModelExplorer.Abstractions;
using ResourcePackLib.ModelExplorer.Attributes;

namespace ResourcePackLib.ModelExplorer.Scenes
{
    public abstract partial class Scene : IScene, IDisposable
    {
        private static readonly Action<IDrawable, GameTime> DrawAction = (drawable, gameTime) => drawable.Draw(gameTime);
        private static readonly Action<IUpdateable, GameTime> UpdateAction = (updateable, gameTime) => updateable.Update(gameTime);

        [Service] protected IGame Game { get; private set; }
        [Service] protected IServiceProvider Services { get; private set; }
        
        public bool Initialized { get; private set; }
        public bool Visible { get; private set; }
        
        public void Initialize()
        {
            if (Initialized)
                return;

            OnInitialize();

            InitializeComponents();

            Initialized = true;
        }

        public void Show()
        {
            if(Visible) return;
            
            Visible = true;
            OnShow();
        }

        public void Hide()
        {
            if(!Visible) return;
            Visible = false;
            OnHide();
        }

        public void Update(GameTime gameTime)
        {
            _updateables.ForEachFilteredItem(UpdateAction, gameTime);
            OnUpdate(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            _drawables.ForEachFilteredItem(DrawAction, gameTime);
            OnDraw(gameTime);
        }

        protected virtual void OnInitialize()
        {
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnHide()
        {
        }

        protected virtual void OnUpdate(GameTime gameTime)
        {
        }

        protected virtual void OnDraw(GameTime gameTime)
        {
        }

        public virtual void OnDispose() { }
        
        public void Dispose()
        {
            Hide();
            OnDispose();
        }
    }
}