using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Input.Listeners;
using Microsoft.Xna.Framework;

namespace Alex.API.Input
{
    public class PlayerInputManagerEvent
    {
        public PlayerIndex PlayerIndex { get; }
        public PlayerInputManager PlayerInputManager { get; }
        
        protected PlayerInputManagerEvent(PlayerIndex index, PlayerInputManager inputManager)
        {
            PlayerIndex = index;
            PlayerInputManager = inputManager;
        }
    }

    public sealed class PlayerInputManagerAdded : PlayerInputManagerEvent
    {
        public PlayerInputManagerAdded(PlayerIndex index, PlayerInputManager inputManager) : base(index, inputManager)
        {
        }
    }
    
    public sealed class PlayerInputManagerRemoved : PlayerInputManagerEvent
    {
        public PlayerInputManagerRemoved(PlayerIndex index, PlayerInputManager inputManager) : base(index, inputManager)
        {
        }
    }
    
    public class InputManager
    {
        private Game Game;

        private Dictionary<PlayerIndex, PlayerInputManager> PlayerInputManagers { get; } = new Dictionary<PlayerIndex, PlayerInputManager>();

        public ICursorInputListener CursorInputListener { get; }

        public int PlayerCount => PlayerInputManagers.Count;

        public EventHandler<PlayerInputManagerAdded> InputManagerAdded;

        public InputManager(Game game)
        {
            Game = game;

            var playerOne = GetOrAddPlayerManager(PlayerIndex.One);
            var mouseListener = new MouseInputListener(PlayerIndex.One);

            playerOne.AddListener(mouseListener);
            playerOne.AddListener(new KeyboardInputListener());

            CursorInputListener = mouseListener;
        }

        public PlayerInputManager GetOrAddPlayerManager(PlayerIndex playerIndex)
        {
            if (!PlayerInputManagers.TryGetValue(playerIndex, out var playerInputManager))
            {
                playerInputManager = new PlayerInputManager(playerIndex);
                PlayerInputManagers.Add(playerIndex, playerInputManager);
                
                InputManagerAdded?.Invoke(this, new PlayerInputManagerAdded(playerIndex, playerInputManager));
            }

            return playerInputManager;
        }

        public void Update(GameTime gameTime)
        {
            if (!Game.IsActive) //Do not process input when Alex isn't focused.
                return;
            
            foreach (var playerInputManager in PlayerInputManagers.Values.ToArray())
            {
               playerInputManager.Update(gameTime);
            }
        }

        public bool Any(Func<PlayerInputManager, bool> playerInputManagerFunc)
        {
            return PlayerInputManagers.Values.ToArray().Any(playerInputManagerFunc);
        }
    }
}
