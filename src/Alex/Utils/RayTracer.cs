using Alex.Blocks.Minecraft;
using Alex.Graphics.Camera;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Utils
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
				projectionMatrix, viewMatrix, Matrix.Identity);
			var farPoint = graphics.Viewport.Unproject(farsource,
				projectionMatrix, viewMatrix, Matrix.Identity);

			var direction = farPoint - nearPoint;
			direction.Normalize();

			var plotter = new PlotCell3f(new Vector3(1, 1, 1));

			plotter.Plot(camera.Position, direction, 5 * 2);

			while (plotter.Next())
			{
				var v = plotter.Get();
				var b = (Block)world.GetBlock(v);
				if (b != null && b.Renderable && b.HasHitbox && b.GetBoundingBox(v.Floor()).Intersects(new BoundingBox(v, v)))
				{
					plotter.End();

					return v;
				}
			}
			return new Vector3(0, -255, 0);
		}

		public static bool CanSee(GraphicsDevice graphics, World world, Camera camera, Vector3 target)
		{
			var projectionMatrix = camera.ProjectionMatrix;
			var viewMatrix = camera.ViewMatrix;

			var nearsource = new Vector3(graphics.Viewport.Width / 2f,
				graphics.Viewport.Height / 2f, 0f);
			var farsource = new Vector3(graphics.Viewport.Width / 2f,
				graphics.Viewport.Height / 2f, 1f);

			var nearPoint = graphics.Viewport.Unproject(nearsource,
				projectionMatrix, viewMatrix, Matrix.Identity);
			var farPoint = graphics.Viewport.Unproject(farsource,
				projectionMatrix, viewMatrix, Matrix.Identity);

			var direction = farPoint - nearPoint;
			direction.Normalize();

			var plotter = new PlotCell3f(new Vector3(1, 1, 1));

			var myPosition = camera.Position;

			var d = (target - myPosition);
			d.Normalize();

			plotter.Plot(myPosition, d, Alex.Instance.GameSettings.RenderDistance^2);
			
			while (plotter.Next())
			{
				var v = plotter.Get();

				var b = (Block)world.GetBlock(v);
				if (b != null && b.Renderable && b.HasHitbox && b.GetBoundingBox(v.Floor()).Intersects(new BoundingBox(v, v)))
				{
					plotter.End();

					return false;
				}

				if (Vector3.DistanceSquared(v, target) < 1f)
				{
					return true;
				}
			}

			return false;
		}
	}
}
