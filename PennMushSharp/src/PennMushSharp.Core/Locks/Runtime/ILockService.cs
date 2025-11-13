namespace PennMushSharp.Core.Locks.Runtime;

public interface ILockService
{
  bool Evaluate(LockRequest request);
}
