using System;
using System.Linq;
using Alex.Blocks.State;

namespace Alex.Blocks.Properties
{
    public class DynamicStateProperty : StateProperty<string>
    {
		private string[] Valid { get; }
	    public DynamicStateProperty(string name, string[] validValues) : base(name)
	    {
		    Valid = validValues;
	    }

	    public override object[] GetValidValues()
	    {
		    return Valid;
	    }

	    public override string ParseValue(string value)
	    {
		    if (Valid.Any(x => x.Equals(value)))
		    {
			    return value;
		    }

		    return Valid.FirstOrDefault();
	    }

	    public override string ToString(string v)
	    {
		    return v;
	    }
    }
}
