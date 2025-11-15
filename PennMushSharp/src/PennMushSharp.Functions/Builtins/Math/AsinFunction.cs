using System;
using System.Collections.Generic;

namespace PennMushSharp.Functions.Builtins;

public sealed class AsinFunction : IFunction
{
  public string Name => "ASIN";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    if (arguments.Count == 0 || !FunctionEvaluationUtils.TryParseDouble(arguments[0], out var value))
      return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);

    if (value < -1 || value > 1)
      return ValueTask.FromResult(FunctionEvaluationUtils.RangeErrorMessage);

    var radians = Math.Asin(value);
    var converted = FunctionEvaluationUtils.RadiansToAngle(radians, arguments.Count >= 2 ? arguments[1] : null);
    return ValueTask.FromResult(FunctionEvaluationUtils.EnsureNumeric(converted));
  }
}
