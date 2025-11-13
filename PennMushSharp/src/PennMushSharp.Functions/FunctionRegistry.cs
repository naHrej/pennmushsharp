using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PennMushSharp.Functions;

public sealed class FunctionRegistry
{
  private readonly ImmutableDictionary<string, IFunction> _functions;

  internal FunctionRegistry(ImmutableDictionary<string, IFunction> functions)
  {
    _functions = functions;
  }

  public static FunctionRegistry Empty { get; } = new(FunctionRegistryBuilder.Empty);

  public bool TryGet(string name, out IFunction? function) => _functions.TryGetValue(name, out function);
}

public interface IFunction
{
  string Name { get; }
  ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default);
}

public sealed class FunctionRegistryBuilder
{
  internal static readonly ImmutableDictionary<string, IFunction> Empty = ImmutableDictionary<string, IFunction>.Empty;
  private readonly Dictionary<string, IFunction> _functions = new(StringComparer.OrdinalIgnoreCase);

  public void Add(IFunction function) => _functions[function.Name] = function;

  public FunctionRegistry Build() => new(_functions.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase));
}
