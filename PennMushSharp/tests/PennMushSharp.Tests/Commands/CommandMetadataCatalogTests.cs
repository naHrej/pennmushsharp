using PennMushSharp.Commands.Metadata;
using Xunit;

namespace PennMushSharp.Tests.Commands;

public sealed class CommandMetadataCatalogTests
{
  [Fact]
  public void TryGet_KnowsAboutBuiltinCommand()
  {
    var result = CommandMetadataCatalog.TryGet("LOOK", out var definition);

    Assert.True(result);
    Assert.NotNull(definition);
    Assert.Equal("LOOK", definition!.Name);
    Assert.False(definition.WizardOnly);
    Assert.NotEmpty(definition.Switches);
  }
}
