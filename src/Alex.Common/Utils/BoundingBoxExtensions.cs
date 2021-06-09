using System;
using Microsoft.Xna.Framework;

namespace Alex.Common.Utils
{
	public static class BoundingBoxExtensions
	{
		/// <summary>
        /// Gets this float value mapped from one range to another.
        /// </summary>
        /// <param name="value">This float.</param>
        /// <param name="min1">The minimum of the source range.</param>
        /// <param name="max1">The maximum of the source range.</param>
        /// <param name="min2">The minimum of the destination range.</param>
        /// <param name="max2">The maximum of the destination range.</param>
        /// <returns></returns>
        public static float Mapped( this float value, float min1, float max1, float min2, float max2 )
        {
            return min2 + ( max2 - min2 ) * ( ( value - min1 ) / ( max1 - min1 ) );
        }

        /// <summary>
        /// Gets this float value clamped to the given range.
        /// </summary>
        /// <param name="value">This float.</param>
        /// <param name="min">The range minimum.</param>
        /// <param name="max">The range maximum.</param>
        /// <returns></returns>
        public static float Clamped( this float value, float min, float max )
        {
            return MathHelper.Clamp( value, min, max );
        }



        /// <summary>
        /// Inflates this bounding box by the given amount.
        /// </summary>
        /// <param name="box">This bounding box.</param>
        /// <param name="amount">The amount to inflate by.</param>
        public static void Inflate( this BoundingBox box, float amount )
        {
            var ha = amount * 0.5f;
            box.Min.X -= ha;
            box.Min.Y -= ha;
            box.Min.Z -= ha;
            box.Max.X += ha;
            box.Max.Y += ha;
            box.Max.Z += ha;
        }
        
        /// <summary>
        /// Inflates this bounding box by the given amount.
        /// </summary>
        /// <param name="box">This bounding box.</param>
        /// <param name="amount">The amount to inflate by.</param>
        public static void Inflate( this BoundingBox box, Vector3 amount )
        {
            var ha = amount * 0.5f;
            
            box.Min.X -= ha.X;
            box.Min.Y -= ha.Y;
            box.Min.Z -= ha.Z;
            box.Max.X += ha.X;
            box.Max.Y += ha.Y;
            box.Max.Z += ha.Z;
        }

        /// <summary>
        /// Gets the center coordinates of this bounding box.
        /// </summary>
        /// <param name="box">The bounding box.</param>
        /// <returns></returns>
        public static Vector3 GetCenter( this BoundingBox box )
        {
            float width = box.Max.X - box.Min.X;
            float height = box.Max.Y - box.Min.Y;
            float depth = box.Max.Z - box.Min.Z;
            return new Vector3(
                box.Min.X + width / 2.0f,
                box.Min.Y + height / 2.0f,
                box.Min.Z + depth / 2.0f
            );
        }

        /// <summary>
        /// Gets the bounding box's width.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <returns></returns>
        public static float GetWidth( this BoundingBox box )
        {
            return Math.Abs( box.Max.X - box.Min.X );
        }

        /// <summary>
        /// Gets the bounding box's height.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <returns></returns>
        public static float GetHeight( this BoundingBox box )
        {
            return Math.Abs( box.Max.Y - box.Min.Y );
        }

        /// <summary>
        /// Gets the bounding box's depth.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <returns></returns>
        public static float GetDepth( this BoundingBox box )
        {
            return Math.Abs( box.Max.Z - box.Min.Z );
        }

        /// <summary>
        /// Gets the dimensions of this bounding box.
        /// </summary>
        /// <param name="box">This bounding box.</param>
        /// <param name="dim">The dimension variable to populate.</param>
        /// <returns></returns>
        public static void GetDimensions( this BoundingBox box, ref Vector3 dim )
        {
            dim.X = box.GetWidth();
            dim.Y = box.GetHeight();
            dim.Z = box.GetDepth();
        }

        /// <summary>
        /// Gets the dimensions of this bounding box.
        /// </summary>
        /// <param name="box">This bounding box.</param>
        /// <returns></returns>
        public static Vector3 GetDimensions( this BoundingBox box )
        {
            Vector3 vec = new Vector3();
            box.GetDimensions( ref vec );
            return vec;
        }
	}
}