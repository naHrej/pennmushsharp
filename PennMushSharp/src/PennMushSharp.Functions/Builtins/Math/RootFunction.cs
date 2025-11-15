using System;
using System.Collections.Generic;

namespace PennMushSharp.Functions.Builtins;

public sealed class RootFunction : IFunction
{
  public string Name => "ROOT";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    if (arguments.Count < 2
      || !FunctionEvaluationUtils.TryParseDouble(arguments[0], out var value)
      || !FunctionEvaluationUtils.TryParseInt(arguments[1], out var degree))
    {
      return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);
    }

    if (degree == 0)
      return ValueTask.FromResult(FunctionEvaluationUtils.RangeErrorMessage);

    if (value < 0 && degree % 2 == 0)
      return ValueTask.FromResult(FunctionEvaluationUtils.ImaginaryNumber);

    var absValue = Math.Abs(value);
    var result = Math.Pow(absValue, 1.0 / degree);
    if (value < 0)
      result = -result;

    return ValueTask.FromResult(FunctionEvaluationUtils.EnsureNumeric(result));
  }
}
