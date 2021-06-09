using Microsoft.Xna.Framework;

namespace Alex.Common.Utils
{
	public interface ITransformation
	{
		Vector3 Translation { get; set; }
		Vector3 Rotation { get; set; }
		Vector3 Scale { get; set; }
	}
}