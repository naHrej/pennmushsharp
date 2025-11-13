using System.Text;

namespace PennMushSharp.Extraction;

public static class LiteralParser
{
  public static string ParseString(string token)
  {
    token = token.Trim();
    if (!token.StartsWith('"') || !token.EndsWith('"'))
      throw new FormatException($"Expected string literal but saw '{token}'.");

    var inner = token[1..^1];
    return DecodeEscapes(inner);
  }

  public static char? ParseChar(string token)
  {
    token = token.Trim();
    if (token is "0" or "'\\0'" or "'\0'")
      return '\0';
    if (string.Equals(token, "NULL", StringComparison.Ordinal))
      return null;

    if (!token.StartsWith('\'') || !token.EndsWith('\''))
      throw new FormatException($"Expected character literal but saw '{token}'.");

    var inner = token[1..^1];
    return DecodeChar(inner);
  }

  internal static char ParseCharLiteralToken(string expression, ref int index)
  {
    if (expression[index] != '\'')
      throw new FormatException("Expected char literal.");

    index++; // skip opening '
    var sb = new StringBuilder();
    var escaped = false;
    while (index < expression.Length)
    {
      var c = expression[index++];
      if (escaped)
      {
        sb.Append(c);
        escaped = false;
        continue;
      }

      if (c == '\\')
      {
        escaped = true;
        continue;
      }

      if (c == '\'')
        break;

      sb.Append(c);
    }

    return DecodeChar(sb.ToString());
  }

  private static char DecodeChar(string value)
  {
    if (value.Length == 0)
      return '\0';

    if (value[0] == '\\')
    {
      if (value.Length == 1)
        return '\\';

      return value[1] switch
      {
        '\\' => '\\',
        '\'' => '\'',
        '"' => '"',
        '0' => '\0',
        'n' => '\n',
        'r' => '\r',
        't' => '\t',
        'x' => (char)Convert.ToInt32(value[2..], 16),
        _ when char.IsDigit(value[1]) => (char)Convert.ToInt32(value[1..], 8),
        _ => value[1]
      };
    }

    return value[0];
  }

  private static string DecodeEscapes(string value)
  {
    var sb = new StringBuilder();
    var escaped = false;
    foreach (var c in value)
    {
      if (!escaped)
      {
        if (c == '\\')
        {
          escaped = true;
        }
        else
        {
          sb.Append(c);
        }
      }
      else
      {
        switch (c)
        {
          case '\\':
            sb.Append('\\');
            break;
          case '\"':
            sb.Append('"');
            break;
          case '\'':
            sb.Append('\'');
            break;
          case 'n':
            sb.Append('\n');
            break;
          case 'r':
            sb.Append('\r');
            break;
          case 't':
            sb.Append('\t');
            break;
          case '0':
            sb.Append('\0');
            break;
          default:
            sb.Append(c);
            break;
        }
        escaped = false;
      }
    }

    if (escaped)
      sb.Append('\\');

    return sb.ToString();
  }
}
