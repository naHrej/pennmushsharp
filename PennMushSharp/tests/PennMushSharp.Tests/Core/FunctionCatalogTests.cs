using PennMushSharp.Core.Functions;
using Xunit;

namespace PennMushSharp.Tests.Core;

public sealed class FunctionCatalogTests
{
  [Fact]
  public void FunctionCatalog_ResolvesBuiltins()
  {
    var catalog = FunctionCatalog.Instance;

    Assert.True(catalog.Functions.Count > 0);

    Assert.True(catalog.TryGetFunction("ADD", out var add));
    Assert.NotNull(add);
    Assert.Equal(2, add!.MinArgs);
    Assert.Equal(int.MaxValue, add.MaxArgs);
    Assert.NotEqual(0U, add.Flags);
  }

  [Fact]
  public void FunctionCatalog_ResolvesAliases()
  {
    var catalog = FunctionCatalog.Instance;

    Assert.True(catalog.TryGetFunction("U", out var ufun));
    Assert.NotNull(ufun);
    Assert.Equal("UFUN", ufun!.Name);

    Assert.True(catalog.TryResolveAlias("U", out var alias));
    Assert.NotNull(alias);
    Assert.Equal("UFUN", alias!.FunctionName);
  }
}
