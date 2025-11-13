using System.Linq;
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
      WriteQuoted(writer, "name", record.Name ?? $"#{record.DbRef}");
      WriteDbRef(writer, "owner", record.Owner ?? record.DbRef);
      WriteDbRef(writer, "location", record.Location ?? -1);
      WriteDbRef(writer, "contents", record.Contents ?? -1);
      WriteDbRef(writer, "exits", record.Exits ?? -1);
      WriteDbRef(writer, "next", record.Next ?? -1);
      WriteDbRef(writer, "parent", record.Parent ?? -1);
      WriteDbRef(writer, "home", record.Home ?? -1);
      WriteDbRef(writer, "zone", record.Zone ?? -1);
      WriteDbRef(writer, "dropto", record.Dropto ?? -1);
      writer.WriteLine($"type {record.Type.ToPennMushCode()}");
      writer.WriteLine($"pennies {record.Pennies ?? 0}");
      WriteQuoted(writer, "flags", string.Join(' ', record.Flags));

      writer.WriteLine($"lockcount {record.Locks.Count}");
      foreach (var lockRecord in record.Locks.Values.OrderBy(l => l.Name, StringComparer.OrdinalIgnoreCase))
        WriteLock(writer, lockRecord);

      writer.WriteLine($"attrcount {record.Attributes.Count}");
      foreach (var attribute in record.Attributes.Values.OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase))
        WriteAttribute(writer, attribute);
    }

    writer.WriteLine("***END OF DUMP***");
  }

  private static void WriteLock(StreamWriter writer, LockRecord lockRecord)
  {
    WriteQuoted(writer, " type", lockRecord.Name);
    WriteDbRef(writer, "  creator", lockRecord.Creator);
    WriteQuoted(writer, "  flags", lockRecord.Flags);
    writer.WriteLine($"  derefs {lockRecord.Derefs}");
    WriteQuoted(writer, "  key", lockRecord.Key);
  }

  private static void WriteAttribute(StreamWriter writer, AttributeRecord attribute)
  {
    WriteQuoted(writer, " name", attribute.Name);
    WriteDbRef(writer, "  owner", attribute.Owner);
    WriteQuoted(writer, "  flags", attribute.Flags);
    writer.WriteLine($"  derefs {attribute.Derefs}");
    WriteQuoted(writer, "  value", attribute.Value);
  }

  private static void WriteDbRef(StreamWriter writer, string label, int value)
  {
    writer.WriteLine($"{label} #{value}");
  }

  private static void WriteQuoted(StreamWriter writer, string prefix, string text)
  {
    text ??= string.Empty;
    var escaped = text
      .Replace("\\", "\\\\")
      .Replace("\"", "\\\"");
    writer.WriteLine($"{prefix} \"{escaped}\"");
  }
}
