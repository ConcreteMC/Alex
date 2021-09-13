using Alex.Common.Graphics;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Entity
{
	public interface IHoldAttachment
	{
		void AddChild(IAttached modelBone);
		void Remove(IAttached modelBone);
	}
	
	public interface IAttached
	{
		Model Model { get; set; }
		
		IHoldAttachment Parent { get; set; }
		int Render(IRenderArgs args, Matrix characterMatrix);
		void Update(IUpdateArgs args);
	}
}