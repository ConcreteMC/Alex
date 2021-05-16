using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Utils
{
	public static class GraphicsDeviceExtensions
	{
		public static void DrawLine(this GraphicsDevice device, Vector3 start, Vector3 end, Color color)
		{
			var vertices = new[] {new VertexPositionColor(start, color), new VertexPositionColor(end, color)};
			device.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
		}

		public static void DrawGizmo(this GraphicsDevice device, Vector3 position, Vector3 forward, Vector3 up, Vector3 right)
		{
			device.DrawLine(position, position + (forward * 3), Color.Blue);
			device.DrawLine(position, position + (right * 3), Color.Red);
			device.DrawLine(position, position + (up * 3), Color.LimeGreen);
		}
	}
}