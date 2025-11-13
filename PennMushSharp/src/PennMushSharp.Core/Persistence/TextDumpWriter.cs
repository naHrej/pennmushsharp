using System.Text;

namespace PennMushSharp.Core.Persistence;

public sealed class TextDumpWriter
{
  public void Write(IEnumerable<GameObjectRecord> records, Stream stream)
  {
    using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024, leaveOpen: true);

    foreach (var record in records.OrderBy(r => r.DbRef))
    {
      writer.WriteLine($"!{record.DbRef}");
      WriteQuoted(writer, "name", record.Name ?? $"Player{record.DbRef}");
      writer.WriteLine($"owner #{record.Owner ?? record.DbRef}");
      writer.WriteLine("location #0");
      writer.WriteLine("contents #-1");
      writer.WriteLine("exits #0");
      writer.WriteLine("next #-1");
      writer.WriteLine("parent #-1");
      writer.WriteLine($"lockcount {record.Locks.Count}");
      foreach (var (lockName, expression) in record.Locks)
      {
        WriteLock(writer, lockName, expression);
      }

      writer.WriteLine($"attrcount {record.Attributes.Count}");
      foreach (var (attrName, value) in record.Attributes)
      {
        WriteAttribute(writer, attrName, value, record.Owner ?? record.DbRef);
      }
    }

    writer.WriteLine("***END OF DUMP***");
  }

  private static void WriteLock(StreamWriter writer, string lockName, string expression)
  {
    WriteQuoted(writer, " type", lockName);
    writer.WriteLine("  creator #1");
    writer.WriteLine("  flags \"\"");
    writer.WriteLine("  derefs 0");
    WriteQuoted(writer, "  key", expression);
  }

  private static void WriteAttribute(StreamWriter writer, string name, string value, int owner)
  {
    WriteQuoted(writer, " name", name);
    writer.WriteLine($"  owner #{owner}");
    writer.WriteLine("  flags \"\"");
    writer.WriteLine("  derefs 0");
    WriteQuoted(writer, "  value", value);
  }

  private static void WriteQuoted(StreamWriter writer, string prefix, string text)
  {
    var escaped = text
      .Replace("\\", "\\\\")
      .Replace("\"", "\\\"");
    writer.WriteLine($"{prefix} \"{escaped}\"");
  }
}
