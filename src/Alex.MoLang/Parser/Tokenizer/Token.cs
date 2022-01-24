namespace Alex.MoLang.Parser.Tokenizer
{
	public class Token
	{
		public TokenType Type;
		public string Text;
		public TokenPosition Position;

		public Token(TokenType tokenType, TokenPosition position)
		{
			this.Type = tokenType;
			this.Text = tokenType.Symbol;
			this.Position = position;
		}

		public Token(TokenType tokenType, string text, TokenPosition position)
		{
			this.Type = tokenType;
			this.Text = text;
			this.Position = position;
		}
	}
}