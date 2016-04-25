using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Blocks;
using Alex.Network;
using Alex.Properties;
using Alex.Rendering;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MiNET;
using MiNET.Net;

namespace Alex.Gamestates
{
	public class PlayingState : Gamestate
	{
        private List<string> ChatMessages { get; set; }
		private FrameCounter FpsCounter { get; set; }
		private Texture2D CrosshairTexture { get; set; }
		private bool RenderDebug { get; set; } = false;
	    private bool RenderChatInput { get; set; } = false;
        private MiNetClient Client { get; set; }
		public override void Init(RenderArgs args)
		{
			OldKeyboardState = Keyboard.GetState();
			FpsCounter = new FrameCounter();
			CrosshairTexture = ResManager.ImageToTexture2D(Resources.crosshair);
			SelectedBlock = Vector3.Zero;
		    ChatMessages = new List<string>()
		    {
		        "<Alex> there",
                "<Alex> This is a test message."
		    };
            Alex.Instance.OnCharacterInput += OnCharacterInput;

		    if (Alex.IsMultiplayer)
		    {
		        Logging.Info("Connecting to server...");
                Client = new MiNetClient(Alex.ServerEndPoint, Alex.Username);
                Client.OnStartGame += Client_OnStartGame;
                Client.OnChunkData += Client_OnChunkData;
                Client.OnChatMessage += Client_OnChatMessage;
                Client.OnDisconnect += ClientOnOnDisconnect;
                Client.OnBlockUpdate += Client_OnBlockUpdate;
		        if (Client.Connect())
		        {
                    Alex.Instance.World.ResetChunks();
                    Logging.Info("Connected to server...");
		        }
		    }

			base.Init(args);
		}

	    private void Client_OnBlockUpdate(McpeUpdateBlock packet)
	    {
	        foreach (var i in packet.blocks)
	        {
	            Alex.Instance.World.SetBlock(i.Coordinates.X, i.Coordinates.Y, i.Coordinates.Z,
	                BlockFactory.GetBlock(i.Id, i.Metadata));
	        }
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

        private void Client_OnStartGame(MiNET.Worlds.GameMode gamemode, MiNET.Utils.Vector3 spawnPoint, long entityId)
        {
            McpeRequestChunkRadius request = McpeRequestChunkRadius.CreateObject();
            request.chunkRadius = 12;
            Client.SendPackage(request);
            Game.GetCamera().Position = new Vector3((float) spawnPoint.X, (float) spawnPoint.Y, (float) spawnPoint.Z);
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
                        convertedChunk.SetBlock(x,y,z, BlockFactory.GetBlock(blockId, metadata));
                    }
                }
            }
            Alex.Instance.World.ChunkManager.AddChunk(convertedChunk, vec);
        }

        private void OnCharacterInput(object sender, char c)
	    {
	        if (RenderChatInput)
	        {
	            Input += c;
	        }
	    }

	    public override void Stop()
		{
			base.Stop();
		}

		public override void Render2D(RenderArgs args)
		{
			FpsCounter.Update((float) args.GameTime.ElapsedGameTime.TotalSeconds);

			args.SpriteBatch.Begin();
			args.SpriteBatch.Draw(CrosshairTexture,
				new Vector2(CenterScreen.X - CrosshairTexture.Width/2f, CenterScreen.Y - CrosshairTexture.Height/2f));

		    if (RenderChatInput)
		    {
		        var heightCalc = Alex.Font.MeasureString("!");
		        if (Input.Length > 0)
		        {
		            heightCalc = Alex.Font.MeasureString(Input);
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

                args.SpriteBatch.DrawString(Alex.Font, Input,
                    new Vector2(5, (int)(args.GraphicsDevice.Viewport.Height - (heightCalc.Y + 5))), Color.White);
            }

		    var count = 2;
		    foreach (var msg in ChatMessages.TakeLast(5).Reverse())
		    {
		        var amsg = msg.ToArray()
		            .Where(i => !Alex.Font.Characters.Contains(i))
		            .Aggregate(msg, (current, i) => current.Replace(i.ToString(), ""));
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

		    if (RenderDebug)
			{
				var fpsString = string.Format("Alex {0} ({1} FPS)", Alex.Version, Math.Round(FpsCounter.AverageFramesPerSecond));
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

				positionString = "Looking at: " + SelectedBlock;
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

			if (SelectedBlock.Y > 0 && SelectedBlock.Y < 256)
			{
				var selBlock = Alex.Instance.World.GetBlock(SelectedBlock.X, SelectedBlock.Y, SelectedBlock.Z);
				var boundingBox = new BoundingBox(SelectedBlock + selBlock.BlockModel.Offset,
					SelectedBlock + selBlock.BlockModel.Offset + selBlock.BlockModel.Size);

				args.SpriteBatch.RenderBoundingBox(
					boundingBox,
					Game.MainCamera.ViewMatrix, Game.MainCamera.ProjectionMatrix, Color.LightGray);
			}

			base.Render2D(args);
		}

		public override void Render3D(RenderArgs args)
		{
			Alex.Instance.World.Render();
			base.Render3D(args);
		}

	    private string Input = "";
		private Vector3 SelectedBlock;
		private KeyboardState OldKeyboardState;
	    private MouseState OldMouseState;
		public override void OnUpdate(GameTime gameTime)
		{
			SelectedBlock = RayTracer.Raytrace();
			
			if (Alex.Instance.IsActive)
			{
			    if (!RenderChatInput)
			    {
                    Alex.Instance.UpdateCamera(gameTime);
                }

				Alex.Instance.HandleInput();

			    MouseState currentMouseState = Mouse.GetState();
			    if (currentMouseState != OldMouseState)
			    {
			        if (currentMouseState.LeftButton == ButtonState.Pressed)
			        {
			            if (SelectedBlock.Y > 0 && SelectedBlock.Y < 256)
			            {
			                Alex.Instance.World.SetBlock(SelectedBlock.X, SelectedBlock.Y, SelectedBlock.Z, new Air());
			            }
			        }

			        if (currentMouseState.RightButton == ButtonState.Pressed)
			        {
			            if (SelectedBlock.Y > 0 && SelectedBlock.Y < 256)
			            {
			                Alex.Instance.World.SetBlock(SelectedBlock.X, SelectedBlock.Y + 1, SelectedBlock.Z, new Stone());
			            }
			        }
			    }
			    OldMouseState = currentMouseState;

				KeyboardState currentKeyboardState = Keyboard.GetState();
				if (currentKeyboardState != OldKeyboardState)
				{
					if (currentKeyboardState.IsKeyDown(KeyBinds.Menu))
					{
					    if (RenderChatInput)
					    {
					        RenderChatInput = false;
					    }
					}

					if (currentKeyboardState.IsKeyDown(KeyBinds.DebugInfo))
					{
						RenderDebug = !RenderDebug;
					}

				    if (RenderChatInput) //Handle Input
				    {
				        if (currentKeyboardState.IsKeyDown(Keys.Back))
				        {
				            if (Input.Length > 0) Input = Input.Remove(Input.Length - 1, 1);
				        }

				        if (currentKeyboardState.IsKeyDown(Keys.Enter))
				        {
				            //Submit message
				            if (Input.Length > 0)
				            {
				                if (Alex.IsMultiplayer)
				                {
                                    Client.SendChat(Input);
				                }
				                else
				                {
                                    ChatMessages.Add("<Me> " + Input);
                                }
				            }
                            Input = string.Empty;
                            RenderChatInput = false;
                        }
                    }
				    else
				    {
				        if (currentKeyboardState.IsKeyDown(KeyBinds.Chat))
				        {
				            RenderChatInput = !RenderChatInput;
				            Input = string.Empty;
				        }
				    }
				}
				OldKeyboardState = currentKeyboardState;
			}

			base.OnUpdate(gameTime);
		}
	}
}
