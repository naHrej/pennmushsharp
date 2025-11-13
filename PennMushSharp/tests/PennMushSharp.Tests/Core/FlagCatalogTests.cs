using PennMushSharp.Core.Flags;
using Xunit;

namespace PennMushSharp.Tests.Core;

public sealed class FlagCatalogTests
{
  [Fact]
  public void FlagCatalog_LoadsEmbeddedSnapshot()
  {
    var catalog = FlagCatalog.Instance;

    Assert.Equal(56, catalog.Flags.Count);
    Assert.Equal(27, catalog.Powers.Count);

    Assert.True(catalog.TryGetFlag("HAVEN", out var haven));
    Assert.NotNull(haven);
    Assert.Equal('H', haven!.Letter);
    Assert.Equal(0x8U, haven.TypeMask);
    Assert.Equal(0x400L, haven.BitPosition);
  }

  [Fact]
  public void FlagCatalog_ResolvesByLetter()
  {
    var catalog = FlagCatalog.Instance;

    Assert.True(catalog.TryGetFlagByLetter('w', out var wizard));
    Assert.NotNull(wizard);
    Assert.Equal("WIZARD", wizard!.Name);
  }
}
