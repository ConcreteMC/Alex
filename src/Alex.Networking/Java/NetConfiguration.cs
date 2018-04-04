#region Imports

using System.Net;

#endregion

namespace Alex.Networking.Java
{
    public sealed class NetConfiguration
    {
        public ProtocolType Protocol { get; set; } = ProtocolType.Tcp;
		public IPAddress Host { get; set; } = IPAddress.Any;
        public int Port { get; set; } = 8181;
    }
}
