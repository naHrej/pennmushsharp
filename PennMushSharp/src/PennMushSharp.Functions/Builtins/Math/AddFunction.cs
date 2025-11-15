using System.Collections.Generic;

namespace PennMushSharp.Functions.Builtins;

public sealed class AddFunction : IFunction
{
  public string Name => "ADD";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    double sum = 0;
    foreach (var argument in arguments)
    {
      if (!FunctionEvaluationUtils.TryParseDouble(argument, out var value))
        return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);
      sum += value;
    }

    return ValueTask.FromResult(FunctionEvaluationUtils.EnsureNumeric(sum));
  }
}
