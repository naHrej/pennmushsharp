using System;
using System.Collections.Generic;
using System.Globalization;

namespace PennMushSharp.Functions.Builtins;

public sealed class LogFunction : IFunction
{
  public string Name => "LOG";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    if (arguments.Count == 0 || !FunctionEvaluationUtils.TryParseDouble(arguments[0], out var value))
      return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);

    if (value <= 0)
      return ValueTask.FromResult(FunctionEvaluationUtils.RangeErrorMessage);

    double result;
    if (arguments.Count >= 2)
    {
      var baseArg = arguments[1];
      if (string.Equals(baseArg, "e", StringComparison.OrdinalIgnoreCase))
      {
        result = Math.Log(value);
      }
      else if (FunctionEvaluationUtils.TryParseDouble(baseArg, out var baseValue))
      {
        if (baseValue <= 0 || Math.Abs(baseValue - 1) < double.Epsilon)
          return ValueTask.FromResult(FunctionEvaluationUtils.RangeErrorMessage);
        result = Math.Log(value) / Math.Log(baseValue);
      }
      else
      {
        return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);
      }
    }
    else
    {
      result = Math.Log10(value);
    }

    return ValueTask.FromResult(FunctionEvaluationUtils.EnsureNumeric(result));
  }
}
