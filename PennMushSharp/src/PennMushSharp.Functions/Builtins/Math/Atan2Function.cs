using System;
using System.Collections.Generic;

namespace PennMushSharp.Functions.Builtins;

public sealed class Atan2Function : IFunction
{
  public string Name => "ATAN2";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    if (arguments.Count < 2
      || !FunctionEvaluationUtils.TryParseDouble(arguments[0], out var y)
      || !FunctionEvaluationUtils.TryParseDouble(arguments[1], out var x))
    {
      return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);
    }

    var radians = Math.Atan2(y, x);
    var converted = FunctionEvaluationUtils.RadiansToAngle(radians, arguments.Count >= 3 ? arguments[2] : null);
    return ValueTask.FromResult(FunctionEvaluationUtils.EnsureNumeric(converted));
  }
}
