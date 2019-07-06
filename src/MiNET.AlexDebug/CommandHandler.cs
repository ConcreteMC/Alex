using System.Linq.Expressions;
using log4net;
using MiNET.Entities.Passive;
using MiNET.Net;
using MiNET.Plugins.Attributes;
using MiNET.Worlds;

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

        [Command(Name = "wt", Aliases = new[]{"wt"})]
        public void TestWorldTransfers(Player player)
        {
            var targetLevel = player.Level;
            if (player.Level.LevelId != "w2")
            {
                targetLevel = Core.LevelManager.GetLevel(player, "w2");
            }
            else
            {
                targetLevel = Core.LevelManager.GetLevel(player, Dimension.Overworld.ToString());
            }

            if (targetLevel == player.Level)
            {
                player.SendMessage($"Could not transfer.");
                return;
            }
            
            player.ChangeDimension(targetLevel, player.SpawnPosition, targetLevel.Dimension);
        }

        [Command(Name = "transfer", Aliases = new[] {"transfer"})]
        public void ServerTransferTest(Player player)
        {
            McpeTransfer transfer = McpeTransfer.CreateObject();
            transfer.serverAddress = "test.pmmp.io";
            transfer.port = 19132;
            
            player.SendPacket(transfer);
        }
    }
}