using Alex.Blocks.State;

namespace Alex.Blocks.Properties
{
	public class UnknownProperty : StateProperty
	{
		public UnknownProperty(string name) : base(name, typeof(string))
		{
		}

		public override object ValueFromString(string value)
		{
			return value;
		}

		public override object[] GetValidValues()
		{
			return new object[0];
		}
	}
}
