using System.Collections.Generic;

namespace PennMushSharp.Functions.Builtins;

public sealed class SubFunction : IFunction
{
  public string Name => "SUB";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    if (arguments.Count == 0)
      return ValueTask.FromResult("#-1 FUNCTION (SUB) EXPECTS ARGUMENTS");

    if (!FunctionEvaluationUtils.TryParseDouble(arguments[0], out var result))
      return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);

    for (var i = 1; i < arguments.Count; i++)
    {
      if (!FunctionEvaluationUtils.TryParseDouble(arguments[i], out var value))
        return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);
      result -= value;
    }

    return ValueTask.FromResult(FunctionEvaluationUtils.EnsureNumeric(result));
  }
}
