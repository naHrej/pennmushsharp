namespace PennMushSharp.Extraction;

public sealed class ExpressionParser
{
  private readonly string _expression;
  private readonly IReadOnlyDictionary<string, ulong> _values;
  private int _index;

  public ExpressionParser(string expression, IReadOnlyDictionary<string, ulong> values)
  {
    _expression = expression.Trim();
    _values = values;
  }

  public ulong Evaluate()
  {
    _index = 0;
    var result = ParseOr();
    SkipWhitespace();
    if (_index != _expression.Length)
      throw new FormatException($"Unexpected trailing characters in expression '{_expression}'.");
    return result;
  }

  private ulong ParseOr()
  {
    var value = ParseXor();
    while (true)
    {
      SkipWhitespace();
      if (Match("||"))
      {
        value = (value != 0 || ParseXor() != 0) ? 1UL : 0UL;
      }
      else if (Match("|"))
      {
        value |= ParseXor();
      }
      else
      {
        return value;
      }
    }
  }

  private ulong ParseXor()
  {
    var value = ParseAnd();
    while (true)
    {
      SkipWhitespace();
      if (Match("^"))
        value ^= ParseAnd();
      else
        return value;
    }
  }

  private ulong ParseAnd()
  {
    var value = ParseShift();
    while (true)
    {
      SkipWhitespace();
      if (Match("&&"))
      {
        value = (value != 0 && ParseShift() != 0) ? 1UL : 0UL;
      }
      else if (Match("&"))
      {
        value &= ParseShift();
      }
      else
      {
        return value;
      }
    }
  }

  private ulong ParseShift()
  {
    var value = ParseAdditive();
    while (true)
    {
      SkipWhitespace();
      if (Match("<<"))
      {
        var right = ParseAdditive();
        value <<= (int)right;
      }
      else if (Match(">>"))
      {
        var right = ParseAdditive();
        value >>= (int)right;
      }
      else
      {
        return value;
      }
    }
  }

  private ulong ParseAdditive()
  {
    var value = ParseMultiplicative();
    while (true)
    {
      SkipWhitespace();
      if (Match("+"))
      {
        value += ParseMultiplicative();
      }
      else if (Match("-"))
      {
        value -= ParseMultiplicative();
      }
      else
      {
        return value;
      }
    }
  }

  private ulong ParseMultiplicative()
  {
    var value = ParseUnary();
    while (true)
    {
      SkipWhitespace();
      if (Match("*"))
      {
        value *= ParseUnary();
      }
      else if (Match("/"))
      {
        value /= ParseUnary();
      }
      else if (Match("%"))
      {
        value %= ParseUnary();
      }
      else
      {
        return value;
      }
    }
  }

  private ulong ParseUnary()
  {
    SkipWhitespace();
    if (Match("+"))
      return ParseUnary();
    if (Match("-"))
      return unchecked(0UL - ParseUnary());
    if (Match("~"))
      return ~ParseUnary();
    return ParsePrimary();
  }

  private ulong ParsePrimary()
  {
    SkipWhitespace();
    if (Match("("))
    {
      var value = ParseOr();
      SkipWhitespace();
      Expect(")");
      return value;
    }

    if (Peek() == '\'' || Peek() == '"')
      return ParseCharLiteral();

    if (char.IsDigit(Peek()))
      return ParseNumber();

    return ParseIdentifier();
  }

  private ulong ParseNumber()
  {
    var start = _index;
    while (_index < _expression.Length && (char.IsDigit(_expression[_index]) || _expression[_index] is 'x' or 'X' or 'a' or 'b' or 'c' or 'd' or 'e' or 'f' or 'A' or 'B' or 'C' or 'D' or 'E' or 'F'))
      _index++;

    while (_index < _expression.Length && _expression[_index] is 'u' or 'U' or 'l' or 'L')
      _index++;

    var token = _expression.Substring(start, _index - start).TrimEnd('U', 'u', 'L', 'l');

    if (token.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
      return Convert.ToUInt64(token[2..], 16);

    if (token.StartsWith("0") && token.Length > 1)
      return Convert.ToUInt64(token, 8);

    return Convert.ToUInt64(token, 10);
  }

  private ulong ParseCharLiteral()
  {
    var literal = LiteralParser.ParseCharLiteralToken(_expression, ref _index);
    return literal;
  }

  private ulong ParseIdentifier()
  {
    var start = _index;
    while (_index < _expression.Length && (char.IsLetterOrDigit(_expression[_index]) || _expression[_index] == '_'))
      _index++;

    if (start == _index)
      throw new FormatException($"Unexpected token at position {_index} in '{_expression}'.");

    var identifier = _expression.Substring(start, _index - start);
    if (!_values.TryGetValue(identifier, out var value))
      throw new UnknownIdentifierException(identifier);
    return value;
  }

  private void SkipWhitespace()
  {
    while (_index < _expression.Length && char.IsWhiteSpace(_expression[_index]))
      _index++;
  }

  private bool Match(string token)
  {
    if (_expression.AsSpan(_index).StartsWith(token))
    {
      _index += token.Length;
      return true;
    }
    return false;
  }

  private void Expect(string token)
  {
    if (!Match(token))
      throw new FormatException($"Expected '{token}' at position {_index} in '{_expression}'.");
  }

  private char Peek() => _index < _expression.Length ? _expression[_index] : '\0';
}

public sealed class UnknownIdentifierException : Exception
{
  public UnknownIdentifierException(string identifier)
    : base($"Unknown identifier '{identifier}'.")
  {
  }
}
