using System.Net;

namespace Alex.Utils
{
	public class BedrockMotd
	{
		public string     Edition;
		public string     MOTD;
		public int        MaxPlayers;
		public int        Players;
		public int        ProtocolVersion;
		public string     ClientVersion;
		public IPEndPoint ServerEndpoint;
		public long       Latency;

		public BedrockMotd(string raw)
		{
			if (string.IsNullOrWhiteSpace(raw)) return;

			var split = raw.Split(';');
			int i     = 0;
			Edition = split[i++];
			MOTD = split[i++];

			if (int.TryParse(split[i++], out int protocolVersion))
			{
				ProtocolVersion = protocolVersion;
			}
			
			ClientVersion = split[i++];

			if (int.TryParse(split[i++], out int players))
			{
				Players = players;
			}

			if (int.TryParse(split[i++], out int maxplayers))
			{
				MaxPlayers = maxplayers;
			}
		}
	}
}