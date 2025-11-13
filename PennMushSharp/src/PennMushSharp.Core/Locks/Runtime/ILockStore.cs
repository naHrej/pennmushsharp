namespace PennMushSharp.Core.Locks.Runtime;

public interface ILockStore
{
  bool TryGet(int thingDbRef, string lockName, out StoredLock storedLock);
}

public interface IMutableLockStore : ILockStore
{
  void SetLock(int thingDbRef, StoredLock storedLock);
  bool RemoveLock(int thingDbRef, string lockName);
}

public readonly record struct StoredLock(string LockName, string Expression);
