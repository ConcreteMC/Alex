using Alex.Common.Graphics;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Entity
{
	public interface IAttached
	{
		IModel Model { get; set; }

		int Render(IRenderArgs args, Matrix characterMatrix);
		void Update(IUpdateArgs args);
	}
}