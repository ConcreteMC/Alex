namespace Alex.Gamestates.Debugging
{
	/*public class DebugModelRenderer : GuiContext3DElement
	{
	    //public ModelExplorer ModelExplorer { get; set; }
	    // private World World { get; }
	    private BlockModelExplorer BlockModelExplorer { get; }
	    private EntityModelExplorer EntityModelExplorer { get; }


	    public DebugModelRenderer(Alex alex)
	    {
	        Background = Color.DeepSkyBlue;
	        Anchor = Alignment.Fill;
	        ClipToBounds = false;

	        //Position = new PlayerLocation(Vector3.Zero);

	        // World = new World(alex, alex.GraphicsDevice, alex.Services.GetService<IOptionsProvider>().AlexOptions,
	       //     new FirstPersonCamera(16, Vector3.Zero, Vector3.Zero), new DebugNetworkProvider());
	       
	        BlockModelExplorer = new BlockModelExplorer(alex, null);
	        EntityModelExplorer = new EntityModelExplorer(alex, null);

	        Drawable = BlockModelExplorer;
	        
	    }

	    public void SwitchModelExplorers()
	    {
	        if (Drawable == BlockModelExplorer)
	        {
	            Drawable = EntityModelExplorer;
	        }
	        else if (Drawable == EntityModelExplorer)
	        {
	            Drawable = BlockModelExplorer;
	        }
	    }
	    
	    /*private Rectangle _previousBounds;
	    protected override void OnUpdate(GameTime gameTime)
	    {
	        base.OnUpdate(gameTime);
	        
	        var bounds = Alex.Instance.GraphicsDevice.PresentationParameters.Bounds;

	        if (bounds != _previousBounds)
	        {
	            //var c = bounds.Center;
	            //Camera.RenderPosition = new Vector3(c.X, c.Y, 0.0f);
	            //Camera.UpdateProjectionMatrix();
	            //Camera.UpdateAspectRatio((float) bounds.Width / (float) bounds.Height);


	            //var tl = GuiRenderer.Project(bounds.TopLeft());
	            //var br = GuiRenderer.Project(bounds.BottomRight());

	            //bounds = new Rectangle((int)tl.X, (int)tl.Y, (int)(br.X - tl.X), (int)(br.Y - tl.Y));

	            //var viewPort = Camera.Viewport;
	            
	            //viewPort.Width = bounds.Width;
	            //viewPort.Height = bounds.Height;
	            //viewPort.X = bounds.X;
	            //viewPort.Y = bounds.Y;
	            //viewPort.MinDepth = 0.01f;
	            //viewPort.MaxDepth = 128f;
	            //viewPort.Bounds = bounds;

	            //Camera.Viewport = viewPort;
	            //Camera.UpdateAspectRatio((float)bounds.Width / (float)bounds.Height);
	            _previousBounds = bounds;
	        }

	        var updateArgs = new UpdateArgs()
	        {
	            GraphicsDevice = Alex.Instance.GraphicsDevice,
	            GameTime       = gameTime,
	            Camera         = Camera
	        };

	        ModelExplorer.SetLocation(EntityPosition);
	        ModelExplorer?.Update(updateArgs);
	        Camera.Update(updateArgs);

	        //Entity.Update(updateArgs);
	        //EntityModelRenderer?.Update(updateArgs, EntityPosition);
	    }*/

	/*protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
	{
	    base.OnDraw(graphics, gameTime);

	    var renderArgs = new RenderArgs()
	    {
	        GraphicsDevice = graphics.SpriteBatch.GraphicsDevice,
	        SpriteBatch    = graphics.SpriteBatch,
	        GameTime       = gameTime,
	        Camera         = Camera,
	    };
	    //ClipToBounds = false;
	    //graphics.End();
	    //using (var context = graphics.BranchContext(BlendState.AlphaBlend, DepthStencilState.Default,RasterizerState.CullNone, SamplerState.PointWrap))
	    {

	        //var oldViewport = renderArgs.GraphicsDevice.Viewport;
	        var oldBlendState = renderArgs.GraphicsDevice.BlendState;
	        var oldDepthStencilState = renderArgs.GraphicsDevice.DepthStencilState;
	        var oldRasterizerState = renderArgs.GraphicsDevice.RasterizerState;
	        var oldSamplerState = renderArgs.GraphicsDevice.SamplerStates[0];

	        //renderArgs.GraphicsDevice.Viewport = Camera.Viewport;
	        renderArgs.GraphicsDevice.BlendFactor = Color.TransparentBlack;
	        renderArgs.GraphicsDevice.BlendState = BlendState.AlphaBlend;
	        renderArgs.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
	        renderArgs.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
	        renderArgs.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
	        //renderArgs.GraphicsDevice.ScissorRectangle = Camera.Viewport.Bounds;
	        
	        //var bounds = RenderBounds;
	        
	        //bounds.Inflate(-3, -3);

	        //var p = graphics.Project(bounds.Location.ToVector2());
	        //var p2 = graphics.Project(bounds.Location.ToVector2() + bounds.Size.ToVector2());
	            
	        //var newViewport = Camera.Viewport;
	        //newViewport.X      = (int)p.X;
	        //newViewport.Y      = (int)p.Y;
	        //newViewport.Width  = (int) (p2.X - p.X);
	        //newViewport.Height = (int) (p2.Y - p.Y);

	        //Camera.Viewport    = newViewport;
	        Camera.UpdateProjectionMatrix();

	        //context.Viewport = Camera.Viewport;

	       // graphics.Begin();

	        ModelExplorer.Render(graphics.Context, renderArgs);


	        //renderArgs.GraphicsDevice.Viewport = oldViewport;
	        renderArgs.GraphicsDevice.BlendState = oldBlendState;
	        renderArgs.GraphicsDevice.DepthStencilState = oldDepthStencilState;
	        renderArgs.GraphicsDevice.RasterizerState = oldRasterizerState;
	        renderArgs.GraphicsDevice.SamplerStates[0] = oldSamplerState;


	        //   graphics.End();
	    }
	    //graphics.Begin();
	}*/

	//      public PlayerLocation EntityPosition { get; set; } = new PlayerLocation(Vector3.Zero);
	//     public Vector3 CameraRotation { get; set; } = Vector3.Zero;

	/*class DebugModelRendererCamera : Camera
	{
	    private readonly DebugModelRenderer _modelView;
	    private Viewport _viewport;

	    public Viewport Viewport
	    {
	        get => _viewport;
	        set
	        {
	            _viewport = value;
	            UpdateAspectRatio(value.AspectRatio);
	        }
	    }

	    public Vector3 EntityPositionOffset { get; set; } = new Vector3(0f, 1.85f, 16f);

	    public DebugModelRendererCamera(DebugModelRenderer guiEntityModelView) : base()
	    {
	        _modelView = guiEntityModelView;
	        Viewport = new Viewport(256, 128, 128, 256, 0.1f, 128.0f);
	        Position = _modelView.EntityPosition;
	        Rotation = Vector3.Zero;
	        
	        FOV = 75f;

	        //UpdateAspectRatio(Viewport.AspectRatio);
	    }

	    protected override void UpdateViewMatrix()
	    {
	        ViewMatrix = MCMatrix.CreateLookAt(Position + EntityPositionOffset, Position, Vector3.Up);
	        
	      //  Matrix rotationMatrix = Matrix.CreateRotationX(-Rotation.Z) * //Pitch
	      //                          Matrix.CreateRotationY(-Rotation.Y); //Yaw

	      //  Vector3 lookAtOffset = Vector3.Transform(Vector3.Backward, rotationMatrix);
	      //  Direction = lookAtOffset;

	    //    var pos = Position + Vector3.Transform(Offset, Matrix.CreateRotationY(-Rotation.Y));
		
	     //   Target = pos + lookAtOffset;
	      //  ViewMatrix = MCMatrix.CreateLookAt(Position, Target, Vector3.Up);

	        //    Matrix rotationMatrix = (Matrix.CreateRotationX(Rotation.X) *
	        //                            Matrix.CreateRotationY(Rotation.Y));

	        //    Vector3 lookAtOffset = Vector3.Transform(EntityPositionOffset, rotationMatrix);

	        //    Target = Position;

	        //    Direction = Vector3.Transform(Vector3.Forward, rotationMatrix);

	        //    ViewMatrix = Matrix.CreateLookAt(Target + lookAtOffset, Target, Vector3.Up);
	    }


	   // public override void UpdateProjectionMatrix()
	    //{
	    //    ProjectionMatrix = MCMatrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FOV), Viewport.AspectRatio,
	    //                                                           Viewport.MinDepth, Viewport.MaxDepth);
	  //  }

	    //public override void UpdateProjectionMatrix()
	    //{
	    //    var bounds = _modelView.RenderBounds;
	    //    ProjectionMatrix =
	    //        Matrix.CreatePerspective(Viewport.Width, Viewport.Height, Viewport.MinDepth, Viewport.MaxDepth);
	    //    //ProjectionMatrix = Matrix.CreateOrthographicOffCenter(Viewport.RenderBounds, NearDistance, FarDistance);// * Matrix.CreateTranslation(new Vector3(bounds.Location.ToVector2(), -1f));
	    //    //ProjectionMatrix = Matrix.CreatePerspectiveOffCenter(_modelView.RenderBounds, NearDistance, FarDistance);
	    //}
	}*
}*/
}