using System.Collections.Generic;
using System.Threading;

namespace Alex.MoLang.Parser.Tokenizer
{
	public class TokenType
	{
		public static readonly TokenType EqualsEquals = new("==", "EqualsEquals");
		public static readonly TokenType NotEquals = new("!=", "Not Equals");
		public static readonly TokenType Coalesce = new("??", "Coalesce");
		public static readonly TokenType And = new("&&", "AndAnd");
		public static readonly TokenType Or = new("||", "Or");
		public static readonly TokenType GreaterOrEquals = new(">=", "Greater or equal to");
		public static readonly TokenType SmallerOrEquals = new("<=", "Smaller or equal to");
		public static readonly TokenType Arrow = new("->", "Arrow");

		public static readonly TokenType Greater = new(">", "Bigger");
		public static readonly TokenType Smaller = new("<", "Smaller");
		public static readonly TokenType BracketLeft = new("(", "Bracket Left");
		public static readonly TokenType BracketRight = new(")", "Bracket Right");
		public static readonly TokenType ArrayLeft = new("[", "Array Left");
		public static readonly TokenType ArrayRight = new("]", "Array Right");
		public static readonly TokenType CurlyBracketLeft = new("{", "Curly Bracket Left");
		public static readonly TokenType CurlyBracketRight = new("}", "Curly Bracket Right");
		public static readonly TokenType Comma = new(",", "Comma");
		public static readonly TokenType Assign = new("=", "Assign");
		public static readonly TokenType Plus = new("+", "Plus");
		public static readonly TokenType Minus = new("-", "Minus");
		public static readonly TokenType Asterisk = new("*", "Asterisk");
		public static readonly TokenType Slash = new("/", "Slash");
		public static readonly TokenType Question = new("?", "Question");
		public static readonly TokenType Colon = new(":", "Colon");
		public static readonly TokenType Semicolon = new(";", "Semicolon");
		public static readonly TokenType Bang = new("!", "Bang");

		public static readonly TokenType Return = new("return", "Return");
		public static readonly TokenType Continue = new("continue", "Continue");
		public static readonly TokenType Break = new("break", "Break");
		public static readonly TokenType ForEach = new("for_each", "ForEach");
		public static readonly TokenType Loop = new("loop", "Loop");
		public static readonly TokenType This = new("this", "Reference");
		public static readonly TokenType True = new("true", "bool");
		public static readonly TokenType False = new("false", "bool");
		public static readonly TokenType String = new("", "string");
		public static readonly TokenType Number = new("", "number");
		public static readonly TokenType FloatingPointNumber = new("", "floating point number");
		public static readonly TokenType Name = new("", "name");
		public static readonly TokenType Eof = new("", "EndOfFile");

		private static int _typeCounter = 0;

		public string Symbol { get; }
		public string TypeName { get; }

		private readonly int _typeId;

		private TokenType(string symbol, string typename = "")
		{
			Symbol = symbol;
			TypeName = typename;

			_typeId = Interlocked.Increment(ref _typeCounter);
		}

		private static readonly TokenType[] Values = new TokenType[]
		{
			EqualsEquals, NotEquals, Coalesce, And, Or, GreaterOrEquals, SmallerOrEquals, Arrow, Greater, Smaller,
			BracketLeft, BracketRight, ArrayLeft, ArrayRight, CurlyBracketLeft, CurlyBracketRight, Comma, Assign,
			Plus, Minus, Asterisk, Slash, Question, Colon, Semicolon, Bang, Return, Continue, Break, ForEach, Loop,
			This, True, False, String, Number, Name, Eof
		};

		public static TokenType BySymbol(string symbol)
		{
			foreach (TokenType tokenType in TokenType.Values)
			{
				if (tokenType.Symbol.Equals(symbol))
				{
					return tokenType;
				}
			}

			return null;
		}

		public static TokenType BySymbol(char symbol)
		{
			return BySymbol(symbol.ToString());
		}

		public bool Equals(TokenType other)
		{
			return _typeId == other._typeId;
		}
	}
}