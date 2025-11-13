using PennMushSharp.Core;
using Xunit;

namespace PennMushSharp.Tests.Core;

public sealed class GameObjectTests
{
  [Fact]
  public void GameObject_StoresBasicState()
  {
    var room = new GameObject(1, "The Void");
    Assert.Equal(1, room.DbRef);
    Assert.Equal("The Void", room.Name);
  }
}
