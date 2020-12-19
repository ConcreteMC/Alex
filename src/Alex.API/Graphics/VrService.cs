using System;
using Alex.API.Gui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using RocketUI;
using SharpVR;
using Valve.VR;

namespace Alex.Graphics.VR
{
    public class VrService : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly IServiceProvider _serviceProvider;
        private VrContext _context;

        public bool Enabled => _context != null;
        
        private Texture_t[] _textures = new Texture_t[2];
        private VRTextureBounds_t[] _textureBounds = new VRTextureBounds_t[2];
        private Matrix _hmdPose;

        public VrService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Initialize()
        {
            var gdm = _serviceProvider.GetService<GraphicsDeviceManager>();
            var g = _serviceProvider.GetService<Game>();
            g.IsFixedTimeStep = false;
            gdm.SynchronizeWithVerticalRetrace = false;
            gdm.GraphicsDevice.PresentationParameters.PresentationInterval = PresentInterval.Immediate; 
            _context = CreateVrContext();

        }

        private static VrContext CreateVrContext()
        {
            if (!VrContext.CanCallNativeDll(out var error))
            {
                Log.Error(error);
                return null;
            }

            var runtime = VrContext.RuntimeInstalled();
            var hmdConnected = VrContext.HmdConnected();
            Log.Debug($"VR Runtime: {(runtime ? "yes" : "no")}");
            Log.Debug($"VR HMD: {(hmdConnected ? "yes" : "no")}");

            if (!runtime)
            {
                Log.Error("VR Runtime not installed, failed to create VR service...");
                return null;
            }

            if (!hmdConnected)
            {
                Log.Error("No HMD connected, failed to create VR service...");
                return null;
            }

            var vrContext = VrContext.Get();

            Log.Info("Initializing VR Runtime");
            try
            {
                vrContext.Initialize();
            }
            catch (SharpVRException ex)
            {
                if (ex.ErrorCode == 108)
                    Log.Error("No HMD is connected and SteamVR failed to report it...");
                else
                    Log.Error($"Initializing the runtime failed with error {ex.Message}");
                return null;
            }

            return vrContext;
        }


        public RenderTarget2D CreateRenderTargetForEye(Eye eye)
        {
            var eyeNo = (int) eye;
            _context.GetRenderTargetSize(out var width, out var height);

           
            var pp = _graphicsDevice.PresentationParameters;

            var renderTarget = new RenderTarget2D(_graphicsDevice, width, height, false, SurfaceFormat.Color,
                DepthFormat.Depth24Stencil8, pp.MultiSampleCount, RenderTargetUsage.PreserveContents);

            _textures[eyeNo] = new Texture_t();

#if DIRECTX
            var info = typeof(RenderTarget2D).GetField("_msTexture", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var handle = info.GetValue(renderTarget) as SharpDX.Direct3D11.Texture2D;
            _textures[eyeNo].handle = handle.NativePointer;
            _textures[eyeNo].eType = ETextureType.DirectX;
#else
            var info = typeof(RenderTarget2D).GetField("glTexture", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var glTexture = (int)info.GetValue(renderTarget);
            _textures[eyeNo].handle = new System.IntPtr(glTexture);
            _textures[eyeNo].eType = ETextureType.OpenGL;
#endif
            _textures[eyeNo].eColorSpace = EColorSpace.Gamma;
            _textureBounds[eyeNo].uMin = 0;
            _textureBounds[eyeNo].uMax = 1;
            _textureBounds[eyeNo].vMin = 0;
            _textureBounds[eyeNo].vMax = 1;

            return renderTarget;
        }

        private GraphicsDevice _graphicsDevice;
        private RenderTarget2D[] _renderTargets;
        
        public void Init(GraphicsDevice graphicsDevice)
        {
            
            
            _graphicsDevice = graphicsDevice;
            _renderTargets = new RenderTarget2D[2];
            _renderTargets[0] = CreateRenderTargetForEye(Eye.Left);
            _renderTargets[1] = CreateRenderTargetForEye(Eye.Right);

            var guiManager = _serviceProvider.GetService<GuiManager>();
            guiManager.ScaledResolution.ViewportSize = new Size(_renderTargets[0].Width,_renderTargets[0].Height );
        }

        public void Draw(Action doDraw)
        {
            for (int i = 0; i < _renderTargets.Length; i++)
            {
                _graphicsDevice.SetRenderTarget(_renderTargets[i]);
                _graphicsDevice.Clear(Color.Black);
                Eye = (Eye)i;
                doDraw.Invoke();
            }
            _graphicsDevice.SetRenderTarget(null);

            OpenVR.Compositor.Submit(EVREye.Eye_Left, ref _textures[0], ref _textureBounds[0], EVRSubmitFlags.Submit_Default);
            OpenVR.Compositor.Submit(EVREye.Eye_Right, ref _textures[1], ref _textureBounds[1], EVRSubmitFlags.Submit_Default);
        }

        public Matrix GetProjectionMatrix(Eye eye)
        {
            return _context.GetProjectionMatrix(eye, 0.1f, 1000.0f);
        }

        public Matrix GetProjectionMatrix() => GetProjectionMatrix(Eye);

        public Matrix GetViewMatrix(Eye eye, Matrix parent)
        {
            var matrixEyePos = _context.GetEyeMatrix(eye);
            return Matrix.Invert(parent) * (_context.Hmd.GetPose() * matrixEyePos);
        }

        public Matrix GetViewMatrix(Matrix parent) => GetViewMatrix(Eye, parent);

        public Eye Eye { get; private set; }
        
        public void SetActiveEye(Eye eye)
        {
            Eye = eye;
        }
        
        public void Update(GameTime gameTime)
        {
            _context.ProcessEvents();
            _context.WaitGetPoses();
            _context.Update();
        }
        
        public void Dispose()
        {
            _context.Shutdown();
        }
    }
}