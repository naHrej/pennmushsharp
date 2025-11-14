using System;
using System.Collections.Generic;
using System.Globalization;

namespace PennMushSharp.Functions.Builtins;

public sealed class PiFunction : IFunction
{
  public string Name => "PI";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    return ValueTask.FromResult(Math.PI.ToString("0.###############", CultureInfo.InvariantCulture));
  }
}
