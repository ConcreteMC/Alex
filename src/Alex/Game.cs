using Alex.Rendering.Camera;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex
{
    public class Game
    {
        internal static bool Initialized;
        internal static Microsoft.Xna.Framework.Game Instance { get; set; }
        internal static Camera MainCamera { get; set; }

        internal static GraphicsDevice GraphicsDevice => Instance.GraphicsDevice;

        internal static RenderDistance RenderDistance = RenderDistance.Normal;

        public static void Initialize(Microsoft.Xna.Framework.Game baseGame)
        {
            Instance = baseGame;
            Initialized = true;
        }

        public static void Init(Vector3 spawnPoint)
        {
            MainCamera = new FirstPersonCamera(spawnPoint, new Vector3(0,0,0));
        }

        public static Camera GetCamera()
        {
            return MainCamera;
        }
    }
}