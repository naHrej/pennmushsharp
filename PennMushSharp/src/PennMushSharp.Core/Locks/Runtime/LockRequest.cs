namespace PennMushSharp.Core.Locks.Runtime;

public readonly record struct LockRequest(int PlayerDbRef, int ThingDbRef, string LockName);
