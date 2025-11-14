using System.Collections.Generic;
using System;

namespace PennMushSharp.Functions.Builtins;

public sealed class DivFunction : IFunction
{
  public string Name => "DIV";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    if (arguments.Count == 0)
      return ValueTask.FromResult("#-1 FUNCTION (DIV) EXPECTS ARGUMENTS");

    if (!FunctionEvaluationUtils.TryParseDouble(arguments[0], out var result))
      return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);

    for (var i = 1; i < arguments.Count; i++)
    {
      if (!FunctionEvaluationUtils.TryParseDouble(arguments[i], out var value))
        return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);

      if (Math.Abs(value) < double.Epsilon)
        return ValueTask.FromResult(FunctionEvaluationUtils.DivisionByZero);

      result /= value;
    }

    return ValueTask.FromResult(FunctionEvaluationUtils.EnsureNumeric(result));
  }
}
