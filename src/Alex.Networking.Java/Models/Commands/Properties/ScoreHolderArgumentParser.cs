namespace Alex.Networking.Java.Models.Commands.Properties
{
	public class ScoreHolderArgumentParser : ArgumentParser
	{
		private byte _flags;
		/// <inheritdoc />
		public ScoreHolderArgumentParser(string name, byte flags) : base(name)
		{
			_flags = flags;
		}
	}
	
	public class EntityArgumentParser : ArgumentParser
	{
		private byte _flags;
		/// <inheritdoc />
		public EntityArgumentParser(string name, byte flags) : base(name)
		{
			_flags = flags;
		}
	}
}