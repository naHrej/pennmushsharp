using System.Collections.Generic;

namespace PennMushSharp.Functions.Builtins;

public sealed class RightFunction : IFunction
{
  public string Name => "RIGHT";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    var source = arguments.Count > 0 ? arguments[0] ?? string.Empty : string.Empty;
    if (arguments.Count < 2 || !FunctionEvaluationUtils.TryParseInt(arguments[1], out var length))
      return ValueTask.FromResult(FunctionEvaluationUtils.IntegerArgumentError);

    if (length < 0)
      return ValueTask.FromResult(FunctionEvaluationUtils.RangeErrorMessage);

    if (length >= source.Length)
      return ValueTask.FromResult(source);

    var start = source.Length - length;
    return ValueTask.FromResult(source[start..]);
  }
}
