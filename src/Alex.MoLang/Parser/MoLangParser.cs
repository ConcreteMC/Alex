using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Alex.MoLang.Parser.Exceptions;
using Alex.MoLang.Parser.Parselet;
using Alex.MoLang.Parser.Tokenizer;
using Alex.MoLang.Parser.Visitors;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser
{
	public class MoLangParser
	{
		public static TimeSpan TotalTimeSpent { get; private set; } = TimeSpan.Zero;

		private static readonly Dictionary<TokenType, PrefixParselet> PrefixParselets =
			new Dictionary<TokenType, PrefixParselet>();

		private static readonly Dictionary<TokenType, InfixParselet> InfixParselets =
			new Dictionary<TokenType, InfixParselet>();

		private readonly TokenIterator _tokenIterator;

		private readonly List<Token> _readTokens = new List<Token>();

		//	private static readonly ExprTraverser ExprTraverser;
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

			//ExprTraverser = new ExprTraverser();
			//ExprTraverser.Visitors.Add(new ExprConnectingVisitor());
		}

		public MoLangParser(TokenIterator iterator)
		{
			_tokenIterator = iterator;
		}

		public IExpression[] Parse()
		{
			//	var traverser = new ExprTraverser();
			//	traverser.Visitors.Add(new ExprConnectingVisitor());

			Stopwatch sw = Stopwatch.StartNew();

			try
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

				return exprs.ToArray(); // traverser.Traverse(exprs.ToArray());
			}
			catch (Exception ex)
			{
				throw new MoLangParserException($"Failed to parse expression", ex);
			}
			finally
			{
				sw.Stop();
				TotalTimeSpent += sw.Elapsed;
			}
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
			expression.Meta.Token = token;
			//expression.Attributes["position"] = token.Position; //.put("position", token.getPosition());
		}

		private Precedence GetPrecedence()
		{
			Token token = ReadToken();

			if (token != null)
			{
				//if (token.Type == TokenType.Eof)
				//	return Precedence.Product;
				//	InfixParselet parselet = InfixParselets[token.Type];

				if (InfixParselets.TryGetValue(token.Type, out var parselet))
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

		public string FixNameShortcut(string name)
		{
			//String[] splits = name.Split(".");
			var index = name.IndexOf('.');

			if (index == -1) //Not found.
			{
				return name;
			}

			var first = name.Substring(0, index);

			switch (first)
			{
				case "q":
					first = "query";

					break;

				case "v":
					first = "variable";

					break;

				case "t":
					first = "temp";

					break;

				case "c":
					first = "context";

					break;
			}

			return name.Remove(0, index).Insert(0, first); // String.Join(".", splits);
		}

		public static string GetNameHead(string name)
		{
			return name.Substring(0, name.IndexOf('.')); // name.Split(".")[0];
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

		public static IExpression[] Parse(string input)
		{
			return new MoLangParser(new TokenIterator(input)).Parse();
		}
	}
}