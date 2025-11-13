using PennMushSharp.Core.Locks.Runtime;

namespace PennMushSharp.Core.Persistence;

public sealed class InMemoryGameState
{
  private readonly IMutableLockStore _lockStore;
  private readonly Dictionary<int, GameObjectRecord> _objects = new();
  private readonly Dictionary<string, GameObjectRecord> _objectsByName = new(StringComparer.OrdinalIgnoreCase);
  private int _nextDbRef = 1;

  public InMemoryGameState(IMutableLockStore lockStore)
  {
    _lockStore = lockStore;
  }

  public void Upsert(GameObjectRecord record)
  {
    if (_objects.TryGetValue(record.DbRef, out var previous) && !string.IsNullOrWhiteSpace(previous.Name))
    {
      _objectsByName.Remove(previous.Name);
    }

    if (record.Owner is null)
      record.Owner = record.DbRef;

    _objects[record.DbRef] = record;
    if (!string.IsNullOrWhiteSpace(record.Name))
    {
      _objectsByName[record.Name] = record;
    }

    foreach (var lockRecord in record.Locks.Values)
    {
      _lockStore.SetLock(record.DbRef, new StoredLock(lockRecord.Name, lockRecord.Key));
    }

    if (record.DbRef >= _nextDbRef)
      _nextDbRef = record.DbRef + 1;
  }

  public bool TryGet(int dbRef, out GameObjectRecord? record)
  {
    return _objects.TryGetValue(dbRef, out record);
  }

  public int Count => _objects.Count;

  public bool TryGet(string name, out GameObjectRecord? record)
  {
    return _objectsByName.TryGetValue(name, out record);
  }

  public GameObjectRecord Allocate(string name, string hashedPassword)
  {
    var record = new GameObjectRecord
    {
      DbRef = _nextDbRef++,
      Name = name,
      Owner = null
    };
    record.SetAttribute("XYXXY", hashedPassword, record.DbRef);
    Upsert(record);
    return record;
  }
}
