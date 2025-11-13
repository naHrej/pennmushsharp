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
    LockRecord? pendingLock = null;
    AttributeRecord? pendingAttribute = null;
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

      if (normalized.StartsWith("***END RECORD***", StringComparison.Ordinal))
      {
        if (current is not null)
        {
          yield return current;
          current = null;
          pendingAttribute = null;
          pendingLock = null;
        }
        continue;
      }

      var indent = CountIndentation(normalized);
      var content = normalized[indent..];

      if (TryStartRecord(content, out var dbRef))
      {
        if (current is not null)
          yield return current;
        current = new GameObjectRecord { DbRef = dbRef };
        pendingAttribute = null;
        pendingLock = null;
        continue;
      }

      if (current is null)
        continue;

      if (content.StartsWith("@lock/", StringComparison.OrdinalIgnoreCase))
      {
        ParseLegacyLock(content, current);
        continue;
      }

      if (indent == 0 && HandleTopLevelLine(current, content))
        continue;

      if (indent > 0 && content.StartsWith("type ", StringComparison.Ordinal) && content.Contains('"'))
      {
        pendingLock = new LockRecord(ParseQuotedValue(content))
        {
          Creator = current.Owner ?? current.DbRef
        };
        continue;
      }

      if (pendingLock is not null && HandleStructuredLockLine(pendingLock, current, content))
      {
        if (content.StartsWith("key ", StringComparison.Ordinal))
          pendingLock = null;
        continue;
      }

      if (indent > 0 && content.StartsWith("name ", StringComparison.Ordinal))
      {
        pendingAttribute = new AttributeRecord(ParseQuotedValue(content))
        {
          Owner = current.Owner ?? current.DbRef
        };
        continue;
      }

      if (pendingAttribute is not null && HandleAttributeLine(pendingAttribute, current, content))
      {
        if (content.StartsWith("value ", StringComparison.Ordinal))
          pendingAttribute = null;
        continue;
      }
    }

    if (current is not null)
      yield return current;
  }

  private static bool HandleTopLevelLine(GameObjectRecord record, string content)
  {
    if (content.StartsWith("name ", StringComparison.Ordinal))
    {
      record.Name = ParseQuotedValue(content);
      return true;
    }

    if (content.StartsWith("owner ", StringComparison.Ordinal))
    {
      record.Owner = ParseNumber(content);
      return true;
    }

    if (content.StartsWith("location ", StringComparison.Ordinal))
    {
      record.Location = ParseNumber(content);
      return true;
    }

    if (content.StartsWith("contents ", StringComparison.Ordinal))
    {
      record.Contents = ParseNumber(content);
      return true;
    }

    if (content.StartsWith("exits ", StringComparison.Ordinal))
    {
      record.Exits = ParseNumber(content);
      return true;
    }

    if (content.StartsWith("next ", StringComparison.Ordinal))
    {
      record.Next = ParseNumber(content);
      return true;
    }

    if (content.StartsWith("parent ", StringComparison.Ordinal))
    {
      record.Parent = ParseNumber(content);
      return true;
    }

    if (content.StartsWith("home ", StringComparison.Ordinal))
    {
      record.Home = ParseNumber(content);
      return true;
    }

    if (content.StartsWith("zone ", StringComparison.Ordinal))
    {
      record.Zone = ParseNumber(content);
      return true;
    }

    if (content.StartsWith("dropto ", StringComparison.Ordinal))
    {
      record.Dropto = ParseNumber(content);
      return true;
    }

    if (content.StartsWith("pennies ", StringComparison.Ordinal))
    {
      record.Pennies = ParseNumber(content);
      return true;
    }

    if (content.StartsWith("type ", StringComparison.Ordinal))
    {
      var typeCode = ParseNumber(content);
      if (typeCode is not null)
        record.Type = GameObjectTypeExtensions.FromPennMushCode(typeCode.Value);
      return true;
    }

    if (content.StartsWith("flags ", StringComparison.Ordinal))
    {
      PopulateFlags(record, ParseQuotedValue(content));
      return true;
    }

    if (content.StartsWith("lockcount ", StringComparison.Ordinal) ||
        content.StartsWith("attrcount ", StringComparison.Ordinal))
    {
      return true;
    }

    return false;
  }

  private static bool HandleStructuredLockLine(LockRecord pendingLock, GameObjectRecord record, string content)
  {
    if (content.StartsWith("creator ", StringComparison.Ordinal))
    {
      pendingLock.Creator = ParseNumber(content) ?? pendingLock.Creator;
      return true;
    }

    if (content.StartsWith("flags ", StringComparison.Ordinal))
    {
      pendingLock.Flags = ParseQuotedValue(content);
      return true;
    }

    if (content.StartsWith("derefs ", StringComparison.Ordinal))
    {
      pendingLock.Derefs = ParseNumber(content) ?? pendingLock.Derefs;
      return true;
    }

    if (content.StartsWith("key ", StringComparison.Ordinal))
    {
      pendingLock.Key = ParseQuotedValue(content);
      record.Locks[pendingLock.Name] = pendingLock;
      return true;
    }

    return false;
  }

  private static bool HandleAttributeLine(AttributeRecord attribute, GameObjectRecord record, string content)
  {
    if (content.StartsWith("owner ", StringComparison.Ordinal))
    {
      attribute.Owner = ParseNumber(content) ?? attribute.Owner;
      return true;
    }

    if (content.StartsWith("flags ", StringComparison.Ordinal))
    {
      attribute.Flags = ParseQuotedValue(content);
      return true;
    }

    if (content.StartsWith("derefs ", StringComparison.Ordinal))
    {
      attribute.Derefs = ParseNumber(content) ?? attribute.Derefs;
      return true;
    }

    if (content.StartsWith("value ", StringComparison.Ordinal))
    {
      attribute.Value = ParseQuotedValue(content);
      record.Attributes[attribute.Name] = attribute;
      return true;
    }

    return false;
  }

  private static bool TryStartRecord(string content, out int dbRef)
  {
    dbRef = 0;
    if (content.Length == 0)
      return false;

    if (content[0] == '!' && TryParseDbRef(content.AsSpan(1), out dbRef))
      return true;

    if (content[0] == '#' && TryParseDbRef(content.AsSpan(1), out dbRef))
      return true;

    return false;
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
      record.SetLock(name, expr);
  }
}
