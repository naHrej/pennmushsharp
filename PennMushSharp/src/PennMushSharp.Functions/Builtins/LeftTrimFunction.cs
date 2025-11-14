using System.Collections.Generic;

namespace PennMushSharp.Functions.Builtins;

public sealed class LeftTrimFunction : IFunction
{
  public string Name => "LTRIM";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    var value = arguments.Count > 0 ? arguments[0] ?? string.Empty : string.Empty;
    var trimChars = arguments.Count > 1 ? arguments[1] : null;
    return ValueTask.FromResult(FunctionEvaluationUtils.Trim(value, trimChars, TrimMode.Left));
  }
}
