using Alex.Common;
using MojangAPI.Model;

namespace Alex.Utils.Auth
{
	public class MojangAuthResponse
	{
		public MojangAuthResult Result { get; }
		public ISession Session { get; set; }
		public PlayerProfile Profile { get; set; }

		public string ErrorMessage { get; set; }
		public string Error { get; set; }
		public int StatusCode { get; set; }

		public bool IsSuccess { get; set; }
		public MojangAuthResponse(MojangAuthResult result)
		{
			Result = result;
			IsSuccess = result == MojangAuthResult.Success;
		}

		public MojangAuthResponse(MojangAPI.Model.MojangAuthResponse response)
		{
			StatusCode = response.StatusCode;
			ErrorMessage = response.ErrorMessage;
			Error = response.Error;
			IsSuccess = response.IsSuccess;
			
			if (response.Session != null)
				Session = new JavaSession(response.Session);
			
		}
	}
}