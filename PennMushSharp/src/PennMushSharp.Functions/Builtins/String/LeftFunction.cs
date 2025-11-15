using System.Collections.Generic;

namespace PennMushSharp.Functions.Builtins;

public sealed class LeftFunction : IFunction
{
  public string Name => "LEFT";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    var source = arguments.Count > 0 ? arguments[0] ?? string.Empty : string.Empty;
    if (arguments.Count < 2 || !FunctionEvaluationUtils.TryParseInt(arguments[1], out var length))
      return ValueTask.FromResult(FunctionEvaluationUtils.IntegerArgumentError);

    if (length < 0)
      return ValueTask.FromResult(FunctionEvaluationUtils.RangeErrorMessage);

    if (length >= source.Length)
      return ValueTask.FromResult(source);

    return ValueTask.FromResult(source[..length]);
  }
}
