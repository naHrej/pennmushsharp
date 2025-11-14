using System.Collections.Generic;
using System.Globalization;

namespace PennMushSharp.Functions.Builtins;

public sealed class StrlenFunction : IFunction
{
  public string Name => "STRLEN";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    var value = arguments.Count > 0 ? arguments[0] ?? string.Empty : string.Empty;
    return ValueTask.FromResult(value.Length.ToString(CultureInfo.InvariantCulture));
  }
}
