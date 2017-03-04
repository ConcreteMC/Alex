using System;
using System.Collections.Concurrent;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
    public class GamestateManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GamestateManager));
        
        private ConcurrentDictionary<string, Gamestate> ActiveStates { get; }
        private Gamestate ActiveState { get; set; }
        private object _lock = new object();

        private GraphicsDevice Graphics { get; }
        private SpriteBatch SpriteBatch { get; }

        public GamestateManager(GraphicsDevice graphics, SpriteBatch spriteBatch)
        {
            Graphics = graphics;
            SpriteBatch = spriteBatch;

            ActiveStates = new ConcurrentDictionary<string, Gamestate>();
        }

        public void AddState(string name, Gamestate state)
        {
            state.Init(new RenderArgs()
            {
                SpriteBatch = SpriteBatch,
                GraphicsDevice = Graphics,
                GameTime = new GameTime()
            });

            ActiveStates.AddOrUpdate(name, state, (s, gamestate) =>
            {
                return state;
            });
        }


        public bool RemoveState(string name)
        {
            Gamestate state;
            if (ActiveStates.TryRemove(name, out state))
            {
                lock (_lock)
                {
                    if (ActiveState == state)
                    {
                        ActiveState = null;
                    }
                }
                return true;
            }
            return false;
        }

        public bool SetActiveState(Gamestate state)
        {
            lock (_lock)
            {
                ActiveState = state;
            }
            return true;
        }

        public bool SetActiveState(string name)
        {
            Gamestate state;
            if (!ActiveStates.TryGetValue(name, out state))
            {
                return false;
            }

            lock (_lock)
            {
                ActiveState = state;
            }
            return true;
        }

        public void Draw(GameTime gameTime)
        {
            lock (_lock)
            {
                if (ActiveState == null) return;

                try
                {
                    RenderArgs args = new RenderArgs()
                    {
                        SpriteBatch = SpriteBatch,
                        GameTime = gameTime,
                        GraphicsDevice = Graphics
                    };

                    ActiveState.Rendering3D(args);
                    ActiveState.Rendering2D(args);
                }
                catch (Exception ex)
                {
                    Log.Warn("An exception occured while trying to render!", ex);
                }
            }
        }

        public void Update(GameTime gameTime)
        {
           // foreach (var i in ActiveStates.ToArray())
            {
                try
                {
                    lock (_lock)
                    {
                        ActiveState.UpdateCall(gameTime);
                    }
                   // i.Value.UpdateCall(gameTime);
                }
                catch(Exception ex)
                {
                    Log.Warn($"An exception occured while trying to call Update!", ex);
                }
            }
        }
    }
}
