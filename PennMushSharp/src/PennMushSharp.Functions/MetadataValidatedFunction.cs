using System;
using System.Collections.Generic;
using PennMushSharp.Core.Functions;

namespace PennMushSharp.Functions;

internal sealed class MetadataValidatedFunction : IFunction
{
  private readonly IFunction _inner;
  private readonly FunctionDefinition _definition;

  public MetadataValidatedFunction(IFunction inner, FunctionDefinition definition)
  {
    _inner = inner;
    _definition = definition;
  }

  public string Name => _inner.Name;

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    var count = arguments.Count;
    if (_definition.MinArgs > 0 && count < _definition.MinArgs)
      return ValueTask.FromResult(FormatError($"expects at least {_definition.MinArgs} arguments"));

    if (_definition.MaxArgs > 0 && _definition.MaxArgs < int.MaxValue && count > _definition.MaxArgs)
      return ValueTask.FromResult(FormatError($"expects at most {_definition.MaxArgs} arguments"));

    return _inner.InvokeAsync(context, arguments, cancellationToken);
  }

  private string FormatError(string fragment) =>
    $"#-1 FUNCTION ({Name}) {fragment.ToUpperInvariant()}";
}
