using PennMushSharp.Core.Locks;
using Xunit;

namespace PennMushSharp.Tests.Core;

public sealed class LockCatalogTests
{
  [Fact]
  public void LockCatalog_LoadsSnapshot()
  {
    var catalog = LockCatalog.Instance;

    Assert.Equal(33, catalog.Locks.Count);
    Assert.Equal(5, catalog.Privileges.Count);

    Assert.True(catalog.TryGetLock("Basic", out var basic));
    Assert.NotNull(basic);
    Assert.Equal("TRUE_BOOLEXP", basic!.DefaultKeyExpression);
    Assert.Equal(1, basic.DefaultCreator);
  }
}
