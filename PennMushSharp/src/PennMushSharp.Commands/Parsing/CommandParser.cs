using System.Linq;
using System.Text;

namespace PennMushSharp.Commands.Parsing;

public sealed class CommandParser
{
  public IReadOnlyList<CommandInvocation> Parse(string? input)
  {
    if (string.IsNullOrWhiteSpace(input))
      return Array.Empty<CommandInvocation>();

    var trimmed = input.TrimStart();
    if (TryParseShorthand(trimmed, out var shorthandInvocation))
      return new[] { shorthandInvocation };

    var segments = SplitSegments(input);
    var results = new List<CommandInvocation>(segments.Count);
    foreach (var segment in segments)
    {
      if (TryParseSegment(segment, out var invocation))
        results.Add(invocation);
    }

    return results;
  }

  private static bool TryParseSegment(string segment, out CommandInvocation invocation)
  {
    invocation = default!;
    var raw = segment.Trim();
    if (raw.Length == 0)
      return false;

    if (TryParseShorthand(raw, out invocation))
      return true;

    var index = 0;
    var name = ReadToken(raw, ref index, stopChars: new[] { ' ', '\t', '/', '=' });
    if (string.IsNullOrEmpty(name))
      return false;

    var switches = new List<CommandSwitch>();
    while (index < raw.Length && raw[index] == '/')
    {
      index++;
      var switchToken = ReadToken(raw, ref index, new[] { ' ', '\t', '/', '=' });
      if (string.IsNullOrEmpty(switchToken))
        continue;

      string? switchArgument = null;
      var colon = switchToken.IndexOf(':');
      if (colon >= 0)
      {
        switchArgument = switchToken[(colon + 1)..];
        switchToken = switchToken[..colon];
      }

      switches.Add(new CommandSwitch(switchToken, switchArgument));
    }

    SkipWhitespace(raw, ref index);

    string? target = null;
    string? argument = null;

    if (index < raw.Length)
    {
      var remainder = raw[index..];
      var equalsIndex = remainder.IndexOf('=');
      if (equalsIndex >= 0)
      {
        target = remainder[..equalsIndex].Trim();
        argument = remainder[(equalsIndex + 1)..].Trim();
      }
      else
      {
        argument = remainder.Trim();
      }
    }

    invocation = new CommandInvocation(name, switches, target, argument, raw);
    return true;
  }

  private static bool TryParseShorthand(string raw, out CommandInvocation invocation)
  {
    invocation = default!;
    var leader = raw[0];
    string commandName;
    var preserveLeadingWhitespace = false;

    switch (leader)
    {
      case '\'':
      case '"':
        commandName = "SAY";
        break;
      case ':':
        commandName = "POSE";
        break;
      case ';':
        commandName = "SEMIPOSE";
        preserveLeadingWhitespace = true;
        break;
      case '\\':
        commandName = "@EMIT";
        break;
      default:
        return false;
    }

    var remainder = raw.Length > 1 ? raw[1..] : string.Empty;
    if (!preserveLeadingWhitespace)
      remainder = remainder.TrimStart();

    var argument = string.IsNullOrWhiteSpace(remainder) ? null : remainder;
    invocation = new CommandInvocation(commandName, Array.Empty<CommandSwitch>(), target: null, argument, raw);
    return true;
  }

  private static void SkipWhitespace(string text, ref int index)
  {
    while (index < text.Length && char.IsWhiteSpace(text[index]))
      index++;
  }

  private static string ReadToken(string text, ref int index, IReadOnlyCollection<char> stopChars)
  {
    var start = index;
    while (index < text.Length && !stopChars.Contains(text[index]))
      index++;
    return text[start..index];
  }

  private static List<string> SplitSegments(string input)
  {
    var builder = new StringBuilder();
    var segments = new List<string>();
    var inQuotes = false;
    var braceDepth = 0;
    var escape = false;

    foreach (var ch in input)
    {
      if (escape)
      {
        if (ch != ';' && ch != '&')
          builder.Append('\\');
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
        inQuotes = !inQuotes;
      else if (ch == '{')
        braceDepth++;
      else if (ch == '}' && braceDepth > 0)
        braceDepth--;

      if ((ch == ';' || ch == '&') && !inQuotes && braceDepth == 0)
      {
        var segment = builder.ToString().Trim();
        if (segment.Length > 0)
          segments.Add(segment);
        builder.Clear();
        continue;
      }

      builder.Append(ch);
    }

    if (escape)
      builder.Append('\\');

    var last = builder.ToString().Trim();
    if (last.Length > 0)
      segments.Add(last);
    return segments;
  }
}
