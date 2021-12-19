using System;
using System.Net;
using Alex.Common.Services;
using Alex.Net;
using Alex.Net.Bedrock;
using Alex.Networking.Bedrock.RakNet;
using Alex.Utils;
using MiNET;
using MiNET.Net;
using MiNET.Net.RakNet;
using MiNET.Plugins.Commands;

namespace Alex.Worlds.Multiplayer;

public class SinglePlayerClient : BedrockClient
{
	private readonly SingleplayerBedrockWorldProvider _provider;

	/// <inheritdoc />
	public SinglePlayerClient(Alex alex, IPEndPoint endpoint, PlayerProfile playerProfile, SingleplayerBedrockWorldProvider wp) :
		base(alex, endpoint, playerProfile, wp)
	{
		_provider = wp;
	}


	/// <inheritdoc />
	protected override BedrockClientPacketHandler CreateMessageHandler()
	{
		return base.CreateMessageHandler();
	}

	/// <inheritdoc />
	protected override ICustomMessageHandler CustomMessageHandlerFactory(RaknetSession session)
	{
		return base.CustomMessageHandlerFactory(session);
	}
}

public class SingleplayerBedrockWorldProvider : BedrockWorldProvider
{
	public static MiNetServer Server { get; set; }
	private readonly MiNetServer _miNetServer;

	private IDisposable _optionsBinding = null;
	/// <inheritdoc />
	public SingleplayerBedrockWorldProvider(Alex alex,
		MiNetServer miNetServer,
		PlayerProfile profile) : base(
		alex, new IPEndPoint(IPAddress.Loopback, 19132), profile)
	{
		_miNetServer = miNetServer;
		_miNetServer.StartServer();
		_miNetServer.PlayerFactory.PlayerCreated += PlayerFactoryOnPlayerCreated;
		_miNetServer.PluginManager?.LoadCommands(new VanillaCommands());
		
		_optionsBinding = alex.Options.AlexOptions.VideoOptions.RenderDistance.Bind(RenderDistanceChanged);
		SetRenderDistance(alex.Options.AlexOptions.VideoOptions.RenderDistance.Value);
	}

	/// <inheritdoc />
	protected override BedrockClient GetClient(Alex alex, IPEndPoint endPoint, PlayerProfile profile)
	{
		return new SinglePlayerClient(alex, endPoint, profile, this);
	}

	/// <inheritdoc />
	protected override void Initiate()
	{
		base.Initiate();
	}

	private int _renderDistance = 11;

	private void SetRenderDistance(int value)
	{
		_renderDistance = value;
		AlexConfigProvider.Instance.Set("ViewDistance", value.ToString());
		AlexConfigProvider.Instance.Set("MaxViewDistance", value.ToString());

		foreach (var level in _miNetServer.LevelManager.Levels)
		{
			level.ViewDistance = value;
		}
		
		var p = _minetPlayer;

		if (p != null)
		{
			p.MaxViewDistance = value;
			p.SetChunkRadius(value);
		}
	}
	
	private void RenderDistanceChanged(int oldvalue, int newvalue)
	{
		SetRenderDistance(newvalue);
	}

	private Player _minetPlayer = null;
	private void PlayerFactoryOnPlayerCreated(object? sender, PlayerEventArgs e)
	{
		_minetPlayer = e.Player;
		e.Player.MaxViewDistance = _renderDistance;
		e.Player.SetChunkRadius(_renderDistance);
		
		e.Player.ActionPermissions = ActionPermissions.Operator;
		e.Player.CommandPermission = 4;
		e.Player.PermissionLevel = PermissionLevel.Operator;
		e.Player.SendAdventureSettings();
	}

	/// <inheritdoc />
	protected override void OnSpawn()
	{
		base.OnSpawn();
	}

	/// <inheritdoc />
	public override void Dispose()
	{
		base.Dispose();

		var server = _miNetServer;

		if (server != null)
		{
			foreach (var level in server.LevelManager.Levels)
			{
				level.WorldProvider.SaveChunks();
			}

			server.StopServer();
		}

		_optionsBinding?.Dispose();
	}
}