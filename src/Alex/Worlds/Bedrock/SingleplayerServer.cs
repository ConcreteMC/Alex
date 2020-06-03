using System.Net;
using System.Threading.Tasks;
using Alex.API.Network;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Net;
using MiNET;
using MiNET.Utils;
using MiNET.Worlds;
using OpenAPI;
using OpenAPI.Utils;
using OpenAPI.World;
using LevelInfo = Alex.API.World.LevelInfo;

namespace Alex.Worlds.Bedrock
{
    public class SingleplayerServer : BedrockWorldProvider
    {
        private OpenServer Server { get; set; }
        private OpenAPI.OpenApi Api { get; set; }
        public IPEndPoint ConnectionEndpoint { get; set; }
        
        private OpenLevel MiNETLevel { get; }
        public SingleplayerServer(string world, Gamemode gamemode, Difficulty difficulty, Alex alex, IPEndPoint endPoint, PlayerProfile profile, DedicatedThreadPool threadPool, out NetworkProvider networkProvider) : base(alex, endPoint, profile, threadPool, out networkProvider)
        {
            Server = new OpenServer();
            ReflectionHelper.SetPrivatePropertyValue(
                typeof(OpenServer), Server, "Endpoint", new IPEndPoint(IPAddress.Loopback, 0));
            
            ConnectionEndpoint = Server.Endpoint;
            Api = ReflectionHelper.GetPrivatePropertyValue<OpenApi>(typeof(OpenServer), Server, "OpenApi");

            MiNET.Worlds.AnvilWorldProvider provider = new MiNET.Worlds.AnvilWorldProvider(world);

            MiNETLevel = new OpenLevel(
                Api, Api.LevelManager, "default", provider, Api.LevelManager.EntityManager, (GameMode) gamemode,
                difficulty);
            
            Api.LevelManager.SetDefaultLevel(MiNETLevel);
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
            
            return Task.Run(
                () =>
                {
                    Server.StartServer();
                    Client.ServerEndpoint = Server.Endpoint;
                }).ContinueWith((t) => base.Load(progressReport));
        }

        public override void Dispose()
        {
            base.Dispose();
            Server.StopServer();
        }
    }
}