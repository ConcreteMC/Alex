namespace Alex.MoLang.Parser.Tokenizer
{
	public class TokenPosition 
	{
		public int StartLineNumber;
		public int EndLineNumber;
		public int StartColumn;
		public int  EndColumn;

		public TokenPosition(int lastStepLine, int currentLine, int lastStep, int index)
		{
			
		}
	}
}