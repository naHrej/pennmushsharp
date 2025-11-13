using PennMushSharp.Core;

namespace PennMushSharp.Functions;

public interface IFunctionEvaluator
{
  ValueTask<string> EvaluateAsync(FunctionExecutionContext context, string expression, CancellationToken cancellationToken = default);
}
