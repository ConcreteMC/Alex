using Alex.ResourcePackLib.IO.Abstract;

namespace Alex.ResourcePackLib.Bedrock
{
	public class ResourceModule : MCPackModule
	{
		/// <inheritdoc />
		public ResourceModule(IFilesystem entry) : base(entry) { }

		/// <inheritdoc />
		internal override bool Load()
		{
			return base.Load();
		}
	}
}