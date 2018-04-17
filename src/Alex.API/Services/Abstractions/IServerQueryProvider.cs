using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Alex.API.Services
{
    public interface IServerQueryProvider
    {

        Task<ServerQueryResponse> QueryServerAsync(string hostname, ushort port);

    }

    public class ServerQueryResponse
    {
        public bool Success { get; }
        
        public string ErrorMessage { get; }
        public ServerQueryStatus Status { get; }

        public ServerQueryResponse(bool success, ServerQueryStatus status)
        {
            Success = success;
            Status = status;
        }

        public ServerQueryResponse(bool success, string errorMessage, ServerQueryStatus status)
        {
            Success = success;
            ErrorMessage = errorMessage;
            Status = status;
        }
    }

    public struct ServerQueryStatus
    {
        public bool Success { get; set; }
        public long Delay   { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public string Address { get; set; }
        public ushort Port { get; set; }

        public string Motd { get; set; }
        public string Version { get; set; }
        public int ProtocolVersion { get; set; }
        public int NumberOfPlayers { get; set; }
        public int MaxNumberOfPlayers { get; set; }
        public string FaviconDataRaw { get; set; }
    }
}
