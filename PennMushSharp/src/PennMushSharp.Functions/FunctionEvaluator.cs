using PennMushSharp.Core;

namespace PennMushSharp.Functions;

public sealed class FunctionEvaluator : IFunctionEvaluator
{
  private readonly FunctionRegistry _registry;

  public FunctionEvaluator(FunctionRegistry registry)
  {
    _registry = registry;
  }

  public ValueTask<string> EvaluateAsync(GameObject actor, string expression, CancellationToken cancellationToken = default)
  {
    // Placeholder implementation: real evaluator will parse nested functions.
    return ValueTask.FromResult(expression);
  }
}
