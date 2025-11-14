using System.Collections.Generic;
using System;

namespace PennMushSharp.Functions.Builtins;

public sealed class PowerFunction : IFunction
{
  public string Name => "POWER";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    if (arguments.Count < 2
      || !FunctionEvaluationUtils.TryParseDouble(arguments[0], out var value)
      || !FunctionEvaluationUtils.TryParseDouble(arguments[1], out var exponent))
    {
      return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);
    }

    var result = Math.Pow(value, exponent);
    if (double.IsNaN(result) || double.IsInfinity(result))
      return ValueTask.FromResult(FunctionEvaluationUtils.RangeErrorMessage);

    return ValueTask.FromResult(FunctionEvaluationUtils.EnsureNumeric(result));
  }
}
