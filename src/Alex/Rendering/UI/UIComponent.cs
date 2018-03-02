using Alex.Gamestates;
using Microsoft.Xna.Framework;

namespace Alex.Rendering.UI
{
	public class UIComponent
	{
		public Vector2 Location { get; set; }
		public Vector2 Size { get; set; }
		public UIComponent()
		{
			Location = Vector2.Zero;
			Size = Vector2.Zero;
		}

		public virtual void Render(RenderArgs args)
		{

		}

		public virtual void Update(GameTime time)
		{

		}

		public virtual void OnWindowResize()
		{

		}
	}
}
