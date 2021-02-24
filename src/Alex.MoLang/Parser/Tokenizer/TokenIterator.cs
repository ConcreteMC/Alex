namespace Alex.MoLang.Parser.Tokenizer
{
    public class TokenIterator
    {
        private readonly string _code;

        private int _index        = 0;
        private int _currentLine  = 0;
        private int _lastStep     = 0;
        private int _lastStepLine = 0;

        public TokenIterator(string code) {
            this._code = code;
        }

        public bool HasNext()
        {
            return _index < _code.Length;
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

                var    expr      = GetStringAt(_index);
                TokenType tokenType = TokenType.BySymbol(expr);

                if (tokenType != null)
                {
                    _index++;

                    return new Token(tokenType, GetPosition());
                }
                else if (expr.Equals("'"))
                {
                    int stringStart  = _index;
                    int stringLength = 1;

                    while (stringStart + stringLength < _code.Length && !GetStringAt(stringStart + stringLength).Equals("'"))
                    {
                        stringLength++;
                    }

                    stringLength++;
                    _index = stringStart + stringLength;

                    return new Token(
                        TokenType.String, _code.Substring(stringStart + 1, stringLength), GetPosition());
                }
                else if (char.IsLetter(expr[0]))
                {
                    var nameStart  = _index;
                    int nameLength = 1;

                    while (nameStart + nameLength < _code.Length && (char.IsLetterOrDigit(GetStringAt(nameStart + nameLength)[0])
                                                                     || GetStringAt(nameStart + nameLength).Equals("_")
                                                                     || GetStringAt(nameStart + nameLength).Equals(".")))
                    {
                        nameLength++;
                    }

                    string    value = _code.Substring(_index, nameLength).ToLower();
                    TokenType token = TokenType.BySymbol(value);

                    if (token == null)
                    {
                        token = TokenType.Name;
                    }

                    _index = nameStart + nameLength;

                    return new Token(token, value, GetPosition());
                }
                else if (char.IsDigit(expr[0]))
                {
                    int     numStart   = _index;
                    int     numLength  = 1;
                    bool hasDecimal = false;

                    while (numStart + numLength < _code.Length && (char.IsDigit(GetStringAt(numStart + numLength)[0])
                                                         || (GetStringAt(numStart + numLength).Equals(".") && !hasDecimal)))
                    {
                        if (GetStringAt(numStart + numLength).Equals("."))
                        {
                            hasDecimal = true;
                        }

                        numLength++;
                    }

                    _index = numStart + numLength;

                    return new Token(TokenType.Number, _code.Substring(numStart, numLength), GetPosition());
                }
                else if (expr.Equals("\n") || expr.Equals("\r"))
                {
                    _currentLine++;
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

        public TokenPosition GetPosition()
        {
            return new TokenPosition(_lastStepLine, _currentLine, _lastStep, _index);
        }

        public string GetStringAt(string str, int i)
        {
            return str.Substring(i, 1);
        }

        public string GetStringAt(int i)
        {
            return _code.Substring(i, 1);
        }
    }
}