using System.Net;
using Newtonsoft.Json;

namespace Alex.Utils.Auth
{
	public interface IDeviceAuthConnectResponse
	{
		string UserCode { get; }
		string DeviceCode { get; }
		string VerificationUrl { get; }
		int Interval { get; }
		int ExpiresIn { get; }
	}

	public class MsaDeviceAuthConnectResponse : IDeviceAuthConnectResponse
	{
		/// <inheritdoc />
		[JsonProperty("user_code")]
		public string UserCode { get; set; }

		/// <inheritdoc />
		[JsonProperty("device_code")]
		public string DeviceCode { get; set; }

		/// <inheritdoc />
		[JsonProperty("verification_uri")]
		public string VerificationUrl { get; set; }

		/// <inheritdoc />
		[JsonProperty("interval")]
		public int Interval { get; set; }

		/// <inheritdoc />
		[JsonProperty("expires_in")]
		public int ExpiresIn { get; set; }
	};

	public class MinecraftTokenResponse
	{
		[JsonProperty("username")] public string Username { get; set; }

		[JsonProperty("roles")] public string[] Roles { get; set; }

		[JsonProperty("access_token")] public string AccessToken { get; set; }

		[JsonProperty("token_type")] public string TokenType { get; set; }

		[JsonProperty("expires_in")] public long ExpiresIn { get; set; }
	}

	public class ApiResponse<T>
	{
		public T Result { get; set; }
		public HttpStatusCode StatusCode { get; set; }
		public bool IsSuccess { get; set; } = false;

		public ApiResponse(bool isSuccess, HttpStatusCode statusCode, T result)
		{
			IsSuccess = isSuccess;
			StatusCode = statusCode;
			Result = result;
		}
	}
}