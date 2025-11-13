using PennMushSharp.Core.Persistence;
using Xunit;

namespace PennMushSharp.Tests.Core;

public sealed class GameObjectRecordTests
{
  [Fact]
  public void SetAttribute_PopulatesDefaults()
  {
    var record = new GameObjectRecord { DbRef = 1, Owner = 2 };

    var attribute = record.SetAttribute("XYXXY", "value");

    Assert.Equal("value", attribute.Value);
    Assert.Equal(2, attribute.Owner);
    Assert.Same(attribute, record.Attributes["XYXXY"]);
  }

  [Fact]
  public void SetLock_PopulatesDefaults()
  {
    var record = new GameObjectRecord { DbRef = 1, Owner = 2 };

    var lockRecord = record.SetLock("Basic", "=#2");

    Assert.Equal("=#2", lockRecord.Key);
    Assert.Equal(2, lockRecord.Creator);
    Assert.Same(lockRecord, record.Locks["Basic"]);
  }
}
