using System.Collections.Generic;

namespace PennMushSharp.Functions.Builtins;

public sealed class MaxFunction : IFunction
{
  public string Name => "MAX";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    if (arguments.Count == 0)
      return ValueTask.FromResult("#-1 FUNCTION (MAX) EXPECTS ARGUMENTS");

    if (!FunctionEvaluationUtils.TryParseDouble(arguments[0], out var current))
      return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);

    for (var i = 1; i < arguments.Count; i++)
    {
      if (!FunctionEvaluationUtils.TryParseDouble(arguments[i], out var candidate))
        return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);

      if (candidate > current)
        current = candidate;
    }

    return ValueTask.FromResult(FunctionEvaluationUtils.EnsureNumeric(current));
  }
}
