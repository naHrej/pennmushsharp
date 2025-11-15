using PennMushSharp.Core;
using PennMushSharp.Core.Persistence;

namespace PennMushSharp.Functions.Builtins;

public sealed class XGetFunction : IFunction
{
  private readonly InMemoryGameState _gameState;

  public XGetFunction(InMemoryGameState gameState)
  {
    _gameState = gameState;
  }

  public string Name => "XGET";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    if (arguments.Count == 0)
      return ValueTask.FromResult(string.Empty);

    var spec = arguments[0];
    var (target, attributeName) = ParseSpec(spec);
    if (string.IsNullOrWhiteSpace(attributeName))
      return ValueTask.FromResult(string.Empty);

    var record = ResolveRecord(target, context.Actor);
    if (record is null)
      return ValueTask.FromResult(string.Empty);

    if (record.Attributes.TryGetValue(attributeName.ToUpperInvariant(), out var attribute))
      return ValueTask.FromResult(attribute.Value ?? string.Empty);

    return ValueTask.FromResult(string.Empty);
  }

  private (string? Target, string Attribute) ParseSpec(string spec)
  {
    var slash = spec.LastIndexOf('/');
    if (slash < 0)
      return (null, spec);

    return (spec[..slash], spec[(slash + 1)..]);
  }

  private GameObjectRecord? ResolveRecord(string? spec, GameObject actor)
  {
    if (string.IsNullOrWhiteSpace(spec) || spec.Equals("ME", StringComparison.OrdinalIgnoreCase))
    {
      _gameState.TryGet(actor.DbRef, out var record);
      return record;
    }

    if (spec.Equals("HERE", StringComparison.OrdinalIgnoreCase) && actor.Location is { } loc)
    {
      _gameState.TryGet(loc, out var record);
      return record;
    }

    if (spec.StartsWith("#", StringComparison.Ordinal) && int.TryParse(spec[1..], out var dbRef))
    {
      _gameState.TryGet(dbRef, out var record);
      return record;
    }

    if (_gameState.TryGet(spec, out var named))
      return named;

    return null;
  }
}
