using PennMushSharp.Core;
using PennMushSharp.Core.Persistence;

namespace PennMushSharp.Commands;

/// <summary>
/// Handles attribute resolution, permission checks, and updates for commands.
/// </summary>
public sealed class AttributeService
{
  private readonly InMemoryGameState _gameState;

  public AttributeService(InMemoryGameState gameState)
  {
    _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
  }

  public bool TryResolveTarget(GameObject actor, string? targetSpecifier, out GameObjectRecord? record, out string? error)
  {
    ArgumentNullException.ThrowIfNull(actor);

    if (string.IsNullOrWhiteSpace(targetSpecifier) || targetSpecifier.Equals("me", StringComparison.OrdinalIgnoreCase))
    {
      if (_gameState.TryGet(actor.DbRef, out record) && record is not null)
      {
        error = null;
        return true;
      }

      record = null;
      error = "#-1 YOU DON'T SEEM TO EXIST.";
      return false;
    }

    if (targetSpecifier.StartsWith("#", StringComparison.Ordinal))
    {
      if (int.TryParse(targetSpecifier[1..], out var dbRef) && _gameState.TryGet(dbRef, out record) && record is not null)
      {
        error = null;
        return true;
      }

      record = null;
      error = "#-1 NO SUCH OBJECT.";
      return false;
    }

    if (_gameState.TryGet(targetSpecifier, out record) && record is not null)
    {
      error = null;
      return true;
    }

    record = null;
    error = "#-1 NO SUCH OBJECT.";
    return false;
  }

  public bool TrySetAttribute(GameObject actor, GameObjectRecord target, string attributeName, string value, out string? error)
  {
    ArgumentNullException.ThrowIfNull(actor);
    ArgumentNullException.ThrowIfNull(target);

    if (!CanModify(actor, target))
    {
      error = "#-1 PERMISSION DENIED.";
      return false;
    }

    var normalized = attributeName.Trim().ToUpperInvariant();
    target.SetAttribute(normalized, value, actor.DbRef);
    error = null;
    return true;
  }

  public bool TryGetAttributeValue(GameObject actor, string? targetSpecifier, string attributeName, out string? value, out string? error)
  {
    attributeName = attributeName.Trim().ToUpperInvariant();
    if (string.IsNullOrWhiteSpace(attributeName))
    {
      value = null;
      error = "#-1 INVALID ATTRIBUTE NAME.";
      return false;
    }

    if (!TryResolveTarget(actor, targetSpecifier, out var record, out error) || record is null)
    {
      value = null;
      return false;
    }

    if (record.Attributes.TryGetValue(attributeName, out var attribute))
    {
      value = attribute.Value;
      error = null;
      return true;
    }

    value = null;
    error = null;
    return false;
  }

  public bool TryRemoveAttribute(GameObject actor, GameObjectRecord target, string attributeName, out string? error)
  {
    ArgumentNullException.ThrowIfNull(actor);
    ArgumentNullException.ThrowIfNull(target);

    if (!CanModify(actor, target))
    {
      error = "#-1 PERMISSION DENIED.";
      return false;
    }

    var normalized = attributeName.Trim().ToUpperInvariant();
    target.Attributes.Remove(normalized);
    error = null;
    return true;
  }

  public bool CanModify(GameObject actor, GameObjectRecord target)
  {
    if (actor.DbRef == target.DbRef)
      return true;

    if (target.Owner.HasValue && target.Owner.Value == actor.DbRef)
      return true;

    return false;
  }
}
