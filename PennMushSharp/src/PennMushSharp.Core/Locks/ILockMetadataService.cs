namespace PennMushSharp.Core.Locks;

public interface ILockMetadataService
{
  bool TryGetDefinition(string name, out LockDefinition? definition);
  IEnumerable<LockDefinition> GetAllDefinitions();
}
