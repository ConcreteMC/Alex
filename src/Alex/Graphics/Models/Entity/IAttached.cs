using Alex.Common.Graphics;
using Alex.Common.Graphics.GpuResources;
using Alex.Gamestates;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Entity
{
	public interface IAttached
	{
		IAttached Parent { get; set; }
		int Render(IRenderArgs args, Microsoft.Xna.Framework.Graphics.Effect effect, Matrix worldMatrix);
		void Update(IUpdateArgs args, Vector3 parentScale);

		string Name { get; }

		void AddChild(IAttached modelBone);
		void Remove(IAttached modelBone);

		IAttached Clone();
	}

	public class AttachedRenderArgs : RenderArgs
	{
		public ManagedVertexBuffer Buffer { get; set; }
	}
}