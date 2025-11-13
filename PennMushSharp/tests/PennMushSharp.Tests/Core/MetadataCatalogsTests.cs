using PennMushSharp.Core.Metadata;
using Xunit;

namespace PennMushSharp.Tests.Core;

public sealed class MetadataCatalogsTests
{
  [Fact]
  public void MetadataCatalogs_ExposeSingletons()
  {
    IMetadataCatalogs catalogs = MetadataCatalogs.Default;

    Assert.NotNull(catalogs.Flags);
    Assert.NotNull(catalogs.Attributes);
    Assert.NotNull(catalogs.Locks);
    Assert.NotNull(catalogs.Functions);

    Assert.Same(catalogs.Flags, MetadataCatalogs.Default.Flags);
    Assert.Same(catalogs.Attributes, MetadataCatalogs.Default.Attributes);
    Assert.Same(catalogs.Locks, MetadataCatalogs.Default.Locks);
    Assert.Same(catalogs.Functions, MetadataCatalogs.Default.Functions);
  }
}
