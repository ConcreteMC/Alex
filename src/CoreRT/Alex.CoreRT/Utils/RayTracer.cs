using Alex.CoreRT.Blocks;
using Alex.CoreRT.Rendering.Camera;
using Alex.CoreRT.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.CoreRT.Utils
{
	public static class RayTracer
	{
		public static Vector3 Raytrace(GraphicsDevice graphics, World world, Camera camera)
		{
		    var projectionMatrix = camera.ProjectionMatrix;
		    var viewMatrix = camera.ViewMatrix;

			var nearsource = new Vector3(graphics.Viewport.Width / 2f,
				graphics.Viewport.Height / 2f, 0f);
			var farsource = new Vector3(graphics.Viewport.Width / 2f,
				graphics.Viewport.Height / 2f, 1f);

			var nearPoint = graphics.Viewport.Unproject(nearsource,
				projectionMatrix, viewMatrix, Matrix.CreateTranslation(0, 0, 0));
			var farPoint = graphics.Viewport.Unproject(farsource,
				projectionMatrix, viewMatrix, Matrix.CreateTranslation(0, 0, 0));

			var direction = farPoint - nearPoint;
			direction.Normalize();

			var plotter = new PlotCell3f(world, new Vector3(0, 0, 0), new Vector3(1, 1, 1));

			plotter.Plot(camera.Position, direction, 5 * 2);

			while (plotter.Next())
			{
				//var actual = plotter.Actual();
				var v = plotter.Get();
				var b = (Block)world.GetBlock(v);
				if (b != null && b.Solid && b.HasHitbox && b.GetBoundingBox(v.Floor()).Intersects(new BoundingBox(v, v)))
				{
					plotter.End();

					return v;
				}
			}
			return new Vector3(0, -255, 0);
		}
	}
}
