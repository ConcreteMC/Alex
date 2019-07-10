using MiNET.Entities;
using MiNET.Worlds;
using NLog;

namespace MiNET.AlexDebug
{
    public class TestEntity : Entity
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(PluginCore));
        
        public TestEntity(Level level, EntityType type) : base(type, level)
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