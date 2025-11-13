namespace PennMushSharp.Core.Locks.Runtime;

public interface ILockExpressionEngine
{
  bool Evaluate(int playerDbRef, int thingDbRef, string expression);
}
