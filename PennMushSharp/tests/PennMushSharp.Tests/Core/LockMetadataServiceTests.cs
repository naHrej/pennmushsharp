using PennMushSharp.Core.Locks;
using Xunit;

namespace PennMushSharp.Tests.Core;

public sealed class LockMetadataServiceTests
{
  [Fact]
  public void LockMetadataService_ReturnsDefaultCatalogData()
  {
    var service = LockMetadataService.CreateDefault();

    Assert.True(service.TryGetDefinition("Control", out var control));
    Assert.NotNull(control);
    Assert.Equal("Control", control!.Name);

    var all = service.GetAllDefinitions().ToList();
    Assert.True(all.Count >= 30);
  }

  [Fact]
  public void LockMetadataService_ReturnsFalseForUnknown()
  {
    var service = LockMetadataService.CreateDefault();

    Assert.False(service.TryGetDefinition("DoesNotExist", out var definition));
    Assert.Null(definition);
  }
}
