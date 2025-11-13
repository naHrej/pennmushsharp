using PennMushSharp.Core;
using PennMushSharp.Core.Persistence;
using Xunit;

namespace PennMushSharp.Tests.Core;

public sealed class GameObjectTests
{
  [Fact]
  public void FromRecord_MapsFields()
  {
    var record = new GameObjectRecord
    {
      DbRef = 1,
      Name = "The Void",
      Type = GameObjectType.Room,
      Owner = 9,
      Location = -1
    };
    record.Flags.Add("ROOM");
    record.SetAttribute("DESC", "A dark space.");
    record.SetLock("Control", "#9");

    var snapshot = GameObject.FromRecord(record);

    Assert.Equal(1, snapshot.DbRef);
    Assert.Equal("The Void", snapshot.Name);
    Assert.Equal(GameObjectType.Room, snapshot.Type);
    Assert.Equal(9, snapshot.Owner);
    Assert.Equal(-1, snapshot.Location);
    Assert.Contains("ROOM", snapshot.Flags);
    Assert.Equal("A dark space.", snapshot.Attributes["DESC"]);
    Assert.Equal("#9", snapshot.Locks["Control"]);
  }
}
