using System.Collections.Generic;
using System.Text;

namespace PennMushSharp.Functions.Builtins;

public sealed class RepeatFunction : IFunction
{
  public string Name => "REPEAT";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    var value = arguments.Count > 0 ? arguments[0] ?? string.Empty : string.Empty;
    if (arguments.Count < 2 || !FunctionEvaluationUtils.TryParseInt(arguments[1], out var count))
      return ValueTask.FromResult(FunctionEvaluationUtils.IntegerArgumentError);

    if (count < 0)
      return ValueTask.FromResult(FunctionEvaluationUtils.RangeErrorMessage);

    if (count == 0 || value.Length == 0)
      return ValueTask.FromResult(string.Empty);

    var builder = new StringBuilder(value.Length * count);
    for (var i = 0; i < count; i++)
      builder.Append(value);
    return ValueTask.FromResult(builder.ToString());
  }
}
