using PennMushSharp.Core;

namespace PennMushSharp.Functions;

public interface IFunctionEvaluator
{
  ValueTask<string> EvaluateAsync(GameObject actor, string expression, CancellationToken cancellationToken = default);
}
