using System;
using Microsoft.Xna.Framework;

namespace RocketUI
{
    public struct RenderTransformation : IEquatable<RenderTransformation>
    {
        public static readonly RenderTransformation None = new RenderTransformation(Vector2.Zero, 0, Vector2.One);

        public Vector2 Origin { get; set; }

        public float Rotation { get; set; }

        public Vector2 Scale { get; set; }

        public RenderTransformation(Vector2 origin, float rotation, Vector2 scale)
        {
            Origin = origin;
            Rotation = rotation;
            Scale = scale;
        }

        

        public static Matrix CreateMatrix(Vector2 position, RenderTransformation transformation)
            => CreateMatrix(position, transformation.Origin, transformation.Rotation, transformation.Scale);
        
        public static Matrix CreateMatrix(Vector2 position, Vector2 origin, float rotation, Vector2 scale)
        {
            var m = Matrix.Identity;
            float cos = 0.0f;
            float sin = 0.0f;
            if ((double) rotation == 0.0)
            {
                m.M11 = scale.X;
                m.M22 = scale.Y;
                m.M41 = origin.X * m.M11 + position.X;
                m.M42 = origin.Y * m.M22 + position.Y;
            }
            else
            {
                cos = (float) Math.Cos((double) rotation);
                sin = (float) Math.Sin((double) rotation);
                m.M11 = scale.X * cos;
                m.M12 = scale.X * sin;
                m.M21 = (float) ((double) scale.Y * -(double) sin);
                m.M22 = scale.Y * cos;
                m.M41 = (float) (origin.X * (double) m.M11 +
                                 origin.Y * (double) m.M21) + position.X;
                m.M42 = (float) (origin.X * (double) m.M12 +
                                 origin.Y * (double) m.M22) + position.Y;
            }

            return m;
        }


        public bool Equals(RenderTransformation other)
        {
            return Origin.Equals(other.Origin) && Rotation.Equals(other.Rotation) && Scale.Equals(other.Scale);
        }

        public override bool Equals(object obj)
        {
            return obj is RenderTransformation other && Equals(other);
        }

        public static bool operator ==(RenderTransformation left, RenderTransformation right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RenderTransformation left, RenderTransformation right)
        {
            return !left.Equals(right);
        }
    }
}