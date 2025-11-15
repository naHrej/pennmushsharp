using System;
using System.Collections.Generic;

namespace PennMushSharp.Functions.Builtins;

public sealed class CosFunction : IFunction
{
  public string Name => "COS";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    if (arguments.Count == 0 || !FunctionEvaluationUtils.TryParseDouble(arguments[0], out var value))
      return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);

    var radians = FunctionEvaluationUtils.AngleToRadians(value, arguments.Count >= 2 ? arguments[1] : null);
    var result = Math.Cos(radians);
    if (double.IsNaN(result) || double.IsInfinity(result))
      return ValueTask.FromResult(FunctionEvaluationUtils.RangeErrorMessage);

    return ValueTask.FromResult(FunctionEvaluationUtils.EnsureNumeric(result));
  }
}
