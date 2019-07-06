using System.Linq.Expressions;
using log4net;
using MiNET.Entities.Passive;
using MiNET.Plugins.Attributes;

namespace MiNET.AlexDebug
{
    public class CommandHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CommandHandler));
        
        private PluginCore Core { get; }
        public CommandHandler(PluginCore core)
        {
            Core = core;
        }

        [Command(Name = "testentity", Aliases = new[]{"testentity"})]
        public void SpawnTestEntity(Player player)
        {
            TestEntity villager = new TestEntity(player.Level);
            villager.KnownPosition = player.KnownPosition;
            villager.NoAi = true;
            
            player.Level.AddEntity(villager);
            villager.SpawnEntity();
            
            player.SendMessage("Entity spawned.");
        }
    }
}