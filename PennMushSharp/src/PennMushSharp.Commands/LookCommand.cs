using System.Collections.Generic;
using System.Linq;
using PennMushSharp.Commands.Parsing;
using PennMushSharp.Core;
using PennMushSharp.Core.Persistence;

namespace PennMushSharp.Commands;

public sealed class LookCommand : ICommand
{
  private readonly InMemoryGameState _gameState;

  public LookCommand(InMemoryGameState gameState)
  {
    _gameState = gameState;
  }

  public string Name => "LOOK";

  public async ValueTask ExecuteAsync(ICommandContext context, CommandInvocation invocation, CancellationToken cancellationToken = default)
  {
    if (!_gameState.TryGet(context.Actor.DbRef, out var actorRecord) || actorRecord is null)
    {
      await context.Output.WriteLineAsync("You are lost in the void.", cancellationToken);
      return;
    }

    var targetRecord = ResolveTargetRecord(context.Actor.Location, actorRecord);
    var roomName = targetRecord.Name ?? $"#{targetRecord.DbRef}";
    var description = GetAttributeValue(targetRecord, "DESCRIBE") ?? "You see nothing special.";

    await context.Output.WriteLineAsync($"{roomName} (#{targetRecord.DbRef})", cancellationToken);
    await context.Output.WriteLineAsync(description, cancellationToken);

    var contentNames = EnumerateContents(targetRecord, context.Actor.DbRef)
      .Select(GetDisplayName)
      .ToList();

    if (contentNames.Count > 0)
    {
      await context.Output.WriteLineAsync($"Contents: {string.Join(", ", contentNames)}", cancellationToken);
    }
  }

  private GameObjectRecord ResolveTargetRecord(int? locationDbRef, GameObjectRecord fallback)
  {
    if (locationDbRef is not null && locationDbRef >= 0 && _gameState.TryGet(locationDbRef.Value, out var record) && record is not null)
      return record;
    return fallback;
  }

  private IEnumerable<GameObjectRecord> EnumerateContents(GameObjectRecord record, int actorDbRef)
  {
    var currentDbRef = record.Contents;
    var visited = new HashSet<int>();
    while (currentDbRef is { } value && value >= 0)
    {
      if (!visited.Add(value))
        yield break;

      if (!_gameState.TryGet(value, out var current) || current is null)
        yield break;

      if (current.DbRef != actorDbRef)
        yield return current;

      currentDbRef = current.Next;
    }
  }

  private static string? GetAttributeValue(GameObjectRecord record, string attributeName)
  {
    return record.Attributes.TryGetValue(attributeName, out var attribute) ? attribute.Value : null;
  }

  private static string GetDisplayName(GameObjectRecord record)
  {
    return string.IsNullOrWhiteSpace(record.Name) ? $"#{record.DbRef}" : record.Name;
  }
}
