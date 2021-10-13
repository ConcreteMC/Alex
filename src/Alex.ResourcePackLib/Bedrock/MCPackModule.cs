using Alex.ResourcePackLib.IO.Abstract;

namespace Alex.ResourcePackLib.Bedrock
{
	public class MCPackModule
	{
		public virtual string Name
		{
			get
			{
				return Entry.Name;
			}
		}

		protected IFilesystem Entry { get; }
		protected MCPackModule(IFilesystem entry)
		{
			Entry = entry;
		}

		internal virtual bool Load()
		{
			return false;
		}
	}
}