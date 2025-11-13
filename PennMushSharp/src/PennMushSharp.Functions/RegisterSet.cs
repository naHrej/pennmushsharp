using System.Collections.Generic;
using System.Text;

namespace PennMushSharp.Functions;

public sealed class RegisterSet
{
  private readonly Dictionary<string, string> _named = new(StringComparer.OrdinalIgnoreCase);
  private readonly Dictionary<int, string> _arguments = new();

  public string? GetNamed(string name) => _named.TryGetValue(name, out var value) ? value : null;

  public void SetNamed(string name, string value) => _named[name] = value;

  public string? GetArgument(int index) => _arguments.TryGetValue(index, out var value) ? value : null;

  public void LoadArguments(string? rawArguments)
  {
    _arguments.Clear();
    if (string.IsNullOrEmpty(rawArguments))
      return;

    var tokens = Tokenize(rawArguments);
    for (var i = 0; i < tokens.Count; i++)
      _arguments[i] = tokens[i];
  }

  public void ClearAll()
  {
    _named.Clear();
    _arguments.Clear();
  }

  private static IReadOnlyList<string> Tokenize(string text)
  {
    var results = new List<string>();
    var builder = new StringBuilder();
    var inQuotes = false;
    var braceDepth = 0;
    var escape = false;

    void Commit()
    {
      if (builder.Length == 0)
        return;
      results.Add(builder.ToString());
      builder.Clear();
    }

    foreach (var ch in text)
    {
      if (escape)
      {
        builder.Append(ch);
        escape = false;
        continue;
      }

      if (ch == '\\')
      {
        escape = true;
        continue;
      }

      if (ch == '"')
      {
        inQuotes = !inQuotes;
        continue;
      }

      if (ch == '{')
      {
        braceDepth++;
        continue;
      }

      if (ch == '}' && braceDepth > 0)
      {
        braceDepth--;
        continue;
      }

      if ((char.IsWhiteSpace(ch) || ch == ',') && !inQuotes && braceDepth == 0)
      {
        Commit();
        continue;
      }

      builder.Append(ch);
    }

    Commit();
    return results;
  }
}
