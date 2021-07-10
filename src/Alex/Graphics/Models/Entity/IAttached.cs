using Alex.Common.Graphics;
using Alex.Common.Graphics.GpuResources;
using Alex.Gamestates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models.Entity
{
	public interface IAttached
	{
		IAttached Parent { get; set; }
		int Render(IRenderArgs args, Microsoft.Xna.Framework.Graphics.Effect effect, Matrix worldMatrix);
		void Update(IUpdateArgs args);

		string Name { get; }

		void AddChild(IAttached modelBone);
		void Remove(IAttached modelBone);

		IAttached Clone();
	}

	public class AttachedRenderArgs : RenderArgs
	{
		public VertexBuffer Buffer { get; set; }
	}
}