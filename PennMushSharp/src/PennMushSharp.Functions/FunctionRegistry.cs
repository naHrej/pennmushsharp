using System.Collections.Generic;
using System.Linq;
using PennMushSharp.Core.Functions;

namespace PennMushSharp.Functions;

public sealed class FunctionRegistry
{
  private readonly IReadOnlyDictionary<string, IFunction> _functions;

  internal FunctionRegistry(IReadOnlyDictionary<string, IFunction> functions)
  {
    _functions = functions;
  }

  public static FunctionRegistry Empty { get; } =
    new Dictionary<string, IFunction>(StringComparer.OrdinalIgnoreCase).AsReadOnlyRegistry();

  public bool TryGet(string name, out IFunction? function) => _functions.TryGetValue(name, out function);
}

public interface IFunction
{
  string Name { get; }
  ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default);
}

public sealed class FunctionRegistryBuilder
{
  private readonly Dictionary<string, IFunction> _functions = new(StringComparer.OrdinalIgnoreCase);
  private readonly Dictionary<string, FunctionDefinition> _definitionsByName;
  private readonly Dictionary<string, IReadOnlyList<string>> _aliasesByFunction;

  public FunctionRegistryBuilder(FunctionCatalog? metadata = null)
  {
    _definitionsByName = new Dictionary<string, FunctionDefinition>(StringComparer.OrdinalIgnoreCase);
    if (metadata is not null)
    {
      foreach (var definition in metadata.Functions)
      {
        if (!_definitionsByName.ContainsKey(definition.Name))
          _definitionsByName[definition.Name] = definition;
      }
    }

    _aliasesByFunction = metadata?.Aliases
        .GroupBy(a => a.FunctionName, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(
          g => g.Key,
          g => (IReadOnlyList<string>)g.Select(a => a.Alias).ToList(),
          StringComparer.OrdinalIgnoreCase)
      ?? new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
  }

  public void Add(IFunction function, IEnumerable<string>? additionalAliases = null)
  {
    var instance = WrapWithMetadata(function);
    Register(function.Name, instance);

    if (_aliasesByFunction.TryGetValue(function.Name, out var metadataAliases))
    {
      foreach (var alias in metadataAliases)
        Register(alias, instance);
    }

    if (additionalAliases is not null)
    {
      foreach (var alias in additionalAliases)
      {
        if (!string.IsNullOrWhiteSpace(alias))
          Register(alias, instance);
      }
    }
  }

  public FunctionRegistry Build() =>
    new Dictionary<string, IFunction>(_functions, StringComparer.OrdinalIgnoreCase).AsReadOnlyRegistry();

  private IFunction WrapWithMetadata(IFunction function)
  {
    if (_definitionsByName.TryGetValue(function.Name, out var definition))
      return new MetadataValidatedFunction(function, definition);
    return function;
  }

  private void Register(string name, IFunction function)
  {
    _functions[name] = function;
  }
}

internal static class FunctionRegistryExtensions
{
  public static FunctionRegistry AsReadOnlyRegistry(this Dictionary<string, IFunction> source) =>
    new(source);
}
