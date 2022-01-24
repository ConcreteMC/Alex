using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Runtime
{
	public class MoScope
	{
		public bool IsBreak { get; set; } = false;
		public bool IsContinue { get; set; } = false;
		public IMoValue ReturnValue { get; set; } = null;
	}
}