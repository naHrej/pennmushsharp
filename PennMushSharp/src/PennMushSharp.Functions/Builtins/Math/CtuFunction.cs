using System.Collections.Generic;

namespace PennMushSharp.Functions.Builtins;

public sealed class CtuFunction : IFunction
{
  public string Name => "CTU";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    if (arguments.Count < 3 || !FunctionEvaluationUtils.TryParseDouble(arguments[0], out var value))
      return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);

    var radians = FunctionEvaluationUtils.AngleToRadians(value, arguments[1]);
    var converted = FunctionEvaluationUtils.RadiansToAngle(radians, arguments[2]);
    return ValueTask.FromResult(FunctionEvaluationUtils.EnsureNumeric(converted));
  }
}
