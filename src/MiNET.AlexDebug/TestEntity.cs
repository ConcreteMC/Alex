using MiNET.Entities.Passive;
using MiNET.Worlds;
using NLog;

namespace MiNET.AlexDebug
{
    public class TestEntity : Cow
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(PluginCore));
        
        public TestEntity(Level level) : base(level)
        {
            
        }

        public override void DoInteraction(int actionId, Player player)
        {
            player.SendMessage("Entity Hit!");
            Log.Info($"Player {player.Username} hit entity {this.EntityTypeId}. {EntityId}");
         //   base.DoInteraction(actionId, player);
        }
    }
}