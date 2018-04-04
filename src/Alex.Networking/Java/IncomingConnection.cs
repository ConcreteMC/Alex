#region Imports

using System;
using System.Net.Sockets;

#endregion

namespace Alex.Networking.Java
{
    internal sealed class IncomingConnection
    {
        public NetServer Owner { get; }
        public Socket Socket { get; }
        public DateTime RequestTime { get; }
        public IncomingConnection(NetServer owner, Socket socket)
        {
            Owner = owner;
            Socket = socket;
            RequestTime = DateTime.UtcNow;
        }
    }
}
