using System.Collections.Generic;

namespace PennMushSharp.Functions.Builtins;

public sealed class MidFunction : IFunction
{
  public string Name => "MID";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    var source = arguments.Count > 0 ? arguments[0] ?? string.Empty : string.Empty;
    if (arguments.Count < 3
      || !FunctionEvaluationUtils.TryParseInt(arguments[1], out var position)
      || !FunctionEvaluationUtils.TryParseInt(arguments[2], out var length))
    {
      return ValueTask.FromResult(FunctionEvaluationUtils.IntegerArgumentError);
    }

    if (position < 0)
      return ValueTask.FromResult(FunctionEvaluationUtils.RangeErrorMessage);

    if (length < 0)
    {
      position = position + length + 1;
      if (position < 0)
        position = 0;
      length = -length;
    }

    if (position >= source.Length || length == 0)
      return ValueTask.FromResult(string.Empty);

    var available = source.Length - position;
    if (length > available)
      length = available;

    return ValueTask.FromResult(source.Substring(position, length));
  }
}
