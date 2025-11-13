namespace PennMushSharp.Core.Persistence;

public sealed class AttributeRecord
{
  public AttributeRecord(string name)
  {
    Name = name ?? throw new ArgumentNullException(nameof(name));
  }

  public string Name { get; }
  public int Owner { get; set; }
  public string Flags { get; set; } = string.Empty;
  public int Derefs { get; set; }
  public string Value { get; set; } = string.Empty;
}
