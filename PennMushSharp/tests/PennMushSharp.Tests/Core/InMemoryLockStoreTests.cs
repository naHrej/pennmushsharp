using PennMushSharp.Core.Locks.Runtime;
using Xunit;

namespace PennMushSharp.Tests.Core;

public sealed class InMemoryLockStoreTests
{
  [Fact]
  public void SetAndGetLock()
  {
    IMutableLockStore store = new InMemoryLockStore();

    store.SetLock(2, new StoredLock("Control", "#5"));

    Assert.True(store.TryGet(2, "Control", out var stored));
    Assert.Equal("#5", stored.Expression);
  }

  [Fact]
  public void RemoveLock()
  {
    IMutableLockStore store = new InMemoryLockStore();
    store.SetLock(2, new StoredLock("Control", "#5"));

    Assert.True(store.RemoveLock(2, "Control"));
    Assert.False(store.TryGet(2, "Control", out _));
  }
}
