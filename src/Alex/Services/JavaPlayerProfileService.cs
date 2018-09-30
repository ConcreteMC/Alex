using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.GameStates;
using Alex.Utils;
using Microsoft.Xna.Framework.Graphics;
using MojangSharp;
using MojangSharp.Endpoints;
using MojangSharp.Responses;
using Newtonsoft.Json;

namespace Alex.Services
{
	public class JavaPlayerProfileService : IPlayerProfileService
	{
		private PlayerProfile _currentProfile;

		public event EventHandler<PlayerProfileChangedEventArgs> ProfileChanged;
		public event EventHandler<PlayerProfileAuthenticateEventArgs> Authenticate;

		public PlayerProfile CurrentProfile
		{
			get => _currentProfile;
			private set
			{
				if (_currentProfile?.Equals(value) ?? false)
					return;

				_currentProfile = value;
				ProfileChanged?.Invoke(this, new PlayerProfileChangedEventArgs(_currentProfile));
			}
		}
		
		public async Task<bool> TryAuthenticateAsync(string username, string password)
		{
			var auth = await new Authenticate(new Credentials()
			{
				Username = username,
				Password = password
			})
				  .PerformRequestAsync();

			if (auth.IsSuccess)
			{
				var profile = await new Profile(auth.SelectedProfile.Value).PerformRequestAsync();

				Texture2D texture = null;
				if (profile.Properties.SkinUri != null)
				{
					SkinUtils.TryGetSkin(profile.Properties.SkinUri, Alex.Instance.GraphicsDevice, out texture);
				}

				var playerProfile = new PlayerProfile(auth.SelectedProfile.Value, auth.SelectedProfile.PlayerName, texture,
												auth.AccessToken, auth.ClientToken);

				Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs(playerProfile));
				CurrentProfile = playerProfile;
				return await Validate(auth.AccessToken);
			}
			else
			{
				Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs(auth.Error.ErrorMessage));
				return false;
			}
		}

		private async Task<bool> Validate(string accessToken)
		{
			return await new Validate(accessToken)
				.PerformRequestAsync()
				.ContinueWith(task =>
			{
				var r = task.Result;
				if (r.IsSuccess)
				{
					Alex.SaveJava(CurrentProfile.Username);
					Alex.Instance.GameStateManager.SetActiveState<TitleState>();
					return true;
				}
				else
				{
					Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs(r.Error.ErrorMessage));
					return false;
				}
			});
		}
	}
}
