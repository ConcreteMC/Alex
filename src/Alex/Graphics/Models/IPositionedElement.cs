using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models
{
	public interface IPositionedElement
	{
		Vector3 Origin { get; }
		Vector3 Rotation { get; }
	}
}