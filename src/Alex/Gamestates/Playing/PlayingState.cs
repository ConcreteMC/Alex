using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Alex.Blocks;
using Alex.Gamestates.Playing;
using Alex.Network;
using Alex.Properties;
using Alex.Rendering;
using Alex.Rendering.Camera;
using Alex.Utils;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MiNET;
using MiNET.Net;
using Player = Alex.Entities.Player;

namespace Alex.Gamestates
{
	public class PlayingState : Gamestate
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(PlayingState));

		protected Alex Alex { get; }
		private World World { get; }
		private FirstPersonCamera Camera { get; }
		private CameraComponent CamComponent { get; }

		private int RenderDistance { get; set; }
		public PlayingState(Alex alex) : base(alex.GraphicsDevice)
		{
			Alex = alex;
			Camera = new FirstPersonCamera(alex.GameSettings.RenderDistance, Vector3.Zero, Vector3.Zero);

			World = new World(Alex, Alex.GraphicsDevice, Camera);

			RenderDistance = Alex.GameSettings.RenderDistance;

			CamComponent = new CameraComponent(Camera, Graphics, World, alex.GameSettings);
		}

		private List<string> ChatMessages { get; set; }
		//private FrameCounter FpsCounter { get; set; }
		private FpsMonitor FpsCounter { get; set; }
		private Texture2D CrosshairTexture { get; set; }
		private bool RenderDebug { get; set; } = false;
		private bool RenderChatInput { get; set; } = false;
		private MiNetClient Client { get; set; }
		public override void Init(RenderArgs args)
		{
			_oldKeyboardState = Keyboard.GetState();
			//FpsCounter = new FrameCounter();

			FpsCounter = new FpsMonitor();
			CrosshairTexture = TextureUtils.ImageToTexture2D(args.GraphicsDevice,Resources.crosshair);

			_selectedBlock = Vector3.Zero;
			ChatMessages = new List<string>();
			Alex.OnCharacterInput += OnCharacterInput;
			World.ResetChunks();

			if (Alex.IsMultiplayer)
			{
				Log.Info("Connecting to server...");
				Client = new MiNetClient(Alex.ServerEndPoint, Alex.Username);
				Client.OnStartGame += Client_OnStartGame;
				Client.OnChunkData += Client_OnChunkData;
				Client.OnChatMessage += Client_OnChatMessage;
				Client.OnDisconnect += ClientOnOnDisconnect;
				Client.OnBlockUpdate += Client_OnBlockUpdate;
				Client.OnPlayerMovement += Client_OnPlayerMovement;
				Client.OnMcpeChunkRadiusUpdate += ClientOnOnMcpeChunkRadiusUpdate;
				Client.OnMcpePlayerStatus += Client_OnMcpePlayerStatus;

				if (Client.Connect())
				{
					Log.Info("Connected to server...");
				}
			}

			base.Init(args);
		}

		private void Client_OnMcpePlayerStatus(McpePlayStatus packet)
		{
			
		}

		protected internal void Disconnect()
		{
			Client?.Disconnect();
		}

		private void Client_OnPlayerMovement(McpeMovePlayer packet)
		{
			if (packet.runtimeEntityId == 0)
			{
				Camera.Position = new Vector3(packet.x, packet.y + 1.8f, packet.z);
			}
		}

		private void Client_OnBlockUpdate(McpeUpdateBlock packet)
		{
		  /*  foreach (var i in packet.blocks)
			{
				Alex.Instance.World.SetBlock(i.Coordinates.X, i.Coordinates.Y, i.Coordinates.Z,
					BlockFactory.GetBlock(i.Id, i.Metadata));
			}*/
		}

		private void ClientOnOnDisconnect(string reason)
		{
			Alex.GamestateManager.SetActiveState(new DisconnectedState(Alex, reason));
			Alex.IsMultiplayer = false; //Unknown if next one is mp as well, reset to false.
		}

		private void Client_OnChatMessage(string message, string source, MiNET.MessageType type)
		{
			if (type == MessageType.Chat)
			{
				if (source != string.Empty)
				{
					message = string.Format("<{0}> {1}", source, message);
				}
				ChatMessages.Add(message);
			}
		}

		private long EntityId;
		private void Client_OnStartGame(MiNET.Worlds.GameMode gamemode, System.Numerics.Vector3 spawnPoint, long entityId)
		{
			EntityId = entityId;
			McpeRequestChunkRadius request = McpeRequestChunkRadius.CreateObject();
			request.chunkRadius = Alex.GameSettings.RenderDistance;
			Client.SendPackage(request);
			Camera.Position = new Vector3((float) spawnPoint.X, (float) spawnPoint.Y + 1.8f, (float) spawnPoint.Z);
		  //  Alex.Instance.IsFreeCam = false;
		}

		private void ClientOnOnMcpeChunkRadiusUpdate(McpeChunkRadiusUpdate packet)
		{
			RenderDistance = packet.chunkRadius;
		}

		private void Client_OnChunkData(MiNET.Worlds.ChunkColumn chunkColumn)
		{
			var vec = new Vector3(chunkColumn.x, 0, chunkColumn.z);
			Chunk convertedChunk = new Chunk(vec);
			for (int x = 0; x < Chunk.ChunkWidth; x++)
			{
				for (int y = 0; y < Chunk.ChunkHeight; y++)
				{
					for (int z = 0; z < Chunk.ChunkDepth; z++)
					{
						var blockId = chunkColumn.GetBlock(x, y, z);
						var metadata = chunkColumn.GetMetadata(x, y, z);

						var skyLight = chunkColumn.GetSkylight(x, y, z);
						var blockLight = chunkColumn.GetBlocklight(x, y, z);

						var height = chunkColumn.GetHeight(x, z);

						convertedChunk.SetHeight(x,z,height);
						convertedChunk.SetBlocklight(x, y, z, blockLight);
						convertedChunk.SetSkylight(x, y, z, skyLight);

						convertedChunk.SetBlock(x, y, z, BlockFactory.GetBlock(blockId, metadata));
					}
				}
			}
			World.ChunkManager.AddChunk(convertedChunk, vec, true);

			//The following does the chunk cleanup.

			var powed = Math.Pow(RenderDistance, 2);
			if (World.ChunkCount > powed)
			{
				Vector3[] chunks = World.ChunkManager.Chunks.Keys.ToArray();

				Vector3 pos = Camera.Position;
				var cPos = new Vector3(((int) Math.Floor(pos.X)) >> 4, 0, ((int) Math.Floor(pos.Z)) >> 4);

				float maxX = cPos.X + RenderDistance;
				float minX = cPos.X - RenderDistance;

				float maxZ = cPos.Z + RenderDistance;
				float minZ = cPos.Z - RenderDistance;

				Vector3 max = new Vector3(Math.Max(maxX, minX), 0, Math.Max(maxZ, minZ));
				Vector3 min = new Vector3(Math.Min(maxX, minX), 0, Math.Min(maxZ, minZ));

				foreach (var i in chunks)
				{
					if ((i.X > max.X || i.X < min.X) || (i.Z > max.Z || i.Z < min.Z))
					{
						World.ChunkManager.RemoveChunk(i);
					}
				}
			}
		}

		private void OnCharacterInput(object sender, char c)
		{
			if (RenderChatInput)
			{
#if FNA
				if (c == (char)8) //BackSpace
				{
					BackSpace();
					return;
				}
				if (c == (char) 13)
				{
					SubmitMessage();
					return;
				}
#endif
				_input += c;
			}
		}

		private void BackSpace()
		{
			if (_input.Length > 0) _input = _input.Remove(_input.Length - 1, 1);
		}

		private void SubmitMessage()
		{
			//Submit message
			if (_input.Length > 0)
			{
				if (Alex.IsMultiplayer)
				{
					Client.SendChat(_input);
				}
				else
				{
					ChatMessages.Add("<Me> " + _input);
				}
			}
			_input = string.Empty;
			RenderChatInput = false;
		}

		public override void Stop()
		{
			Disconnect();
		}

		public override void Render2D(RenderArgs args)
		{
			//FpsCounter.Update((float) args.GameTime.ElapsedGameTime.TotalSeconds);
			try
			{
				args.SpriteBatch.Begin();

#if MONOGAME
			args.SpriteBatch.Draw(CrosshairTexture,
				new Vector2(CenterScreen.X - CrosshairTexture.Width/2f, CenterScreen.Y - CrosshairTexture.Height/2f));
#endif
#if FNA
				args.SpriteBatch.Draw(CrosshairTexture,
					new Vector2(CenterScreen.X - CrosshairTexture.Width/2f, CenterScreen.Y - CrosshairTexture.Height/2f),
					Color.White);
#endif
				if (RenderChatInput)
				{
					var heightCalc = Alex.Font.MeasureString("!");
					string chatInput = _input.StripIllegalCharacters();
					if (chatInput.Length > 0)
					{
						heightCalc = Alex.Font.MeasureString(chatInput);
					}

					int extra = 0;
					if (heightCalc.X > args.GraphicsDevice.Viewport.Width/2f)
					{
						extra = (int) (heightCalc.X - args.GraphicsDevice.Viewport.Width/2f);
					}

					args.SpriteBatch.FillRectangle(
						new Rectangle(0, (int) (args.GraphicsDevice.Viewport.Height - (heightCalc.Y + 5)),
							(args.GraphicsDevice.Viewport.Width/2) + extra, (int) heightCalc.Y),
						new Color(Color.Black, 64));

					args.SpriteBatch.DrawString(Alex.Font, chatInput,
						new Vector2(5, (int) (args.GraphicsDevice.Viewport.Height - (heightCalc.Y + 5))), Color.White);
				}

				var count = 2;
				foreach (var msg in ChatMessages.TakeLast(5).Reverse())
				{
					var amsg = msg.StripColors();
					amsg = amsg.StripIllegalCharacters();
					var heightCalc = Alex.Font.MeasureString(amsg);

					int extra = 0;
					if (heightCalc.X > args.GraphicsDevice.Viewport.Width/2f)
					{
						extra = (int) (heightCalc.X - args.GraphicsDevice.Viewport.Width/2f);
					}

					args.SpriteBatch.FillRectangle(
						new Rectangle(0, (int) (args.GraphicsDevice.Viewport.Height - ((heightCalc.Y*count) + 10)),
							(args.GraphicsDevice.Viewport.Width/2) + extra, (int) heightCalc.Y),
						new Color(Color.Black, 64));
					args.SpriteBatch.DrawString(Alex.Font, amsg,
						new Vector2(5, (int) (args.GraphicsDevice.Viewport.Height - ((heightCalc.Y*count) + 10))), Color.White);
					count++;
				}

				Block selBlock = new Air();
				if (_selectedBlock.Y > 0 && _selectedBlock.Y < 256)
				{
					selBlock = World.GetBlock(_selectedBlock.X, _selectedBlock.Y, _selectedBlock.Z);
					var boundingBox = new BoundingBox(_selectedBlock + selBlock.BlockModel.Offset,
						_selectedBlock + selBlock.BlockModel.Offset + selBlock.BlockModel.Size);

					args.SpriteBatch.RenderBoundingBox(
						boundingBox,
						Camera.ViewMatrix, Camera.ProjectionMatrix, Color.LightGray);
				}

				if (RenderDebug)
				{
					var fpsString = string.Format("Alex {0} ({1} FPS, {2} chunk updates)", Alex.Version,
						Math.Round(FpsCounter.Value), World.ChunkUpdates);
					var meisured = Alex.Font.MeasureString(fpsString);

					args.SpriteBatch.FillRectangle(new Rectangle(0, 0, (int) meisured.X, (int) meisured.Y),
						new Color(Color.Black, 64));
					args.SpriteBatch.DrawString(Alex.Font,
						fpsString, new Vector2(0, 0),
						Color.White);

					var y = (int) meisured.Y;
					var positionString = "Position: " + Camera.Position;
					meisured = Alex.Font.MeasureString(positionString);

					args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int) meisured.X, (int) meisured.Y),
						new Color(Color.Black, 64));
					args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);

					y += (int) meisured.Y;

					positionString = "Looking at: " + _selectedBlock;
					meisured = Alex.Font.MeasureString(positionString);

					args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int) meisured.X, (int) meisured.Y),
						new Color(Color.Black, 64));
					args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);

					y += (int) meisured.Y;

					positionString = string.Format("Block: {0} ID: {1}:{2}", selBlock, selBlock.BlockId, selBlock.Metadata);
					meisured = Alex.Font.MeasureString(positionString);

					args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int) meisured.X, (int) meisured.Y),
						new Color(Color.Black, 64));
					args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);

					y += (int) meisured.Y;

					positionString = "Vertices: " + World.Vertices;
					meisured = Alex.Font.MeasureString(positionString);

					args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int) meisured.X, (int) meisured.Y),
						new Color(Color.Black, 64));
					args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);

					y += (int) meisured.Y;

					positionString = "Chunks: " + World.ChunkCount;
					meisured = Alex.Font.MeasureString(positionString);

					args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int) meisured.X, (int) meisured.Y),
						new Color(Color.Black, 64));
					args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);
				}
			}
			finally
			{
				args.SpriteBatch.End();
			}
			base.Render2D(args);
		}

		public override void Render3D(RenderArgs args)
		{
			Camera.UpdateAspectRatio(args.GraphicsDevice.Viewport.AspectRatio);

			World.Render();

			FpsCounter.Update();
		}

		private string _input = "";
		private Vector3 _selectedBlock;
		private KeyboardState _oldKeyboardState;
		private MouseState _oldMouseState;
		public override void OnUpdate(GameTime gameTime)
		{
			if (Alex.IsActive)
			{
				UpdateRayTracer(Alex.GraphicsDevice, World);

				CheckInput(gameTime);

				SendPositionUpdate(gameTime);
			}

			base.OnUpdate(gameTime);
		}

		private DateTime LastMovementUpdate { get; set; } = DateTime.MinValue;
		protected internal void SendPositionUpdate(GameTime gameTime)
		{
			DateTime now = DateTime.UtcNow;
			if (now.Subtract(LastMovementUpdate).TotalMilliseconds >= 50)
			{
				var camPos = Camera.Position;
				if (camPos != _oldPosition)
				{
					McpeMovePlayer movePlayerPacket = McpeMovePlayer.CreateObject();
					movePlayerPacket.x = camPos.X;
					movePlayerPacket.y = camPos.Y;
					movePlayerPacket.z = camPos.Z;
					movePlayerPacket.yaw = 0;
					movePlayerPacket.headYaw = 0;
					movePlayerPacket.pitch = 0;
					movePlayerPacket.runtimeEntityId = EntityId;
					Client.SendPackage(movePlayerPacket);
				}
				_oldPosition = camPos;

				LastMovementUpdate = now;
			}
		}

		protected void UpdateRayTracer(GraphicsDevice graphics, World world)
		{
			_selectedBlock = RayTracer.Raytrace(graphics, world, Camera);
		}

		protected void CheckInput(GameTime gameTime)
		{
			CamComponent.Update(gameTime, !RenderChatInput);

			MouseState currentMouseState = Mouse.GetState();
			if (currentMouseState != _oldMouseState)
			{
				if (currentMouseState.LeftButton == ButtonState.Pressed)
				{
					if (_selectedBlock.Y > 0 && _selectedBlock.Y < 256)
					{
						World.SetBlock(_selectedBlock.X, _selectedBlock.Y, _selectedBlock.Z, new Air());
					}
				}

				if (currentMouseState.RightButton == ButtonState.Pressed)
				{
					if (_selectedBlock.Y > 0 && _selectedBlock.Y < 256)
					{
						World.SetBlock(_selectedBlock.X, _selectedBlock.Y + 1, _selectedBlock.Z, new Stone());
					}
				}
			}
			_oldMouseState = currentMouseState;

			KeyboardState currentKeyboardState = Keyboard.GetState();
			if (currentKeyboardState != _oldKeyboardState)
			{
				if (currentKeyboardState.IsKeyDown(KeyBinds.Menu))
				{
					if (RenderChatInput)
					{
						RenderChatInput = false;
					}
					else
					{
						Alex.GamestateManager.SetActiveState(new InGameMenuState(Alex, this, currentKeyboardState));
					}
				}

				if (currentKeyboardState.IsKeyDown(KeyBinds.DebugInfo))
				{
					RenderDebug = !RenderDebug;
				}

				if (currentKeyboardState.IsKeyDown(KeyBinds.ToggleFreeCam))
				{
					CamComponent.IsFreeCam = !CamComponent.IsFreeCam;
				}

				if (currentKeyboardState.IsKeyDown(KeyBinds.ReBuildChunks))
				{
					World.RebuildChunks();
				}

				if (RenderChatInput) //Handle Input
				{
#if MONOGAME
					if (currentKeyboardState.IsKeyDown(Keys.Back))
					{
						BackSpace();
					}

					if (currentKeyboardState.IsKeyDown(Keys.Enter))
					{
						SubmitMessage();
					}
#endif
				}
				else
				{
					if (currentKeyboardState.IsKeyDown(KeyBinds.Chat))
					{
						RenderChatInput = !RenderChatInput;
						_input = string.Empty;
					}
				}
			}
			_oldKeyboardState = currentKeyboardState;
		}

		private Vector3 _oldPosition = Vector3.Zero;
	}
}
