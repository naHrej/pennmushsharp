using PennMushSharp.Core.Attributes;
using Xunit;

namespace PennMushSharp.Tests.Core;

public sealed class AttributeCatalogTests
{
  [Fact]
  public void AttributeCatalog_LoadsEmbeddedSnapshot()
  {
    var catalog = AttributeCatalog.Instance;

    Assert.True(catalog.Attributes.Count > 0);
    Assert.True(catalog.Aliases.Count > 0);

    Assert.True(catalog.TryGetAttribute("DESCRIBE", out var describe));
    Assert.NotNull(describe);
    Assert.NotEqual(0U, describe!.Flags);
  }

  [Fact]
  public void AttributeCatalog_ResolvesAliases()
  {
    var catalog = AttributeCatalog.Instance;

    Assert.True(catalog.TryGetAttribute("DESC", out var describe));
    Assert.NotNull(describe);
    Assert.Equal("DESCRIBE", describe!.Name);

    Assert.True(catalog.TryResolveAlias("DESC", out var alias));
    Assert.NotNull(alias);
    Assert.Equal("DESCRIBE", alias!.RealName);
  }
}
