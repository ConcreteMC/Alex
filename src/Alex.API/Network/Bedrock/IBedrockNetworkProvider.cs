using System;
using System.Numerics;
using System.Threading;
using Alex.API.Data;
using Alex.API.World;
using MiNET.Net;
using MiNET.Utils;
using LevelInfo = MiNET.Worlds.LevelInfo;

namespace Alex.API.Network.Bedrock
{
    public interface IBedrockNetworkProvider : INetworkProvider, IDisposable
    {
        IWorldReceiver WorldReceiver { get; set; }
        Vector3 SpawnPoint { get; set; }
        LevelInfo LevelInfo { get; }
        PlayerLocation CurrentLocation { get; set; }
        int ChunkRadius { get; set; }
        long EntityId { get; set; }
        long NetworkEntityId { get; set; }
        int PlayerStatus { get; set; }
        bool HasSpawned { get; set; }
        AutoResetEvent PlayerStatusChanged { get; set; }

        void Start(ManualResetEventSlim resetEvent);
        
        void SendPacket(Packet packet);

        void RequestChunkRadius(int radius);
        void SendDisconnectionNotification();
        void SendMcpeMovePlayer();

        void InitiateEncryption(byte[] x5u, byte[] salt);

        void ShowDisconnect(string reason, bool useTranslation = false);
    }
}