using System.Collections.Concurrent;

namespace PennMushSharp.Core.Locks.Runtime;

public sealed class InMemoryLockStore : IMutableLockStore
{
  private readonly ConcurrentDictionary<LockKey, StoredLock> _locks = new(new LockKeyComparer());

  public bool TryGet(int thingDbRef, string lockName, out StoredLock storedLock)
  {
    if (string.IsNullOrWhiteSpace(lockName))
    {
      storedLock = default;
      return false;
    }

    return _locks.TryGetValue(new LockKey(thingDbRef, lockName), out storedLock);
  }

  public void SetLock(int thingDbRef, StoredLock storedLock)
  {
    if (string.IsNullOrWhiteSpace(storedLock.LockName))
      throw new ArgumentException("Lock name is required", nameof(storedLock));

    _locks[new LockKey(thingDbRef, storedLock.LockName)] = storedLock;
  }

  public bool RemoveLock(int thingDbRef, string lockName)
  {
    return _locks.TryRemove(new LockKey(thingDbRef, lockName), out _);
  }

  private readonly record struct LockKey(int Thing, string LockName);

  private sealed class LockKeyComparer : IEqualityComparer<LockKey>
  {
    private static readonly StringComparer NameComparer = StringComparer.OrdinalIgnoreCase;

    public bool Equals(LockKey x, LockKey y)
    {
      return x.Thing == y.Thing && NameComparer.Equals(x.LockName, y.LockName);
    }

    public int GetHashCode(LockKey obj)
    {
      return HashCode.Combine(obj.Thing, NameComparer.GetHashCode(obj.LockName));
    }
  }
}
