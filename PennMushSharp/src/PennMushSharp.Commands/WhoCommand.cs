using System.Linq;
using PennMushSharp.Core;

namespace PennMushSharp.Commands;

public interface ISessionRegistry
{
  IReadOnlyCollection<SessionInfo> GetActiveSessions();
}

public sealed class SessionInfo
{
  public required GameObject Actor { get; init; }
  public required DateTime ConnectedAtUtc { get; init; }
}

public sealed class WhoCommand : ICommand
{
  private readonly ISessionRegistry _registry;

  public WhoCommand(ISessionRegistry registry)
  {
    _registry = registry;
  }

  public string Name => "WHO";

  public async ValueTask ExecuteAsync(ICommandContext context, string arguments, CancellationToken cancellationToken = default)
  {
    var sessions = _registry.GetActiveSessions();
    if (sessions.Count == 0)
    {
      await context.Output.WriteLineAsync("No one is connected.", cancellationToken);
      return;
    }

    await context.Output.WriteLineAsync("Player Name       Connected", cancellationToken);
    foreach (var session in sessions.OrderByDescending(s => s.ConnectedAtUtc))
    {
      var minutes = (int)(DateTime.UtcNow - session.ConnectedAtUtc).TotalMinutes;
      await context.Output.WriteLineAsync($"{session.Actor.Name,-16} {minutes,3}m", cancellationToken);
    }
    await context.Output.WriteLineAsync($"Total: {sessions.Count} session(s).", cancellationToken);
  }
}
