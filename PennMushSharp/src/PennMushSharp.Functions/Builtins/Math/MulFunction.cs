using System.Collections.Generic;

namespace PennMushSharp.Functions.Builtins;

public sealed class MulFunction : IFunction
{
  public string Name => "MUL";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    double product = 1;
    foreach (var argument in arguments)
    {
      if (!FunctionEvaluationUtils.TryParseDouble(argument, out var value))
        return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);
      product *= value;
    }

    return ValueTask.FromResult(FunctionEvaluationUtils.EnsureNumeric(product));
  }
}
