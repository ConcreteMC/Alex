namespace Alex.Utils.Auth
{
	public class MsaDeviceAuthConnectResponse
	{
		public string user_code;
		public string device_code;
		public string verification_uri;
		public int    interval;
		public int    expires_in;
	};
}