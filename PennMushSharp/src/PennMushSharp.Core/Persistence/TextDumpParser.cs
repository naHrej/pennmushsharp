using System.Text;

namespace PennMushSharp.Core.Persistence;

public sealed class TextDumpParser
{
  public IEnumerable<GameObjectRecord> Parse(Stream stream)
  {
    using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
    foreach (var record in Parse(reader))
      yield return record;
  }

  public IEnumerable<GameObjectRecord> Parse(TextReader reader)
  {
    GameObjectRecord? current = null;
    string? pendingLockName = null;
    string? pendingAttributeName = null;
    string? line;

    while ((line = reader.ReadLine()) is not null)
    {
      if (string.IsNullOrWhiteSpace(line))
        continue;

      var normalized = line.TrimEnd('\r', '\n');
      if (normalized.StartsWith("***END OF DUMP***", StringComparison.Ordinal))
      {
        if (current is not null)
          yield return current;
        yield break;
      }

      var indent = CountIndentation(normalized);
      var content = normalized[indent..];

      if (content.Length > 0 && content[0] == '!' && TryParseDbRef(content.AsSpan(1), out var newDbRef))
      {
        if (current is not null)
          yield return current;
        current = new GameObjectRecord { DbRef = newDbRef };
        pendingLockName = null;
        pendingAttributeName = null;
        continue;
      }

      if (content.StartsWith('#') && TryParseDbRef(content.AsSpan(1), out var legacyDbRef))
      {
        if (current is not null)
          yield return current;
        current = new GameObjectRecord { DbRef = legacyDbRef };
        pendingLockName = null;
        pendingAttributeName = null;
        continue;
      }

      if (current is null)
        continue;

      if (content.StartsWith("@lock/", StringComparison.OrdinalIgnoreCase))
      {
        ParseLegacyLock(content, current);
        continue;
      }

      if (indent == 0)
      {
        if (content.StartsWith("name ", StringComparison.Ordinal))
        {
          current.Name = ParseQuotedValue(content);
          continue;
        }

        if (content.StartsWith("owner ", StringComparison.Ordinal))
        {
          current.Owner = ParseNumber(content);
          continue;
        }

        if (content.StartsWith("flags ", StringComparison.Ordinal))
        {
          PopulateFlags(current, ParseQuotedValue(content));
          continue;
        }

        continue;
      }

      if (content.StartsWith("name ", StringComparison.Ordinal))
      {
        pendingAttributeName = ParseQuotedValue(content);
        continue;
      }

      if (pendingAttributeName is not null && content.StartsWith("value ", StringComparison.Ordinal))
      {
        var attrValue = ParseQuotedValue(content);
        current.Attributes[pendingAttributeName] = attrValue;
        pendingAttributeName = null;
        continue;
      }

      if (content.StartsWith("type \"", StringComparison.Ordinal))
      {
        pendingLockName = ParseQuotedValue(content);
        continue;
      }

      if (pendingLockName is not null && content.StartsWith("key ", StringComparison.Ordinal))
      {
        var expr = ParseQuotedValue(content);
        if (!string.IsNullOrWhiteSpace(pendingLockName))
          current.Locks[pendingLockName] = expr;
        pendingLockName = null;
        continue;
      }
    }

    if (current is not null)
      yield return current;
  }

  private static int CountIndentation(string line)
  {
    var count = 0;
    while (count < line.Length && (line[count] == ' ' || line[count] == '\t'))
      count++;
    return count;
  }

  private static bool TryParseDbRef(ReadOnlySpan<char> span, out int value)
  {
    span = span.Trim();
    if (span.Length > 0 && span[0] == '#')
      span = span[1..];
    return int.TryParse(span, out value);
  }

  private static int? ParseNumber(string content)
  {
    var index = content.IndexOf('#');
    string number;
    if (index >= 0)
      number = content[(index + 1)..];
    else
    {
      var space = content.IndexOf(' ');
      number = space >= 0 ? content[(space + 1)..] : content;
    }

    number = number.Trim();
    return int.TryParse(number, out var result) ? result : null;
  }

  private static void PopulateFlags(GameObjectRecord record, string rawFlags)
  {
    record.Flags.Clear();
    if (string.IsNullOrWhiteSpace(rawFlags))
      return;

    foreach (var flag in rawFlags.Split(' ', StringSplitOptions.RemoveEmptyEntries))
      record.Flags.Add(flag);
  }

  private static string ParseQuotedValue(string content)
  {
    var start = content.IndexOf('"');
    var end = content.LastIndexOf('"');
    if (start < 0 || end <= start)
      return string.Empty;
    return Unescape(content[(start + 1)..end]);
  }

  private static string Unescape(string value)
  {
    if (!value.Contains('\\'))
      return value;

    var builder = new StringBuilder(value.Length);
    for (var i = 0; i < value.Length; i++)
    {
      var current = value[i];
      if (current != '\\' || i + 1 >= value.Length)
      {
        builder.Append(current);
        continue;
      }

      var next = value[++i];
      builder.Append(next switch
      {
        '\\' => '\\',
        '"' => '"',
        'n' => '\n',
        'r' => '\r',
        't' => '\t',
        _ => next
      });
    }

    return builder.ToString();
  }

  private static void ParseLegacyLock(string content, GameObjectRecord record)
  {
    var separator = content.IndexOf(':');
    if (separator <= 0)
      return;

    var name = content.Substring(6, separator - 6).Trim();
    var expr = content[(separator + 1)..].Trim();
    if (!string.IsNullOrEmpty(name))
      record.Locks[name] = expr;
  }
}
