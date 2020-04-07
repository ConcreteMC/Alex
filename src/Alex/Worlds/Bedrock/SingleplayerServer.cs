using System.Net;
using System.Threading.Tasks;
using Alex.API.Network;
using Alex.API.Services;
using MiNET;
using MiNET.Utils;
using MiNET.Worlds;
using LevelInfo = Alex.API.World.LevelInfo;

namespace Alex.Worlds.Bedrock
{
    public class SingleplayerServer : BedrockWorldProvider
    {
        private MiNetServer Server { get; set; }
        public IPEndPoint ConnectionEndpoint { get; set; }
        
        private Level MiNETLevel { get; }
        public SingleplayerServer(string world, Alex alex, IPEndPoint endPoint, PlayerProfile profile, DedicatedThreadPool threadPool, out INetworkProvider networkProvider) : base(alex, endPoint, profile, threadPool, out networkProvider)
        {
            Server = new MiNetServer(new IPEndPoint(IPAddress.Loopback, 0));
            ConnectionEndpoint = Server.Endpoint;
            
            Server.LevelManager = new LevelManager();
            MiNETLevel = Server.LevelManager.GetLevel(null, world);
        }

        protected override void Initiate(out LevelInfo info)
        {
            base.Initiate(out info);
            if (MiNETLevel.WorldProvider is MiNET.Worlds.AnvilWorldProvider anvilWorldProvider)
            {
                var lvl = anvilWorldProvider.LevelInfo;
                
                info = new LevelInfo();
                
                info.SpawnX = lvl.SpawnX;
                info.SpawnY = lvl.SpawnY;
                info.SpawnZ = lvl.SpawnZ;
                info.Initialized = lvl.Initialized;
                info.Raining = lvl.Raining;
                info.Thundering = lvl.Thundering;
                info.Time = lvl.Time;
                info.DayTime = lvl.DayTime;
                info.LevelName = lvl.LevelName;
            }
        }

        public override Task Load(ProgressReport progressReport)
        {
            if (base.Client is BedrockClient client)
            {
                client.ServerEndpoint = ConnectionEndpoint;
            }

            return Task.Run(() => { Server.StartServer(); }).ContinueWith((t) => base.Load(progressReport));
        }

        public override void Dispose()
        {
            base.Dispose();
            Server.StopServer();
        }
    }
}