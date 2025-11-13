using PennMushSharp.Core.Metadata;

namespace PennMushSharp.Core.Locks;

public sealed class LockMetadataService : ILockMetadataService
{
  private readonly LockCatalog _catalog;

  public LockMetadataService(ILockCatalogProvider provider)
  {
    _catalog = provider.LockCatalog ?? throw new ArgumentNullException(nameof(provider));
  }

  public static LockMetadataService CreateDefault()
    => new LockMetadataService(new DefaultLockCatalogProvider());

  public bool TryGetDefinition(string name, out LockDefinition? definition)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      definition = null;
      return false;
    }

    return _catalog.TryGetLock(name, out definition);
  }

  public IEnumerable<LockDefinition> GetAllDefinitions() => _catalog.Locks;
}

public interface ILockCatalogProvider
{
  LockCatalog LockCatalog { get; }
}

file sealed class DefaultLockCatalogProvider : ILockCatalogProvider
{
  public LockCatalog LockCatalog => MetadataCatalogs.Default.Locks;
}
