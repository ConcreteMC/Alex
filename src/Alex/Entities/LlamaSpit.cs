using Alex.Net;
using Alex.Worlds;

namespace Alex.Entities
{
	public class LlamaSpit : Entity
	{
		/// <inheritdoc />
		public LlamaSpit(World level) : base(999, level, null)
		{
			Width = 0.25;
			Height = 0.25;
		}
	}
}