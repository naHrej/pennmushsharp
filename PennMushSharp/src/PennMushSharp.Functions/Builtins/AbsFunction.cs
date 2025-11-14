using System.Collections.Generic;
using System;

namespace PennMushSharp.Functions.Builtins;

public sealed class AbsFunction : IFunction
{
  public string Name => "ABS";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    if (arguments.Count == 0)
      return ValueTask.FromResult("#-1 FUNCTION (ABS) EXPECTS ARGUMENTS");

    if (!FunctionEvaluationUtils.TryParseDouble(arguments[0], out var value))
      return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);

    return ValueTask.FromResult(FunctionEvaluationUtils.EnsureNumeric(Math.Abs(value)));
  }
}
