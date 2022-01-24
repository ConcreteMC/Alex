using System;
using Alex.MoLang.Parser.Exceptions;

namespace Alex.MoLang.Parser.Tokenizer
{
	public class TokenIterator
	{
		private readonly string _code;

		private int _index = 0;
		private int _currentLine = 0;
		private int _lastStep = 0;
		private int _lastStepLine = 0;

		public TokenIterator(string code)
		{
			this._code = code;
		}

		public Token Next()
		{
			while (_index < _code.Length)
			{
				if (_code.Length > _index + 1)
				{
					// check tokens with double chars
					TokenType token = TokenType.BySymbol(_code.Substring(_index, 2));

					if (token != null)
					{
						_index += 2;

						return new Token(token, GetPosition());
					}
				}

				var expr = GetCharacterAt(_index);
				TokenType tokenType = TokenType.BySymbol(expr);

				if (tokenType != null)
				{
					_index++;

					return new Token(tokenType, GetPosition());
				}
				else
				{
					if (expr.Equals('\''))
					{
						int stringStart = _index + 1;
						int stringLength = 0;

						while (stringStart + stringLength < _code.Length
						       && !GetCharacterAt(stringStart + stringLength).Equals('\''))
						{
							stringLength++;
						}

						stringLength++;
						_index = stringStart + stringLength;

						return new Token(
							TokenType.String, _code.Substring(stringStart, stringLength - 1), GetPosition());
					}

					if (char.IsLetter(expr))
					{
						var nameStart = _index;
						int nameLength = 1;

						while (nameStart + nameLength < _code.Length
						       && (char.IsLetterOrDigit(GetCharacterAt(nameStart + nameLength))
						           || GetCharacterAt(nameStart + nameLength).Equals('_')
						           || GetCharacterAt(nameStart + nameLength).Equals('.')))
						{
							nameLength++;
						}

						string value = _code.Substring(_index, nameLength);
						TokenType token = TokenType.BySymbol(value);

						if (token == null)
						{
							token = TokenType.Name;
						}

						_index = nameStart + nameLength;

						return new Token(token, value, GetPosition());
					}

					if (char.IsDigit(expr))
					{
						int numStart = _index;
						int numLength = 1;
						bool hasDecimal = false;
						bool isFloat = false;


						while (numStart + numLength < _code.Length)
						{
							var character = GetCharacterAt(numStart + numLength);

							if (!char.IsDigit(character))
							{
								if (character == 'f' && !isFloat)
								{
									isFloat = true;
								}
								else if (character == '.' && !hasDecimal)
								{
									hasDecimal = true;
								}
								else
								{
									break;
								}
							}

							numLength++;

							if (isFloat)
								break;
						}

						_index = numStart + numLength;

						if (isFloat)
							return new Token(
								TokenType.FloatingPointNumber, _code.Substring(numStart, numLength - 1), GetPosition());

						return new Token(TokenType.Number, _code.Substring(numStart, numLength), GetPosition());
					}

					if (expr.Equals('\n') || expr.Equals('\r'))
					{
						_currentLine++;
					}
				}

				_index++;
			}

			return new Token(TokenType.Eof, GetPosition());
		}

		public void Step()
		{
			_lastStep = _index;
			_lastStepLine = _currentLine;
		}

		private TokenPosition GetPosition()
		{
			return new TokenPosition(_lastStepLine, _currentLine, _lastStep, _index);
		}

		private char GetCharacterAt(int index)
		{
			if (index > _code.Length - 1)
				throw new MoLangParserException(
					$"Value '{index + 1}' is outside of range Min: 0, Max:{_code.Length - 1}",
					new IndexOutOfRangeException(nameof(index)));

			return _code[index]; //.Substring(index, 1)[0];
		}
	}
}