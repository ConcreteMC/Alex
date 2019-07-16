using System.Collections.Generic;
using log4net;
using MiNET.Blocks;
using MiNET.Plugins;
using MiNET.Worlds;
using LogManager = log4net.LogManager;

namespace MiNET.AlexDebug
{
    public class PluginCore : Plugin
    {
        //private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(PluginCore));
        private static readonly ILog Log = LogManager.GetLogger(typeof(PluginCore));
        public LevelManager LevelManager { get; private set; }
        public PluginCore()
        {
            
        }

        protected override void OnEnable()
        {
            Log.Info($"Enabled Alex test plugin.");
            Context.LevelManager.LevelCreated += LevelManagerOnLevelCreated;
            Context.Server.PlayerFactory.PlayerCreated += OnPlayerCreated;
            Context.PluginManager.LoadCommands(new CommandHandler(this));
            Context.LevelManager.Levels.Add(new Level(Context.LevelManager, "w2", new AnvilWorldProvider()
                {
                    MissingChunkProvider = new SuperflatGenerator(Dimension.Overworld)
                    {
                        BlockLayers = new List<Block>()
                        {
                            BlockFactory.GetBlockById(7),
                            BlockFactory.GetBlockById(3),
                            BlockFactory.GetBlockById(3),
                            BlockFactory.GetBlockById(12)
                        }
                    }
                }, 
                new EntityManager(), GameMode.Creative, Difficulty.Easy));
            
            // Context.LevelManager.GetLevel(null, "")
           
            foreach (var level in Context.LevelManager.Levels)
            {
                LinkLevelEvents(level);
            }

            LevelManager = Context.LevelManager;
        }
        
        private void LevelManagerOnLevelCreated(object sender, LevelEventArgs e)
        {
            LinkLevelEvents(e.Level);
        }
        
        private void LinkLevelEvents(Level level)
        {
            level.PlayerAdded += LevelOnPlayerAdded;
        }

        private void LevelOnPlayerAdded(object sender, LevelEventArgs e)
        {
            e.Player.SendTitle("MiNET Debug Plugin", TitleType.SubTitle);
            e.Player.SendTitle("Alex");
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