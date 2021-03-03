using System;
using System.Collections.Generic;
using Alex.MoLang.Parser.Exceptions;
using Alex.MoLang.Parser.Parselet;
using Alex.MoLang.Parser.Tokenizer;

namespace Alex.MoLang.Parser
{
	public class MoLangParser
	{
		private readonly static Dictionary<TokenType, PrefixParselet> PrefixParselets =
			new Dictionary<TokenType, PrefixParselet>();

		private readonly static Dictionary<TokenType, InfixParselet> InfixParselets =
			new Dictionary<TokenType, InfixParselet>();

		private readonly TokenIterator _tokenIterator;
		private readonly List<Token>   _readTokens = new List<Token>();

		static MoLangParser()
		{
			PrefixParselets.Add(TokenType.Name, new NameParselet());
			PrefixParselets.Add(TokenType.String, new StringParselet());
			PrefixParselets.Add(TokenType.Number, new NumberParselet());
			PrefixParselets.Add(TokenType.FloatingPointNumber, new FloatParselet());
			PrefixParselets.Add(TokenType.True, new BooleanParselet());
			PrefixParselets.Add(TokenType.False, new BooleanParselet());
			PrefixParselets.Add(TokenType.Return, new ReturnParselet());
			PrefixParselets.Add(TokenType.Continue, new ContinueParselet());
			PrefixParselets.Add(TokenType.Break, new BreakParselet());
			PrefixParselets.Add(TokenType.Loop, new LoopParselet());
			PrefixParselets.Add(TokenType.ForEach, new ForEachParselet());
			PrefixParselets.Add(TokenType.This, new ThisParselet());
			PrefixParselets.Add(TokenType.BracketLeft, new GroupParselet());
			PrefixParselets.Add(TokenType.CurlyBracketLeft, new BracketScopeParselet());
			PrefixParselets.Add(TokenType.Minus, new UnaryMinusParselet());
			PrefixParselets.Add(TokenType.Plus, new UnaryPlusParselet());
			PrefixParselets.Add(TokenType.Bang, new BooleanNotParselet());

			InfixParselets.Add(TokenType.Question, new TernaryParselet());
			InfixParselets.Add(TokenType.ArrayLeft, new ArrayAccessParselet());
			InfixParselets.Add(TokenType.Plus, new GenericBinaryOpParselet(Precedence.Sum));
			InfixParselets.Add(TokenType.Minus, new GenericBinaryOpParselet(Precedence.Sum));
			InfixParselets.Add(TokenType.Slash, new GenericBinaryOpParselet(Precedence.Product));
			InfixParselets.Add(TokenType.Asterisk, new GenericBinaryOpParselet(Precedence.Product));
			InfixParselets.Add(TokenType.EqualsEquals, new GenericBinaryOpParselet(Precedence.Compare));
			InfixParselets.Add(TokenType.NotEquals, new GenericBinaryOpParselet(Precedence.Compare));
			InfixParselets.Add(TokenType.Greater, new GenericBinaryOpParselet(Precedence.Compare));
			InfixParselets.Add(TokenType.GreaterOrEquals, new GenericBinaryOpParselet(Precedence.Compare));
			InfixParselets.Add(TokenType.Smaller, new GenericBinaryOpParselet(Precedence.Compare));
			InfixParselets.Add(TokenType.SmallerOrEquals, new GenericBinaryOpParselet(Precedence.Compare));
			InfixParselets.Add(TokenType.And, new GenericBinaryOpParselet(Precedence.And));
			InfixParselets.Add(TokenType.Or, new GenericBinaryOpParselet(Precedence.Or));
			InfixParselets.Add(TokenType.Coalesce, new GenericBinaryOpParselet(Precedence.Coalesce));
			InfixParselets.Add(TokenType.Arrow, new GenericBinaryOpParselet(Precedence.Arrow));
			InfixParselets.Add(TokenType.Assign, new AssignParselet());
		}

		public MoLangParser(TokenIterator iterator)
		{
			_tokenIterator = iterator;
		}

		public List<IExpression> Parse()
		{
			List<IExpression> exprs = new List<IExpression>();

			do
			{
				IExpression expr = ParseExpression();

				if (expr != null)
				{
					exprs.Add(expr);
				}
				else
				{
					break;
				}
			} while (MatchToken(TokenType.Semicolon));

			return exprs;
		}

		public IExpression ParseExpression()
		{
			return ParseExpression(Precedence.Anything);
		}

		public IExpression ParseExpression(Precedence precedence)
		{
			Token token = ConsumeToken();

			if (token.Type.Equals(TokenType.Eof))
			{
				return null;
			}

			PrefixParselet parselet = PrefixParselets[token.Type];

			if (parselet == null)
			{
				throw new MoLangParserException("Cannot parse " + token.Type.GetType().Name + " expression");
			}

			IExpression expr = parselet.Parse(this, token);
			InitExpr(expr, token);

			return ParseInfixExpression(expr, precedence);
		}

		private IExpression ParseInfixExpression(IExpression left, Precedence precedence)
		{
			Token token;

			while (precedence < GetPrecedence())
			{
				token = ConsumeToken();
				left = InfixParselets[token.Type].Parse(this, token, left);
				InitExpr(left, token);
			}

			return left;
		}

		private void InitExpr(IExpression expression, Token token)
		{
			expression.Attributes["position"] = token.Position; //.put("position", token.getPosition());
		}

		private Precedence GetPrecedence()
		{
			Token token = ReadToken();

			if (token != null)
			{
				//if (token.Type == TokenType.Eof)
				//	return Precedence.Product;
			//	InfixParselet parselet = InfixParselets[token.Type];

				if ( InfixParselets.TryGetValue(token.Type, out var parselet))
				{
					return parselet.Precedence;
				}
				else
				{ 
					//throw new MoLangParserException($"Invalid precedence token of type '{token.Type.TypeName}' and text '{token.Text}' at {token.Position.LineNumber}:{token.Position.Index}");
				}
			}

			return Precedence.Anything;
		}

		public List<IExpression> ParseArgs()
		{
			List<IExpression> args = new List<IExpression>();

			if (MatchToken(TokenType.BracketLeft))
			{
				if (!MatchToken(TokenType.BracketRight))
				{
					// check for empty groups
					do
					{
						args.Add(ParseExpression());
					} while (MatchToken(TokenType.Comma));

					ConsumeToken(TokenType.BracketRight);
				}
			}

			return args;
		}

		public String FixNameShortcut(String name)
		{
			String[] splits = name.Split(".");

			switch (splits[0])
			{
				case "q":
					splits[0] = "query";

					break;

				case "v":
					splits[0] = "variable";

					break;

				case "t":
					splits[0] = "temp";

					break;

				case "c":
					splits[0] = "context";

					break;
			}

			return String.Join(".", splits);
		}

		public String GetNameHead(String name)
		{
			return name.Split(".")[0];
		}

		public Token ConsumeToken()
		{
			return ConsumeToken(null);
		}

		public Token ConsumeToken(TokenType expectedType)
		{
			_tokenIterator.Step();
			Token token = ReadToken();

			if (expectedType != null)
			{
				if (!token.Type.Equals(expectedType))
				{
					throw new MoLangParserException(
						$"Expected token of type '{expectedType.TypeName}' but found '{token.Type.TypeName}' at line {token.Position.LineNumber}:{token.Position.Index}");
				}
			}

			if (_readTokens.Count > 0)
			{
				_readTokens.RemoveAt(0);

				return token;
			}
			
			return null;
		}

		public bool MatchToken(TokenType expectedType)
		{
			return MatchToken(expectedType, true);
		}

		public bool MatchToken(TokenType expectedType, bool consume)
		{
			Token token = ReadToken();

			if (token == null || !token.Type.Equals(expectedType))
			{
				return false;
			}
			else
			{
				if (consume)
				{
					ConsumeToken();
				}

				return true;
			}
		}

		private Token ReadToken()
		{
			return ReadToken(0);
		}

		private Token ReadToken(int distance)
		{
			while (distance >= _readTokens.Count)
			{
				_readTokens.Add(_tokenIterator.Next());
			}

			return _readTokens[distance];
		}
	}
}