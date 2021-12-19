using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Alex.Common.Gui.Elements.Icons;
using Alex.Common.Gui.Graphics;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Gamestates.Common;
using Alex.Gamestates.Multiplayer;
using fNbt;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Utils;
using MiNET.Worlds;
using Newtonsoft.Json;
using NLog;
using RocketUI;

namespace Alex.Gamestates.Singleplayer;

public enum WorldType
{
	Anvil,
	LevelDB
}

public class WorldInfo
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(WorldInfo));
	public string Name { get; }
	private readonly IStorageSystem _storageSystem;
	public WorldType Type { get; private set; } = WorldType.Anvil;
	
	public LevelInfo LevelInfo { get; set; }
	public byte[] IconData { get; set; }

	public bool IsCompatible { get; set; } = false;
	public string GameVersion { get; set; } = "N/A";
	public WorldInfo(IStorageSystem storageSystem, string name)
	{
		Name = name;
		_storageSystem = storageSystem;
	}

	private const string LevelDat = "level.dat";
	public bool Initiate()
	{
		if (_storageSystem.Exists(LevelDat))
		{
			if (_storageSystem.TryGetDirectory("db", out _) && LoadLevelDB())
			{
				//LevelDB?
				return true;
			}
			
			return LoadAnvil();
		}
		return false;
	}

	private bool LoadLevelDB()
	{
		try
		{
			using (var fs = _storageSystem.OpenFileStream(LevelDat, FileMode.Open))
			{
				var file = new NbtFile
				{
					BigEndian = false,
					UseVarInt = false
				};

				fs.Seek(8, SeekOrigin.Begin);
				file.LoadFromStream(fs, NbtCompression.None);
				
				var levelInfo = file.RootTag.Deserialize<LevelInfoBedrock>();

				LevelInfo = new LevelInfo()
				{
					LevelName = levelInfo.LevelName,
					RainTime = levelInfo.RainTime,
					ThunderTime = levelInfo.LightningTime,
					DayTime = levelInfo.Time % 24000,
					Time = levelInfo.Time,
					Version = levelInfo.NetworkVersion,
					DataVersion = levelInfo.StorageVersion,
					SpawnX = levelInfo.SpawnX,
					SpawnY = levelInfo.SpawnY,
					SpawnZ = levelInfo.SpawnZ,
					AllowCommands = levelInfo.CommandsEnabled == 1,
					GameType = levelInfo.GameType,
					GeneratorVersion = levelInfo.Generator,
					LastPlayed = levelInfo.LastPlayed,
					RandomSeed = levelInfo.RandomSeed
				};

				GameVersion = levelInfo.BaseGameVersion;
				//IsCompatible = LevelInfo.DataVersion <= 922; //Anvil 1.11
			}
			
			if (_storageSystem.TryReadBytes("world_icon.jpeg", out var iconData))
			{
				IconData = iconData;
			}

			Type = WorldType.LevelDB;
			IsCompatible = true;

			return LevelInfo != null;
		}
		catch (Exception ex)
		{
			Log.Warn(ex, $"Could not process world: {Name}");
		}

		return false;
	}
	
	private bool LoadAnvil()
	{
		try
		{
			NbtFile file = new NbtFile();
			using (var fs = _storageSystem.OpenFileStream(LevelDat, FileMode.Open))
			{
				file.LoadFromStream(fs, NbtCompression.AutoDetect);

				NbtTag dataTag = file.RootTag["Data"];
				LevelInfo = new LevelInfo(dataTag);
				IsCompatible = LevelInfo.DataVersion <= 922; //Anvil 1.11
			}

			if (_storageSystem.TryReadBytes("icon.png", out var iconData))
			{
				IconData = iconData;
			}
			
			if (ProtocolVersionInfo.TryGetFromDataVersion(LevelInfo.DataVersion, out var versionInfo))
			{
				GameVersion = versionInfo.MinecraftVersion;
			}
			else
			{
				GameVersion = $"Unknown: {LevelInfo.DataVersion}";
			}
			
			Type = WorldType.Anvil;

			return LevelInfo != null;
		}
		catch (Exception ex)
		{
			Log.Warn(ex, $"Could not process world: {Name}");
		}

		return false;
	}
}

public class ProtocolVersionInfo
{
	[JsonProperty("minecraftVersion")]
	public string MinecraftVersion { get; set; }
	
	[JsonProperty("version")]
	public int Version { get; set; }
	
	[JsonProperty("dataVersion")]
	public int DataVersion { get; set; }
	
	[JsonProperty("usesNetty")]
	public bool UsesNetty { get; set; }
	
	[JsonProperty("majorVersion")]
	public string MajorVersion { get; set; }
	
	public ProtocolVersionInfo()
	{
		
	}

	private static ProtocolVersionInfo[] _versions;

	static ProtocolVersionInfo()
	{
		var raw = ResourceManager.ReadStringResource("Alex.Resources.javaProtocolVersions.json");
		_versions = JsonConvert.DeserializeObject<ProtocolVersionInfo[]>(raw);
	}

	public static bool TryGetFromDataVersion(int dataVersion, out ProtocolVersionInfo versionInfo)
	{
		versionInfo = default;
		var result = _versions.FirstOrDefault(x => x.DataVersion == dataVersion);

		if (result != null)
		{
			versionInfo = result;

			return true;
		}

		return false;
	}
	
	public static bool TryGetFromVersion(int version, out ProtocolVersionInfo versionInfo)
	{
		versionInfo = default;
		var result = _versions.FirstOrDefault(x => x.Version == version);

		if (result != null)
		{
			versionInfo = result;

			return true;
		}

		return false;
	}
}

public class WorldListEntry : ListItem
{
	public WorldInfo WorldInfo { get; }
	
	private readonly TextureElement     _serverIcon;
	private readonly StackContainer     _textWrapper;
        
	private          TextElement _worldName;
	private readonly TextElement _serverMotd;

	private WorldStatusIcon _statusElement;
	public WorldListEntry(WorldInfo worldInfo)
	{
		WorldInfo = worldInfo;
		SetFixedSize(355, 36);

		Margin = new Thickness(5, 5, 5, 5);
		Padding = Thickness.One;
		Anchor = Alignment.TopFill;

		AddChild( _serverIcon = new TextureElement()
		{
			Width = GuiServerListEntryElement.ServerIconSize,
			Height = GuiServerListEntryElement.ServerIconSize,
                
			Anchor = Alignment.TopLeft
		});
		
		AddChild(_statusElement = new WorldStatusIcon(worldInfo.IsCompatible ? $"{worldInfo.Type}" : $"{ChatColors.Red}Not Supported")
		{
			Anchor = Alignment.TopRight,
		});

		AddChild( _textWrapper = new StackContainer()
		{
			ChildAnchor = Alignment.TopFill,
			Anchor = Alignment.TopLeft
		});
		_textWrapper.Padding = new Thickness(0,0);
		_textWrapper.Margin = new Thickness(GuiServerListEntryElement.ServerIconSize + 5, 0, 0, 0);

		_textWrapper.AddChild(_worldName = new TextElement()
		{
			Text = worldInfo.Name,
			Margin = Thickness.Zero
		});

		_textWrapper.AddChild(_serverMotd = new TextElement()
		{
			Margin = new Thickness(0, 0, 5, 0),
			Text = $"{worldInfo.LevelInfo.LevelName} ({DateTimeOffset.FromUnixTimeMilliseconds(worldInfo.LevelInfo.LastPlayed).DateTime.ToString("g")})\n{((GameMode)worldInfo.LevelInfo.GameType).ToString()}, Version: {worldInfo.GameVersion}"
		});
	}


	private Texture2D _texture = null;
	/// <inheritdoc />
	protected override void OnInit(IGuiRenderer renderer)
	{
		base.OnInit(renderer);

		if (WorldInfo.IconData != null)
		{
			_texture = TextureUtils.ImageToTexture2D(this, Alex.Instance.GraphicsDevice, WorldInfo.IconData);
			_serverIcon.Texture = _texture;
		}
		else
		{
			_serverIcon.Texture = renderer.GetTexture(AlexGuiTextures.DefaultServerIcon);
		}
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		if (disposing)
		{
			_texture?.Dispose();
			_texture = null;
		}
	}
}