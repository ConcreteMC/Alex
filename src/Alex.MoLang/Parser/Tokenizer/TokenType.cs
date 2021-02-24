using System.Collections.Generic;

namespace Alex.MoLang.Parser.Tokenizer
{
	public class TokenType
	{
		public static readonly TokenType EqualsEquals            = new TokenType("==");
		public static readonly TokenType NotEquals        = new TokenType("!=");
		public static readonly TokenType Coalesce          = new TokenType("??");
		public static readonly TokenType And               = new TokenType("&&");
		public static readonly TokenType Or                = new TokenType("||");
		public static readonly TokenType GreaterOrEquals = new TokenType(">=");
		public static readonly TokenType SmallerOrEquals = new TokenType("<=");
		public static readonly TokenType Arrow = new TokenType("->");
		
		public static readonly TokenType Greater             = new TokenType(">");
		public static readonly TokenType Smaller             = new TokenType("<");
		public static readonly TokenType BracketLeft        = new TokenType("(");
		public static readonly TokenType BracketRight       = new TokenType(")");
		public static readonly TokenType ArrayLeft          = new TokenType("[");
		public static readonly TokenType ArrayRight         = new TokenType("]");
		public static readonly TokenType CurlyBracketLeft  = new TokenType("{");
		public static readonly TokenType CurlyBracketRight = new TokenType("}");
		public static readonly TokenType Comma               = new TokenType(",");
		public static readonly TokenType Assign              = new TokenType("=");
		public static readonly TokenType Plus                = new TokenType("+");
		public static readonly TokenType Minus               = new TokenType("-");
		public static readonly TokenType Asterisk            = new TokenType("*");
		public static readonly TokenType Slash               = new TokenType("/");
		public static readonly TokenType Question            = new TokenType("?");
		public static readonly TokenType Colon               = new TokenType(":");
		public static readonly TokenType Semicolon           = new TokenType(";");
		public static readonly TokenType Bang = new TokenType("!");
		
		public static readonly TokenType Return   = new TokenType("return");
		public static readonly TokenType Continue = new TokenType("continue");
		public static readonly TokenType Break    = new TokenType("break");
		public static readonly TokenType ForEach = new TokenType("for_each");
		public static readonly TokenType Loop     = new TokenType("loop");
		public static readonly TokenType This     = new TokenType("this");
		public static readonly TokenType True     = new TokenType("true");
		public static readonly TokenType False    = new TokenType("false");
		public static readonly TokenType String   = new TokenType("", "string");
		public static readonly TokenType Number   = new TokenType("", "number");
		public static readonly TokenType Name     = new TokenType("", "name");
		public static readonly TokenType Eof      = new TokenType("", "EndOfFile");
		
		public string Symbol   { get; }
		public string TypeName { get; }
		public TokenType(string symbol, string typename = "")
		{
			Symbol = symbol;
			TypeName = typename;
		}

		public static readonly TokenType[] Values = new TokenType[]
		{
			EqualsEquals,
			NotEquals,
			Coalesce,
			And,
			Or,
			GreaterOrEquals,
			SmallerOrEquals,
			Arrow,
			Greater,
			Smaller,
			BracketLeft,
			BracketRight,
			ArrayLeft,
			ArrayRight,
			CurlyBracketLeft,
			CurlyBracketRight,
			Comma,
			Assign,
			Plus,
			Minus,
			Asterisk,
			Slash,
			Question,
			Colon,
			Semicolon,
			Bang,
			Return,
			Continue,
			Break,
			ForEach,
			Loop,
			This,
			True,
			False,
			String,
			Number,
			Name,
			Eof
		};

		public static TokenType BySymbol(string symbol) {
			foreach (TokenType tokenType in TokenType.Values) {
				if (tokenType.Symbol.Equals(symbol)) {
					return tokenType;
				}
			}

			return null;
		}
	}
}