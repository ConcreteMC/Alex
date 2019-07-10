using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.API.Services
{
	using J = Newtonsoft.Json.JsonPropertyAttribute;
	using N = Newtonsoft.Json.NullValueHandling;

	public delegate void PingServerDelegate(ServerPingResponse response);
	public delegate void ServerStatusDelegate(ServerQueryResponse reponse);
    public interface IServerQueryProvider
    {
	    Task QueryBedrockServerAsync(string hostname, ushort port, PingServerDelegate pingCallback = null, ServerStatusDelegate statusCallBack = null);
		Task QueryServerAsync(string hostname, ushort port, PingServerDelegate pingCallback = null, ServerStatusDelegate statusCallBack = null);

    }

	public class ServerListPingDescriptionJson
	{
		public string Text { get; set; }

		public class DescriptionConverter : JsonConverter<Description>
		{
			public override Description ReadJson(JsonReader reader, Type objectType, Description existingValue,
				bool hasExistingValue, JsonSerializer serializer)
			{
				if (reader.TokenType == JsonToken.StartObject)
				{
					JObject item = JObject.Load(reader);
					return item.ToObject<Description>();
				}
				else if (reader.TokenType == JsonToken.String)
				{
					return new Description()
					{
						Text = (string)reader.Value
					};
				}

				return null;
			}

			public override bool CanWrite
			{
				get { return false; }
			}

			public override void WriteJson(JsonWriter writer, Description value, JsonSerializer serializer)
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class ServerQuery
	{
		[J("version")] public Version Version { get; set; }
		[J("players")] public Players Players { get; set; }

		[JsonConverter(typeof(ServerListPingDescriptionJson.DescriptionConverter))]
		[J("description")] public Description Description { get; set; }
		[J("favicon")] public string Favicon { get; set; }
		[J("modinfo")] public Modinfo Modinfo { get; set; }
	}

	public partial class Description
	{
		[J("extra")] public List<Extra> Extra { get; set; }
		[J("text")] public string Text { get; set; }
	}

	public partial class Extra
	{
		[J("text")] public string Text { get; set; }
		[J("color", NullValueHandling = N.Ignore)] public string Color { get; set; }
		[J("bold", NullValueHandling = N.Ignore)] public bool? Bold { get; set; }
		[J("italic", NullValueHandling = N.Ignore)] public bool? Italic { get; set; }
		[J("underlined", NullValueHandling = N.Ignore)] public bool? Underlined { get; set; }
		[J("strikethrough", NullValueHandling = N.Ignore)] public bool? Strikethrough { get; set; }
		[J("obfuscated", NullValueHandling = N.Ignore)] public bool? Obfuscated { get; set; }
	}

	public partial class Modinfo
	{
		[J("type")] public string Type { get; set; }
		[J("modList")] public List<object> ModList { get; set; }
	}

	public partial class Players
	{
		[J("max")] public int Max { get; set; }
		[J("online")] public int Online { get; set; }
	}

	public partial class Version
	{
		[J("name")] public string Name { get; set; }
		[J("protocol")] public int Protocol { get; set; }
	}

	public partial class ServerQuery
	{
		public static ServerQuery FromJson(string json) => JsonConvert.DeserializeObject<ServerQuery>(json);
	}


	public class ServerPingResponse
	{
		public bool Success { get; }
		public string ErrorMessage { get; }
		public long Ping { get; }

		public ServerPingResponse(bool success, long ping)
		{
			Success = success;
			Ping = ping;
		}

		public ServerPingResponse(bool success, string error, long ping)
		{
			Success = success;
			ErrorMessage = error;
			Ping = ping;
		}
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
	    public bool WaitingOnPing { get; set; }
        public bool Success { get; set; }
        public long Delay   { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public string Address { get; set; }
        public ushort Port { get; set; }

	    public ServerQuery Query { get; set; }
    }
}
