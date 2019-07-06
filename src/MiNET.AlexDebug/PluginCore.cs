using System;
using log4net;
using MiNET.Plugins;
using MiNET.Worlds;
using NLog;
using LogManager = log4net.LogManager;

namespace MiNET.AlexDebug
{
    public class PluginCore : Plugin
    {
        //private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(PluginCore));
        private static readonly ILog Log = LogManager.GetLogger(typeof(PluginCore));
        public PluginCore()
        {
            
        }

        protected override void OnEnable()
        {
            Log.Info($"Enabled Alex test plugin.");
            Context.LevelManager.LevelCreated += LevelManagerOnLevelCreated;
            Context.Server.PlayerFactory.PlayerCreated += OnPlayerCreated;
            Context.PluginManager.LoadCommands(new CommandHandler(this));
            
            foreach (var level in Context.LevelManager.Levels)
            {
                LinkLevelEvents(level);
            }
        }
        
        private void LevelManagerOnLevelCreated(object sender, LevelEventArgs e)
        {
            LinkLevelEvents(e.Level);
        }
        
        private void LinkLevelEvents(Level level)
        {
            
        }

        private void OnPlayerCreated(object sender, PlayerEventArgs e)
        {
            
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }
    }
}