using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Alex.Blocks;
using Alex.Graphics.Items;
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
			CrosshairTexture = ResManager.ImageToTexture2D(Resources.crosshair);
			_selectedBlock = Vector3.Zero;
		    ChatMessages = new List<string>();
            Alex.Instance.OnCharacterInput += OnCharacterInput;
            Alex.Instance.World.ResetChunks();

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

		        if (Client.Connect())
		        {
                    Log.Info("Connected to server...");
		        }
		    }

			base.Init(args);
		}

	    protected internal void Disconnect()
	    {
	        Client?.Disconnect();
	    }

        private void Client_OnPlayerMovement(McpeMovePlayer packet)
        {
            if (packet.entityId == 0)
            {
                Game.GetCamera().Position = new Vector3(packet.x, packet.y + 1.8f, packet.z);
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
	        Alex.Instance.SetGameState(new DisconnectedState(reason));
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

	    private int RenderDistance = Alex.Instance.GameSettings.RenderDistance;

	    private long EntityId;
        private void Client_OnStartGame(MiNET.Worlds.GameMode gamemode, System.Numerics.Vector3 spawnPoint, long entityId)
        {
            EntityId = entityId;
            McpeRequestChunkRadius request = McpeRequestChunkRadius.CreateObject();
            request.chunkRadius = Alex.Instance.GameSettings.RenderDistance;
            Client.SendPackage(request);
            Game.GetCamera().Position = new Vector3((float) spawnPoint.X, (float) spawnPoint.Y + 1.8f, (float) spawnPoint.Z);
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
	        for (int x = 0; x < 16; x++)
	        {
	            for (int y = 0; y < 128; y++)
	            {
	                for (int z = 0; z < 16; z++)
	                {
	                    var blockId = chunkColumn.GetBlock(x, y, z);
	                    var metadata = chunkColumn.GetMetadata(x, y, z);
	                    convertedChunk.SetBlock(x, y, z, BlockFactory.GetBlock(blockId, metadata));
	                }
	            }
	        }
	        Alex.Instance.World.ChunkManager.AddChunk(convertedChunk, vec, true);

            //The following does the chunk cleanup.

            var powed = Math.Pow(RenderDistance, 2);
            if (Alex.Instance.World.ChunkCount > powed)
            {
                Vector3[] chunks = Alex.Instance.World.ChunkManager.Chunks.Keys.ToArray();

                Vector3 pos = Game.GetCamera().Position;
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
                        Alex.Instance.World.ChunkManager.RemoveChunk(i);
                    }
                }
            }
	    }

	    private void OnCharacterInput(object sender, char c)
	    {
	        if (RenderChatInput)
	        {
	            _input += c;
	        }
	    }

	    public override void Stop()
		{
			Disconnect();
		}

		public override void Render2D(RenderArgs args)
		{
			//FpsCounter.Update((float) args.GameTime.ElapsedGameTime.TotalSeconds);
			args.SpriteBatch.Begin();
			args.SpriteBatch.Draw(CrosshairTexture,
				new Vector2(CenterScreen.X - CrosshairTexture.Width/2f, CenterScreen.Y - CrosshairTexture.Height/2f));

		    if (RenderChatInput)
		    {
		        var heightCalc = Alex.Font.MeasureString("!");
		        if (_input.Length > 0)
		        {
		            heightCalc = Alex.Font.MeasureString(_input);
		        }

                int extra = 0;
                if (heightCalc.X > args.GraphicsDevice.Viewport.Width / 2f)
                {
                    extra = (int)(heightCalc.X - args.GraphicsDevice.Viewport.Width / 2f);
                }

		        args.SpriteBatch.FillRectangle(
		            new Rectangle(0, (int) (args.GraphicsDevice.Viewport.Height - (heightCalc.Y + 5)),
		                (args.GraphicsDevice.Viewport.Width/2) + extra, (int) heightCalc.Y),
		            new Color(Color.Black, 64));

                args.SpriteBatch.DrawString(Alex.Font, _input,
                    new Vector2(5, (int)(args.GraphicsDevice.Viewport.Height - (heightCalc.Y + 5))), Color.White);
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
                        (args.GraphicsDevice.Viewport.Width / 2) + extra, (int) heightCalc.Y),
		            new Color(Color.Black, 64));
		        args.SpriteBatch.DrawString(Alex.Font, amsg,
		            new Vector2(5, (int) (args.GraphicsDevice.Viewport.Height - ((heightCalc.Y*count) + 10))), Color.White);
		        count++;
		    }

			Block selBlock = new Air();
			if (_selectedBlock.Y > 0 && _selectedBlock.Y < 256)
			{
				selBlock = Alex.Instance.World.GetBlock(_selectedBlock.X, _selectedBlock.Y, _selectedBlock.Z);
				var boundingBox = new BoundingBox(_selectedBlock + selBlock.BlockModel.Offset,
					_selectedBlock + selBlock.BlockModel.Offset + selBlock.BlockModel.Size);

				args.SpriteBatch.RenderBoundingBox(
					boundingBox,
					Game.MainCamera.ViewMatrix, Game.MainCamera.ProjectionMatrix, Color.LightGray);
			}

			if (RenderDebug)
			{
				var fpsString = string.Format("Alex {0} ({1} FPS, {2} chunk updates)", Alex.Version, Math.Round(FpsCounter.Value), Alex.Instance.World.ChunkUpdates);
				var meisured = Alex.Font.MeasureString(fpsString);

				args.SpriteBatch.FillRectangle(new Rectangle(0, 0, (int)meisured.X, (int)meisured.Y), new Color(Color.Black, 64));
				args.SpriteBatch.DrawString(Alex.Font,
					fpsString, new Vector2(0, 0),
					Color.White);

				var y = (int) meisured.Y;
				var positionString = "Position: " + Game.MainCamera.Position;
				meisured = Alex.Font.MeasureString(positionString);

				args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int)meisured.X, (int)meisured.Y), new Color(Color.Black, 64));
				args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0,y), Color.White);

				y += (int)meisured.Y;

				positionString = "Looking at: " + _selectedBlock;
				meisured = Alex.Font.MeasureString(positionString);

				args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int)meisured.X, (int)meisured.Y), new Color(Color.Black, 64));
				args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);

                y += (int)meisured.Y;

				positionString =  string.Format("Block: {0} ID: {1}:{2}", selBlock, selBlock.BlockId, selBlock.Metadata);
				meisured = Alex.Font.MeasureString(positionString);

				args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int)meisured.X, (int)meisured.Y), new Color(Color.Black, 64));
				args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);

				y += (int)meisured.Y;

				positionString = "Vertices: " + Alex.Instance.World.Vertices;
                meisured = Alex.Font.MeasureString(positionString);

                args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int)meisured.X, (int)meisured.Y), new Color(Color.Black, 64));
                args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);

                y += (int)meisured.Y;

                positionString = "Chunks: " + Alex.Instance.World.ChunkCount;
                meisured = Alex.Font.MeasureString(positionString);

                args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int)meisured.X, (int)meisured.Y), new Color(Color.Black, 64));
                args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);
            }
			args.SpriteBatch.End();

			base.Render2D(args);
		}

		public override void Render3D(RenderArgs args)
		{
			Alex.Instance.World.Render();
			base.Render3D(args);

		    FpsCounter.Update();
		}

	    private string _input = "";
		private Vector3 _selectedBlock;
		private KeyboardState _oldKeyboardState;
	    private MouseState _oldMouseState;
		public override void OnUpdate(GameTime gameTime)
		{
			UpdateFpsCounter();

		    if (Alex.Instance.IsActive)
		    {
                UpdateRayTracer();

                CheckInput(gameTime);

                SendPositionUpdate(gameTime);
		    }

			base.OnUpdate(gameTime);
		}

	    protected internal void UpdateFpsCounter()
	    {
	      //  FpsCounter.Update();
	    }

	    private DateTime LastMovementUpdate { get; set; } = DateTime.MinValue;
	    protected internal void SendPositionUpdate(GameTime gameTime)
	    {
	        DateTime now = DateTime.UtcNow;
	        if (now.Subtract(LastMovementUpdate).TotalMilliseconds >= 50)
	        {
	            var camPos = Game.GetCamera().Position;
	            if (camPos != _oldPosition)
	            {
	                McpeMovePlayer movePlayerPacket = McpeMovePlayer.CreateObject();
	                movePlayerPacket.x = camPos.X;
	                movePlayerPacket.y = camPos.Y;
	                movePlayerPacket.z = camPos.Z;
	                movePlayerPacket.yaw = 0;
	                movePlayerPacket.headYaw = 0;
	                movePlayerPacket.pitch = 0;
	                movePlayerPacket.entityId = EntityId;
	                Client.SendPackage(movePlayerPacket);
	            }
	            _oldPosition = camPos;

	            LastMovementUpdate = now;
	        }
	    }

	    protected void UpdateRayTracer()
	    {
	        _selectedBlock = RayTracer.Raytrace();
	    }

	    protected void CheckInput(GameTime gameTime)
	    {
            Alex.Instance.UpdateCamera(gameTime, !RenderChatInput);

            MouseState currentMouseState = Mouse.GetState();
	        if (currentMouseState != _oldMouseState)
	        {
	            if (currentMouseState.LeftButton == ButtonState.Pressed)
	            {
	                if (_selectedBlock.Y > 0 && _selectedBlock.Y < 256)
	                {
	                    Alex.Instance.World.SetBlock(_selectedBlock.X, _selectedBlock.Y, _selectedBlock.Z, new Air());
	                }
	            }

	            if (currentMouseState.RightButton == ButtonState.Pressed)
	            {
	                if (_selectedBlock.Y > 0 && _selectedBlock.Y < 256)
	                {
	                    Alex.Instance.World.SetBlock(_selectedBlock.X, _selectedBlock.Y + 1, _selectedBlock.Z, new Stone());
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
	                    Alex.Instance.SetGameState(new InGameMenuState(this, currentKeyboardState), false, false);
	                }
	            }

	            if (currentKeyboardState.IsKeyDown(KeyBinds.DebugInfo))
	            {
	                RenderDebug = !RenderDebug;
	            }

	            if (currentKeyboardState.IsKeyDown(KeyBinds.ToggleFreeCam))
	            {
	                Alex.Instance.IsFreeCam = !Alex.Instance.IsFreeCam;
	            }

	            if (RenderChatInput) //Handle Input
	            {
	                if (currentKeyboardState.IsKeyDown(Keys.Back))
	                {
	                    if (_input.Length > 0) _input = _input.Remove(_input.Length - 1, 1);
	                }

	                if (currentKeyboardState.IsKeyDown(Keys.Enter))
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
