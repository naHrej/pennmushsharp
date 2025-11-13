using System.Text;
using System.Text.RegularExpressions;

namespace PennMushSharp.Extraction;

public static class InitializerParser
{
  public static IReadOnlyList<IReadOnlyList<string>> Parse(string source, string tableName, int expectedFields)
  {
    var block = ExtractBlock(source, tableName);
    var entries = new List<IReadOnlyList<string>>();
    var braceDepth = 0;
    var current = new StringBuilder();

    foreach (var ch in block)
    {
      if (ch == '{')
      {
        if (braceDepth == 0)
          current.Clear();
        braceDepth++;
        continue;
      }

      if (ch == '}')
      {
        braceDepth--;
        if (braceDepth == 0)
        {
          var fields = SplitFields(current.ToString(), expectedFields);
          entries.Add(fields);
        }
        continue;
      }

      if (braceDepth > 0)
        current.Append(ch);
    }

    return entries;
  }

  private static string ExtractBlock(string source, string tableName)
  {
    var pattern = new Regex($@"{Regex.Escape(tableName)}\s*\[\]\s*=\s*\{{", RegexOptions.Multiline);
    var match = pattern.Match(source);
    if (!match.Success)
      throw new InvalidOperationException($"Unable to locate initializer for {tableName}.");

    var startIndex = source.IndexOf('{', match.Index);
    if (startIndex < 0)
      throw new InvalidOperationException($"Malformed initializer for {tableName}.");

    var depth = 0;
    for (var i = startIndex; i < source.Length; i++)
    {
      var ch = source[i];
      if (ch == '{') depth++;
      else if (ch == '}') depth--;

      if (depth == 0)
      {
        return source.Substring(startIndex + 1, i - startIndex - 1);
      }
    }

    throw new InvalidOperationException($"Unterminated initializer for {tableName}.");
  }

  private static IReadOnlyList<string> SplitFields(string entry, int expectedFields)
  {
    var result = new List<string>();
    var sb = new StringBuilder();
    var inSingle = false;
    var inDouble = false;
    var escape = false;

    foreach (var ch in entry)
    {
      if (inSingle)
      {
        sb.Append(ch);
        if (escape)
        {
          escape = false;
        }
        else if (ch == '\\')
        {
          escape = true;
        }
        else if (ch == '\'')
        {
          inSingle = false;
        }
        continue;
      }

      if (inDouble)
      {
        sb.Append(ch);
        if (escape)
        {
          escape = false;
        }
        else if (ch == '\\')
        {
          escape = true;
        }
        else if (ch == '\"')
        {
          inDouble = false;
        }
        continue;
      }

      if (ch == '\'')
      {
        inSingle = true;
        sb.Append(ch);
        continue;
      }

      if (ch == '\"')
      {
        inDouble = true;
        sb.Append(ch);
        continue;
      }

      if (ch == ',')
      {
        result.Add(sb.ToString().Trim());
        sb.Clear();
        continue;
      }

      sb.Append(ch);
    }

    if (sb.Length > 0)
      result.Add(sb.ToString().Trim());

    if (result.Count != expectedFields)
      throw new InvalidOperationException($"Expected {expectedFields} fields but found {result.Count} in entry '{entry}'.");

    return result;
  }
}
