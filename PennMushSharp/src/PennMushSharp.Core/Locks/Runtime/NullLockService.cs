namespace PennMushSharp.Core.Locks.Runtime;

public sealed class NullLockService : ILockService
{
  public static NullLockService Instance { get; } = new();

  private NullLockService() { }

  public bool Evaluate(LockRequest request) => true;
}
