using System.Collections.Generic;

namespace PennMushSharp.Functions.Builtins;

public sealed class ModFunction : IFunction
{
  public string Name => "MOD";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    if (arguments.Count < 2)
      return ValueTask.FromResult("#-1 FUNCTION (MOD) EXPECTS AT LEAST 2 ARGUMENTS");

    if (!FunctionEvaluationUtils.TryParseLong(arguments[0], out var dividend) ||
        !FunctionEvaluationUtils.TryParseLong(arguments[1], out var divisor))
      return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);

    if (divisor == 0)
      return ValueTask.FromResult(FunctionEvaluationUtils.DivisionByZero);

    var result = dividend % divisor;
    return ValueTask.FromResult(FunctionEvaluationUtils.EnsureNumeric(result));
  }
}
