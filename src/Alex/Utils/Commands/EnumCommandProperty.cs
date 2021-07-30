using System;
using System.Linq;
using NLog;

namespace Alex.Utils.Commands
{
	public class EnumCommandProperty : CommandProperty
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(EnumCommandProperty));
		public string[] Options { get; }

		/// <inheritdoc />
		public EnumCommandProperty(string name, bool required = true, string[] options = null, string enumName = "enum") : base(name, required, enumName)
		{
			Options = options;
		}

		/// <inheritdoc />
		public override bool TryParse(SeekableTextReader reader)
		{
			if (reader.ReadSingleWord(out string result) > 0)
			{
				Log.Debug($"Enum Read: {result}");
				var options = Options.Any(x => x.StartsWith(result, StringComparison.InvariantCultureIgnoreCase));

				if (options)
				{
					Matches = Options.Where(x => x.StartsWith(result, StringComparison.InvariantCultureIgnoreCase))
					   .ToArray();
					return true;
				}
			}
			
			//	Log.Debug($"")

			return false;
		}
	}
}