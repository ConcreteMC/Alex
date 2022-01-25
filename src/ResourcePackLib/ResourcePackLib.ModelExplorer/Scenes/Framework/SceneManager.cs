using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using ResourcePackLib.ModelExplorer.Abstractions;
using ResourcePackLib.ModelExplorer.Utilities;

namespace ResourcePackLib.ModelExplorer.Scenes
{
    public class SceneManager : DrawableGameComponent
    {
        public IScene ActiveScene => _activeScene?.Scene;

        private WrappedScene _activeScene;
        private readonly Stack<WrappedScene> _sceneStack;

        private bool _initialized = false;
        private IServiceProvider _serviceProvider;

        public SceneManager(IGame game) : base(game.Game)
        {
            _serviceProvider = game.ServiceProvider;
            DrawOrder = 0;
            _sceneStack = new Stack<WrappedScene>();
        }

        public override void Initialize()
        {
            _initialized = true;
            base.Initialize();

            ActiveScene?.Initialize();
        }

        public void SetScene<TScene>() where TScene : class, IScene
        {
            ResetStack();
            SetSceneInternal<TScene>();
        }

        private void SetSceneInternal<TScene>() where TScene : class, IScene
        {
            var scope = _serviceProvider.CreateScope();
            var scene = scope.ServiceProvider.GetOrCreateInstance<TScene>();
            var wrappedScene = new WrappedScene(scope, scene);
            SetSceneInternal(wrappedScene);
        }
        
        private void SetSceneInternal([NotNull] WrappedScene wrappedScene)
        {
            if (_initialized)
                wrappedScene.Scene.Initialize();
            
            _activeScene?.Scene.Hide();
            _activeScene?.Dispose();
            _activeScene = null;
            
            _activeScene = wrappedScene;
            _activeScene?.Scene.Show();
        }

        public void PushScene<TScene>() where TScene : class, IScene
        {
            if (ActiveScene != null)
                _sceneStack.Push(_activeScene);
            SetSceneInternal<TScene>();
        }

        public void Pop()
        {
            if (_sceneStack.TryPop(out var previousScene))
            {
                SetSceneInternal(previousScene);
            }
        }

        public void Back() => Pop();

        public void ResetStack()
        {
            if (_sceneStack.Count > 0)
            {
                while (_sceneStack.TryPop(out var s))
                {
                    s?.Scene.Hide();
                    s?.Dispose();
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            ActiveScene?.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            ActiveScene?.Draw(gameTime);
        }

        class WrappedScene : IDisposable
        {
            public WrappedScene(IServiceScope scope, IScene scene)
            {
                ServiceScope = scope;
                Scene = scene;
            }

            public IServiceScope ServiceScope { get; }
            public IScene Scene { get; }

            public void Dispose()
            {
                ServiceScope?.Dispose();
            }
        }
    }
}