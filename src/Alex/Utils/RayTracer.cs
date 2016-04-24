using Microsoft.Xna.Framework;

namespace Alex.Utils
{
	public static class RayTracer
	{
		public static Vector3 Raytrace()
		{
			var nearsource = new Vector3(Alex.Instance.GraphicsDevice.Viewport.Width / 2f,
				Alex.Instance.GraphicsDevice.Viewport.Height / 2f, 0f);
			var farsource = new Vector3(Alex.Instance.GraphicsDevice.Viewport.Width / 2f,
				Alex.Instance.GraphicsDevice.Viewport.Height / 2f, 1f);

			var nearPoint = Alex.Instance.GraphicsDevice.Viewport.Unproject(nearsource,
				Game.MainCamera.ProjectionMatrix, Game.MainCamera.ViewMatrix, Matrix.CreateTranslation(0, 0, 0));
			var farPoint = Alex.Instance.GraphicsDevice.Viewport.Unproject(farsource,
				Game.MainCamera.ProjectionMatrix, Game.MainCamera.ViewMatrix, Matrix.CreateTranslation(0, 0, 0));

			var direction = farPoint - nearPoint;
			direction.Normalize();

			var plotter = new PlotCell3f(Alex.Instance.World, new Vector3(0, 0, 0), new Vector3(1, 1, 1));

			plotter.Plot(Game.MainCamera.Position, direction, 5 * 2);

			while (plotter.Next())
			{
				var actual = plotter.Actual();
				var v = plotter.Get();
				var b = Alex.Instance.World.GetBlock(v.X, v.Y, v.Z);
				if (b != null && b.BlockId != 0)
				{
					plotter.End();

					return v;
				}
			}
			return new Vector3(0, -255, 0);
		}
	}
}
