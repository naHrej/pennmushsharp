using System.Collections.Generic;
using System;

namespace PennMushSharp.Functions.Builtins;

public sealed class CeilFunction : IFunction
{
  public string Name => "CEIL";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    if (arguments.Count == 0 || !FunctionEvaluationUtils.TryParseDouble(arguments[0], out var value))
      return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);

    return ValueTask.FromResult(FunctionEvaluationUtils.EnsureNumeric(Math.Ceiling(value)));
  }
}
