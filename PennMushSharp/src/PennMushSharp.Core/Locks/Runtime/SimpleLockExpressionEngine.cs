namespace PennMushSharp.Core.Locks.Runtime;

public sealed class SimpleLockExpressionEngine : ILockExpressionEngine
{
  public static SimpleLockExpressionEngine Instance { get; } = new();

  private SimpleLockExpressionEngine() { }

  public bool Evaluate(int playerDbRef, int thingDbRef, string expression)
  {
    if (string.IsNullOrWhiteSpace(expression))
      return false;

    var parser = new Parser(expression, playerDbRef, thingDbRef);
    return parser.Parse();
  }

  private sealed class Parser
  {
    private readonly string _expr;
    private readonly int _player;
    private readonly int _thing;
    private int _index;

    public Parser(string expr, int player, int thing)
    {
      _expr = expr.Trim();
      _player = player;
      _thing = thing;
    }

    public bool Parse()
    {
      try
      {
        var result = ParseOr();
        SkipWhitespace();
        if (_index != _expr.Length)
          return false;
        return result;
      }
      catch
      {
        return false;
      }
    }

    private bool ParseOr()
    {
      var value = ParseAnd();
      while (true)
      {
        SkipWhitespace();
        if (Match('|'))
        {
          var right = ParseAnd();
          value = value || right;
        }
        else
        {
          return value;
        }
      }
    }

    private bool ParseAnd()
    {
      var value = ParseUnary();
      while (true)
      {
        SkipWhitespace();
        if (Match('&'))
        {
          var right = ParseUnary();
          value = value && right;
        }
        else
        {
          return value;
        }
      }
    }

    private bool ParseUnary()
    {
      SkipWhitespace();
      if (Match('!'))
        return !ParseUnary();
      if (Match('('))
      {
        var inner = ParseOr();
        Expect(')');
        return inner;
      }
      return ParseLiteral();
    }

    private bool ParseLiteral()
    {
      SkipWhitespace();
      var start = _index;
      while (_index < _expr.Length && !char.IsWhiteSpace(_expr[_index]) && _expr[_index] is not '&' and not '|' and not ')')
        _index++;
      if (start == _index)
        throw new FormatException();
      var token = _expr[start.._index];
      return EvaluateToken(token);
    }

    private bool EvaluateToken(string token)
    {
      if (string.Equals(token, "TRUE_BOOLEXP", StringComparison.OrdinalIgnoreCase) ||
          string.Equals(token, "TRUE", StringComparison.OrdinalIgnoreCase))
        return true;
      if (string.Equals(token, "FALSE_BOOLEXP", StringComparison.OrdinalIgnoreCase) ||
          string.Equals(token, "FALSE", StringComparison.OrdinalIgnoreCase))
        return false;

      if (token.StartsWith('#'))
      {
        if (int.TryParse(token.AsSpan(1), out var dbref))
          return _player == dbref;
        return false;
      }

      if (token.Equals("PLAYER", StringComparison.OrdinalIgnoreCase))
        return true;

      return false; // Unknown token
    }

    private void SkipWhitespace()
    {
      while (_index < _expr.Length && char.IsWhiteSpace(_expr[_index]))
        _index++;
    }

    private bool Match(char c)
    {
      if (_index < _expr.Length && _expr[_index] == c)
      {
        _index++;
        return true;
      }
      return false;
    }

    private void Expect(char c)
    {
      if (!Match(c))
        throw new FormatException();
    }
  }
}
