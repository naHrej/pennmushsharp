using PennMushSharp.Core.Attributes;
using PennMushSharp.Core.Flags;
using PennMushSharp.Core.Functions;
using PennMushSharp.Core.Locks;

namespace PennMushSharp.Core.Metadata;

public interface IMetadataCatalogs
{
  FlagCatalog Flags { get; }
  AttributeCatalog Attributes { get; }
  LockCatalog Locks { get; }
  FunctionCatalog Functions { get; }
}

public sealed class MetadataCatalogs : IMetadataCatalogs
{
  public static MetadataCatalogs Default { get; } = new();

  private MetadataCatalogs()
  {
  }

  public FlagCatalog Flags => FlagCatalog.Instance;
  public AttributeCatalog Attributes => AttributeCatalog.Instance;
  public LockCatalog Locks => LockCatalog.Instance;
  public FunctionCatalog Functions => FunctionCatalog.Instance;
}
