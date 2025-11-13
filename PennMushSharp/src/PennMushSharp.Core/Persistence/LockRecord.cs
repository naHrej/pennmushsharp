namespace PennMushSharp.Core.Persistence;

public sealed class LockRecord
{
  public LockRecord(string name)
  {
    Name = name ?? throw new ArgumentNullException(nameof(name));
  }

  public string Name { get; }
  public int Creator { get; set; }
  public string Flags { get; set; } = string.Empty;
  public int Derefs { get; set; }
  public string Key { get; set; } = string.Empty;
}
