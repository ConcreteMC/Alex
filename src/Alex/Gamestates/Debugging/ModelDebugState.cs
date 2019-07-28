using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Blocks.State;
using Alex.Entities;
using Alex.GameStates;
using Alex.GameStates.Gui.Common;
using Alex.GameStates.Playing;
using Alex.Graphics.Camera;
using Alex.Graphics.Models.Entity;
using Alex.Gui.Elements;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Alex.Worlds;
using GLib;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RocketUI;
using Color = Microsoft.Xna.Framework.Color;
using DateTime = System.DateTime;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Task = System.Threading.Tasks.Task;

namespace Alex.Gamestates.Debug
{
    public class ModelDebugState : GuiGameStateBase
    {
        private readonly GuiStackMenu _mainMenu;
        private readonly GuiContainer _menuContainer;
        
        private FirstPersonCamera Camera { get; } = new FirstPersonCamera(16, Vector3.Zero, Vector3.Zero);

        private AlphaTestEffect _alphaEffect = null;
        private BasicEffect _basicEffect = null;
        private Effect _currentEffect;
        
        
        private readonly GuiDebugInfo _debugInfo;
        private long _ramUsage = 0;
        private long _threadsUsed, _maxThreads, _complPortUsed, _maxComplPorts;
        
        private World World { get; }
        public ModelDebugState()
        {
            World = new World(Alex, Alex.GraphicsDevice, Alex.Services.GetService<IOptionsProvider>().AlexOptions,
                Camera, new DebugNetworkProvider());
            _debugInfo = new GuiDebugInfo();
            
            Background = new Color(Color.DeepSkyBlue.ToVector3());
            
            _menuContainer = new GuiContainer()
            {
                Margin = new Thickness(0, 0, 15, 0),
                Padding = new Thickness(0, 50, 0, 0),
                Width = 125,
                Anchor = Alignment.FillY | Alignment.MinX,

                //ChildAnchor = Alignment.CenterY | Alignment.FillX,
                BackgroundOverlay = new Color(Color.Black, 0.35f),
                //Orientation = Orientation.Vertical
            };

            _mainMenu = new GuiStackMenu()
            {
                Margin = new Thickness(0, 0, 15, 0),
                Padding = new Thickness(0, 50, 0, 0),
                Width = 125,
                Anchor = Alignment.FillY | Alignment.MinX,

                ChildAnchor = Alignment.CenterY | Alignment.FillX,
                BackgroundOverlay = new Color(Color.Black, 0.35f)
            };
            
            _mainMenu.AddMenuItem("Skip", () => { Task.Run(() =>
            {
                ModelExplorer.Skip();
            }); });
            _mainMenu.AddMenuItem("Next", NextModel);
            _mainMenu.AddMenuItem("Previous", PrevModel);
            _mainMenu.AddMenuItem("Switch Models", SwitchModelExplorers);
            
            AddChild(_mainMenu);

            _debugInfo.AddDebugRight(() => Alex.DotnetRuntime);
            //_debugInfo.AddDebugRight(() => MemoryUsageDisplay);
            _debugInfo.AddDebugRight(() => $"RAM: {PlayingState.GetBytesReadable(_ramUsage, 2)}");
            _debugInfo.AddDebugRight(() => $"GPU: {PlayingState.GetBytesReadable(GpuResourceManager.GetMemoryUsage, 2)}");
            _debugInfo.AddDebugRight(() =>
            {
                return
                    $"Threads: {(_threadsUsed):00}/{_maxThreads}\nCompl.ports: {_complPortUsed:00}/{_maxComplPorts}";
            });
            
            _debugInfo.AddDebugRight(() =>
            {
                if (ModelExplorer == null)
                    return string.Empty;
                
                return ModelExplorer.GetDebugInfo();
            });

            _keyState = Keyboard.GetState();
            
           ModelExplorer = BlockModelExplorer = new BlockModelExplorer(Alex, World);
           EntityModelExplorer = new EntityModelExplorer(Alex, World);
        }

        private BlockModelExplorer BlockModelExplorer { get; }
        private EntityModelExplorer EntityModelExplorer { get; }
        private void SwitchModelExplorers()
        {
            if (ModelExplorer == BlockModelExplorer)
            {
                ModelExplorer = EntityModelExplorer;
            }
            else if (ModelExplorer == EntityModelExplorer)
            {
                ModelExplorer = BlockModelExplorer;
            }
        }

        protected override void OnShow()
        {
            base.OnShow();
            Alex.GuiManager.AddScreen(_debugInfo);

            Alex.IsMouseVisible = true;
        }

        protected override void OnHide()
        {
            base.OnHide();
            Alex.GuiManager.RemoveScreen(_debugInfo);
            
            Alex.IsMouseVisible = false;
        }

        private void PrevModel()
        {
            ModelExplorer.Previous();
        }

        private void NextModel()
        {
            ModelExplorer.Next();
        }

        private Rectangle GetBounds()
        {
            return new Rectangle(_menuContainer.RenderBounds.Right, 0, RenderBounds.Width - _menuContainer.RenderBounds.Right, RenderBounds.Height);   
        }
        
        private Rectangle _previousBounds;
        private static Vector3 _rotationCenter = Vector3.One / 2f;
        private DateTime _previousMemUpdate = DateTime.UtcNow;
        private int _previousIndex = -1;
        
        private Vector3 _rotation = Vector3.Zero;
        private KeyboardState _keyState = default;
        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            /*_rotation += (float)gameTime.ElapsedGameTime.TotalSeconds;*/

            var world = Matrix.CreateTranslation(-_rotationCenter) * Matrix.CreateRotationX(MathHelper.ToRadians(_rotation.X)) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(_rotation.Y)) *
                        Matrix.CreateRotationZ(MathHelper.ToRadians(_rotation.Z)) * Matrix.CreateTranslation(_rotationCenter) * Matrix.CreateTranslation(Vector3.Backward * 6);
            
            ModelExplorer?.SetRotation(_rotation);
            
            if (_basicEffect == null)
            {
                _basicEffect = new BasicEffect(Alex.GraphicsDevice);
                _basicEffect.VertexColorEnabled = true;
                _basicEffect.TextureEnabled = true;
            }
            
            if (_alphaEffect == null)
            {
                _alphaEffect = new AlphaTestEffect(Alex.GraphicsDevice);
                _alphaEffect.VertexColorEnabled = true;
            }
            
            _alphaEffect.Projection = _basicEffect.Projection = Camera.ProjectionMatrix;
            _alphaEffect.View = _basicEffect.View = Camera.ViewMatrix;
            _alphaEffect.World  = _basicEffect.World = world;

            var bounds = Screen.RenderBounds;

            if (bounds != _previousBounds)
            {
                Camera.UpdateAspectRatio(bounds.Width / (float)bounds.Height);
                _previousBounds = bounds;
            }

            var now = DateTime.UtcNow;
            if (now - _previousMemUpdate > TimeSpan.FromSeconds(5))
            {
                _previousMemUpdate = now;

                //Task.Run(() =>
                {
                    _ramUsage = Environment.WorkingSet;

                    ThreadPool.GetMaxThreads(out int maxThreads, out int maxCompletionPorts);
                    ThreadPool.GetAvailableThreads(out int availableThreads, out int availableComplPorts);
                    _threadsUsed = maxThreads - availableThreads;
                    _complPortUsed = maxCompletionPorts - availableComplPorts;

                    _maxThreads = maxThreads;
                    _maxComplPorts = maxCompletionPorts;
                }//);
            }


            var keyState = Keyboard.GetState();

           // if (keyState != _keyState)
            {
                if (keyState.IsKeyDown(Keys.W) )
                {
                    _rotation.X += (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
                else if (keyState.IsKeyDown(Keys.S))
                {
                    _rotation.X -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
                
                if (keyState.IsKeyDown(Keys.A))
                {
                    _rotation.Y += (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
                else if (keyState.IsKeyDown(Keys.D))
                {
                    _rotation.Y-= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                if (keyState.IsKeyUp(Keys.R) && _keyState.IsKeyDown(Keys.R))
                {
                    _rotation = Vector3.Zero;
                }
            }
            
            _keyState = keyState;
            /*var updateArgs = new UpdateArgs()
            {
                GraphicsDevice = Alex.Instance.GraphicsDevice,
                GameTime       = gameTime,
                Camera         = Camera
            };*/
            
            Camera.UpdateProjectionMatrix();
            
            ModelExplorer.Update(new UpdateArgs()
            {
                Camera = Camera,
                GameTime = gameTime,
                GraphicsDevice = Alex.GraphicsDevice
            }, _alphaEffect, _basicEffect, out _currentEffect);

             Camera.MoveTo(Vector3.Zero, Vector3.Zero);
            
        }

        private ModelExplorer ModelExplorer { get; set; }
        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            base.OnDraw(graphics, gameTime);

            var bounds = RenderBounds;// GetBounds();
                
            bounds.Inflate(-3, -3);

            var p = graphics.Project(bounds.Location.ToVector2());
            var p2 = graphics.Project(bounds.Location.ToVector2() + bounds.Size.ToVector2());
                
            var newViewport = new Viewport();
            newViewport.X      = (int)p.X;
            newViewport.Y      = (int)p.Y;
            newViewport.Width  = (int) (p2.X - p.X);
            newViewport.Height = (int) (p2.Y - p.Y);

            //context.Viewport = newViewport;

            using (var context = graphics.BranchContext(BlendState.AlphaBlend, DepthStencilState.Default,
                RasterizerState.CullClockwise, SamplerState.PointWrap))
            {
                context.Viewport = newViewport;
                //graphics.Begin();

                if (_currentEffect != null)
                {
                    foreach (var pass in _currentEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        ModelExplorer.Render(context, new RenderArgs()
                        {
                            Camera = Camera,
                            GameTime = gameTime,
                            GraphicsDevice = context.GraphicsDevice,
                            SpriteBatch = graphics.SpriteBatch
                        });
                    }
                }
                else
                {
                    ModelExplorer.Render(context, new RenderArgs()
                    {
                        Camera = Camera,
                        GameTime = gameTime,
                        GraphicsDevice =  context.GraphicsDevice,
                        SpriteBatch = graphics.SpriteBatch
                    });
                }

               // graphics.End();
            }
        }
    }

    public class EntityModelExplorer : ModelExplorer
    {
        private KeyValuePair<string, BedrockResourcePack.EntityDefinition>[] _entityDefinitions;
        private int _index = 0;
        
        private GraphicsDevice GraphicsDevice { get; }
        private Alex Alex { get; }
        private World World { get; }
        public EntityModelExplorer(Alex alex, World world)
        {
            Alex = alex;
            World = world;
            GraphicsDevice = alex.GraphicsDevice;

            _entityDefinitions = alex.Resources.BedrockResourcePack.EntityDefinitions.ToArray();
        }

        private EntityModelRenderer _currentRenderer = null;

        private void SetVertices()
        {
            var def = _entityDefinitions[_index];

            EntityModelRenderer renderer = null;

            if (def.Value != null && def.Value.Geometry != null && def.Value.Geometry.ContainsKey("default") &&
                def.Value.Textures != null)
            {
                EntityModel model;
                if (ModelFactory.TryGetModel(def.Value.Geometry["default"],
                        out model) && model != null)
                {
                    var textures = def.Value.Textures;
                    string texture;
                    if (!textures.TryGetValue("default", out texture))
                    {
                        texture = textures.FirstOrDefault().Value;
                    }

                    if (Alex.Resources.BedrockResourcePack.Textures.TryGetValue(texture,
                        out Bitmap bmp))
                    {
                        Texture2D t = TextureUtils.BitmapToTexture2D(Alex.GraphicsDevice, bmp);

                        renderer = new EntityModelRenderer(model, t);
                    }
                }
            }

            _currentRenderer = renderer;
        }

        public override void Next()
        {
            if (_index + 1 >= _entityDefinitions.Length)
            {
                _index = 0;
            }
            else
            {
                _index++;
            }
            
            SetVertices();
        }

        public override void Previous()
        {
            if (_index - 1 < 0)
            {
                _index = _entityDefinitions.Length - 1;
            }
            else
            {
                _index--;
            }
            
            SetVertices();
        }

        public override void Skip()
        {
            return;
            int start = _index;
            var currentState = _entityDefinitions[start];
                
            for (int i = start; i < _entityDefinitions.Length; i++)
            {
                var state = _entityDefinitions[i];
                /*if (!string.Equals(currentState.Name, state.Name))
                {
                    _index = i;
                    SetVertices();
                    break;
                }*/
            }
        }
        
        private PlayerLocation Location { get; } = new PlayerLocation(Vector3.Backward * 3);

        public override void SetRotation(Vector3 rotation)
        {
            Location.Yaw = Location.HeadYaw = MathUtils.RadianToDegree(rotation.Y);
            Location.Pitch = MathUtils.RadianToDegree(rotation.Z);
        }
        
        public override void Render(GraphicsContext context, RenderArgs renderArgs)
        {
            var renderer = _currentRenderer;
            if (renderer == null)
                return;

            renderer?.Render(renderArgs, Location);   
        }

        private int _previousIndex = -1;
        public override void Update(UpdateArgs args, AlphaTestEffect alphaEffect, BasicEffect basicEffect, out Effect currentEffect)
        {
            var block = _entityDefinitions[_index];
            currentEffect = null;
            
            /*if (_index != _previousIndex)
            {
                if (block.Block.Animated)
                {
                    alphaEffect.Texture = Alex.Resources.Atlas.GetAtlas(0);
                    basicEffect.Texture = Alex.Resources.Atlas.GetAtlas(0);
                }
                else
                {
                    alphaEffect.Texture = Alex.Resources.Atlas.GetStillAtlas();
                    basicEffect.Texture = Alex.Resources.Atlas.GetStillAtlas();
                }

                _previousIndex = _index;
            }
            
            currentEffect = (block.Block.Transparent || block.Block.Animated) ? (Effect) alphaEffect : basicEffect;*/
            _currentRenderer?.Update(args, Location);
        }

        public override string GetDebugInfo()
        {
            var block = _entityDefinitions[_index];
                
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{block.Key}");

            return sb.ToString();
        }
    }

    public class BlockModelExplorer : ModelExplorer
    {
        private IBlockState[] _blockStates;
        private int _index = 0;
        
        private GraphicsDevice GraphicsDevice { get; }
        private Alex Alex { get; }
        private World World { get; }
        public BlockModelExplorer(Alex alex, World world)
        {
            Alex = alex;
            World = world;
            GraphicsDevice = alex.GraphicsDevice;
            
            _blockStates = BlockFactory.AllBlockstates.Values.ToArray();
        }
        private VertexPositionNormalTextureColor[] _vertices = null;
        private int[] _indices = null;
        private bool _canRender = false;
        
        private PooledVertexBuffer _buffer = null;
        private PooledIndexBuffer _indexBuffer = null;
        
        private void SetVertices()
        {
            var b = _blockStates[_index];
            
            var vertices = b.Model
                .GetVertices(World
                    , Vector3.Zero, _blockStates[_index].Block);

            if (vertices.vertices.Length > 0 && vertices.indexes.Length > 0)
            {
                var oldBuffer = _buffer;
                var oldIndexBuffer = _indexBuffer;
                
                var newBuffer = GpuResourceManager.GetBuffer(this, Alex.GraphicsDevice,
                    VertexPositionNormalTextureColor.VertexDeclaration, vertices.vertices.Length, BufferUsage.None);

                var newIndexBuffer = GpuResourceManager.GetIndexBuffer(this, Alex.GraphicsDevice,
                    IndexElementSize.ThirtyTwoBits, vertices.indexes.Length, BufferUsage.None);

                newBuffer.SetData(vertices.vertices);

                newIndexBuffer.SetData(vertices.indexes);

                _buffer = newBuffer;
                _indexBuffer = newIndexBuffer;

                oldIndexBuffer?.MarkForDisposal();
                oldBuffer?.MarkForDisposal();
                
                GraphicsDevice.Indices = _indexBuffer;
                GraphicsDevice.SetVertexBuffer(_buffer);
                
                _canRender = true;
            }

            // _vertices = vertices.vertices;
            // _indices = vertices.indexes;
        }
        
        public override void Next()
        {
            _canRender = false;
            if (_index + 1 >= _blockStates.Length)
            {
                _index = 0;
            }
            else
            {
                _index++;
            }
            
            SetVertices();
        }

        public override void Previous()
        {
            _canRender = false;
            
            if (_index - 1 < 0)
            {
                _index = _blockStates.Length - 1;
            }
            else
            {
                _index--;
            }
            
            SetVertices();
        }

        public override void Skip()
        {
            int start = _index;
            var currentState = _blockStates[start];
                
            for (int i = start; i < _blockStates.Length; i++)
            {
                var state = _blockStates[i];
                if (!string.Equals(currentState.Name, state.Name))
                {
                    _index = i;
                    SetVertices();
                    break;
                }
            }
        }

        public override void Render(GraphicsContext context, RenderArgs renderArgs)
        {
            if (!_canRender)
                return;
            
            context.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                _indexBuffer.IndexCount / 3);

        }

        private int _previousIndex = -1;
        public override void Update(UpdateArgs args, AlphaTestEffect alphaEffect, BasicEffect basicEffect, out Effect currentEffect)
        {
            var block = _blockStates[_index];
            
            if (_index != _previousIndex)
            {
                if (block.Block.Animated)
                {
                    alphaEffect.Texture = Alex.Resources.Atlas.GetAtlas(0);
                    basicEffect.Texture = Alex.Resources.Atlas.GetAtlas(0);
                }
                else
                {
                    alphaEffect.Texture = Alex.Resources.Atlas.GetStillAtlas();
                    basicEffect.Texture = Alex.Resources.Atlas.GetStillAtlas();
                }

                _previousIndex = _index;
            }
            
            currentEffect = (block.Block.Transparent || block.Block.Animated) ? (Effect) alphaEffect : basicEffect;
        }

        public override string GetDebugInfo()
        {
            var block = _blockStates[_index];
                
            StringBuilder sb = new StringBuilder();

            if (block != null)
            {
                sb.AppendLine($"{block.Name}");
                    
                if (block is BlockState s && s.IsMultiPart)
                {
                    sb.AppendLine($"MultiPart=true");
                }

                var dict = block.ToDictionary();
                foreach (var kv in dict)
                {
                    sb.AppendLine($"{kv.Key}={kv.Value}");
                }
            }

            return sb.ToString();
        }
    }

    public abstract class ModelExplorer
    {
        public abstract void Next();
        public abstract void Previous();
        public abstract void Skip();
        public abstract void Render(GraphicsContext context, RenderArgs renderArgs);
        public abstract void Update(UpdateArgs args, AlphaTestEffect alphaEffect, BasicEffect basicEffect, out Effect currentEffect);
        public abstract string GetDebugInfo();

        public virtual void SetRotation(Vector3 rotation)
        {
            
        }
    }
}