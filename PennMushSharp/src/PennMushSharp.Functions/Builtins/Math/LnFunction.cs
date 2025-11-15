using System;
using System.Collections.Generic;

namespace PennMushSharp.Functions.Builtins;

public sealed class LnFunction : IFunction
{
  public string Name => "LN";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    if (arguments.Count == 0 || !FunctionEvaluationUtils.TryParseDouble(arguments[0], out var value))
      return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);

    if (value <= 0)
      return ValueTask.FromResult(FunctionEvaluationUtils.RangeErrorMessage);

    return ValueTask.FromResult(FunctionEvaluationUtils.EnsureNumeric(Math.Log(value)));
  }
}
