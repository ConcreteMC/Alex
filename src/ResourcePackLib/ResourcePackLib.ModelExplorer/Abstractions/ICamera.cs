using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ResourcePackLib.ModelExplorer.Abstractions
{
    public interface ICamera : IUpdateable
    {
        public Vector3 Up { get; }
        public Vector3 Right { get; }
        public Vector3 Forward { get; }
        public Viewport Viewport { get; }
        
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        
        public Matrix View { get; }
        public Matrix Projection { get; }
        
        public float NearDistance { get; }
        public float FarDistance { get; }

        public void Draw(Action doDraw);
        public void MoveRelative(Vector3 move);

    }
}