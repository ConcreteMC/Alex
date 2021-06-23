using Alex.MoLang.Attributes;
using Alex.Worlds;

namespace Alex.Entities.Projectiles
{
	public class XpOrbEntity : ThrowableEntity
	{
		/// <inheritdoc />
		public XpOrbEntity(World level) : base(level) { }

		[MoProperty("texture_frame_index")] public int TextureFrameIndex { get; set; } = 0;
	}
}