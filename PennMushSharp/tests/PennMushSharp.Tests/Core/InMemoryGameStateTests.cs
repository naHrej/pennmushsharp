using PennMushSharp.Core.Locks.Runtime;
using PennMushSharp.Core.Persistence;
using Xunit;

namespace PennMushSharp.Tests.Core;

public sealed class InMemoryGameStateTests
{
  [Fact]
  public void Upsert_PopulatesLockStore()
  {
    var lockStore = new InMemoryLockStore();
    var state = new InMemoryGameState(lockStore);

    var record = new GameObjectRecord { DbRef = 5 };
    record.SetLock("Control", "#5");

    state.Upsert(record);

    Assert.True(lockStore.TryGet(5, "Control", out var stored));
    Assert.Equal("#5", stored.Expression);
  }
}
