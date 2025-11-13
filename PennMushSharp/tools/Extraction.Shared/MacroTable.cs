using System.Text.RegularExpressions;

namespace PennMushSharp.Extraction;

public sealed class MacroTable
{
  private readonly Dictionary<string, ulong> _values = new(StringComparer.Ordinal);
  private readonly List<(string Name, string Expression)> _pending = new();

  public void Load(string path)
  {
    var text = File.ReadAllText(path);
    text = CommentUtilities.RemoveComments(text);
    text = CommentUtilities.CombineContinuations(text);

    foreach (var line in text.Split('\n'))
    {
      var trimmed = line.Trim();
      if (!trimmed.StartsWith("#define ", StringComparison.Ordinal))
        continue;

      var definition = trimmed.Substring("#define ".Length).Trim();
      if (string.IsNullOrEmpty(definition))
        continue;

      var parts = Regex.Split(definition, "\\s+", RegexOptions.Compiled, TimeSpan.FromSeconds(1));
      if (parts.Length < 2)
        continue;

      var name = parts[0];
      if (name.Contains('('))
        continue; // Skip macros with parameters

      var valueExpr = definition.Substring(name.Length).Trim();
      if (string.IsNullOrEmpty(valueExpr))
        continue;

      valueExpr = valueExpr.TrimEnd(';');
      var trimmedExpr = valueExpr.Trim();
      if (trimmedExpr.StartsWith('"') && trimmedExpr.EndsWith('"'))
        continue;

      if (!TryAdd(name, valueExpr))
      {
        _pending.Add((name, valueExpr));
      }
    }
  }

  public void ResolvePending()
  {
    var resolved = true;
    while (_pending.Count > 0 && resolved)
    {
      resolved = false;
      for (var i = _pending.Count - 1; i >= 0; i--)
      {
        var (name, expr) = _pending[i];
        if (TryAdd(name, expr))
        {
          _pending.RemoveAt(i);
          resolved = true;
        }
      }
    }

    if (_pending.Count > 0)
    {
      var names = string.Join(", ", _pending.Select(p => p.Name));
      throw new InvalidOperationException($"Unresolved macros: {names}");
    }
  }

  public uint EvaluateUInt(string expression)
  {
    var value = EvaluateInternal(expression);
    return checked((uint)value);
  }

  public long EvaluateInt64(string expression)
  {
    var value = EvaluateInternal(expression);
    return unchecked((long)value);
  }

  public ulong EvaluateUInt64(string expression)
  {
    return EvaluateInternal(expression);
  }

  public void SetValue(string name, ulong value)
  {
    _values[name] = value;
  }

  private bool TryAdd(string name, string expression)
  {
    if (_values.ContainsKey(name))
      return true;

    try
    {
      var parser = new ExpressionParser(expression, _values);
      var value = parser.Evaluate();
      _values[name] = value;
      return true;
    }
    catch (FormatException ex)
    {
      throw new FormatException($"Failed to parse macro '{name}' expression '{expression}'", ex);
    }
    catch (UnknownIdentifierException)
    {
      return false;
    }
  }

  private ulong EvaluateInternal(string expression)
  {
    var parser = new ExpressionParser(expression, _values);
    return parser.Evaluate();
  }
}
