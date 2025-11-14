using System.Collections.Generic;

namespace PennMushSharp.Functions.Builtins;

public sealed class SetrFunction : IFunction
{
  public string Name => "SETR";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    if (arguments.Count < 2)
      return ValueTask.FromResult("#-1 SETR REQUIRES VARIABLE AND VALUE");

    var registerName = arguments[0];
    if (string.IsNullOrWhiteSpace(registerName))
      return ValueTask.FromResult("#-1 SETR REQUIRES VARIABLE AND VALUE");

    var value = arguments[1];
    context.SetRegister(registerName, value);
    return ValueTask.FromResult(value);
  }
}
