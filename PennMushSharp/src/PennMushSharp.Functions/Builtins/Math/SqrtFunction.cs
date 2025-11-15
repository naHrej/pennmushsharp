using System.Collections.Generic;
using System;

namespace PennMushSharp.Functions.Builtins;

public sealed class SqrtFunction : IFunction
{
  public string Name => "SQRT";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    if (arguments.Count == 0 || !FunctionEvaluationUtils.TryParseDouble(arguments[0], out var value))
      return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);

    if (value < 0)
      return ValueTask.FromResult(FunctionEvaluationUtils.ImaginaryNumber);

    return ValueTask.FromResult(FunctionEvaluationUtils.EnsureNumeric(Math.Sqrt(value)));
  }
}
