namespace Alex.MoLang.Parser.Tokenizer
{
	public class TokenPosition
	{
		//	public int StartLineNumber;
		//public int EndLineNumber;
		//public int StartColumn;
		//	public int  EndColumn;
		public int LineNumber;
		public int Index;

		public TokenPosition(int lastStepLine, int currentLine, int lastStep, int index)
		{
			LineNumber = currentLine;
			Index = index;
		}
	}
}