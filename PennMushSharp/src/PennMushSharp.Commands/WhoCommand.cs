using System.Linq;
using PennMushSharp.Commands.Parsing;
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
  public required string Host { get; init; }
  public int DescriptorCount { get; init; } = 1;
  public int CommandCount { get; set; }
  public DateTime LastCommandUtc { get; set; }
}

public sealed class WhoCommand : ICommand
{
  private readonly ISessionRegistry _registry;

  public WhoCommand(ISessionRegistry registry)
  {
    _registry = registry;
  }

  public string Name => "WHO";

  public async ValueTask ExecuteAsync(ICommandContext context, CommandInvocation invocation, CancellationToken cancellationToken = default)
  {
    var sessions = _registry.GetActiveSessions();
    if (sessions.Count == 0)
    {
      await context.Output.WriteLineAsync("No one is connected.", cancellationToken);
      return;
    }

    var now = DateTime.UtcNow;
    await context.Output.WriteLineAsync("Player Name       Loc #    On For  Idle  Cmds Des  Host", cancellationToken);
    foreach (var session in sessions.OrderBy(s => s.Actor.Name, StringComparer.OrdinalIgnoreCase))
    {
      await context.Output.WriteLineAsync(FormatLine(session, now), cancellationToken);
    }

    var summary = sessions.Count == 1
      ? "There is one player connected."
      : $"There are {sessions.Count} players connected.";
    await context.Output.WriteLineAsync(summary, cancellationToken);
  }

  private static string FormatLine(SessionInfo session, DateTime now)
  {
    var location = session.Actor.Location is { } loc and >= 0 ? $"#{loc}" : "#-1";
    var onFor = FormatDuration(now - session.ConnectedAtUtc).PadLeft(8);
    var idle = FormatDuration(now - session.LastCommandUtc).PadLeft(6);
    var cmds = session.CommandCount.ToString().PadLeft(6);
    var descriptors = session.DescriptorCount.ToString().PadLeft(4);
    return string.Format("{0,-16}{1,7}{2}{3}{4}{5} {6}",
      session.Actor.Name,
      location,
      onFor,
      idle,
      cmds,
      descriptors,
      session.Host);
  }

  private static string FormatDuration(TimeSpan span)
  {
    if (span.TotalHours >= 1)
      return $"{(int)Math.Floor(span.TotalHours)}h";
    if (span.TotalMinutes >= 1)
      return $"{(int)Math.Floor(span.TotalMinutes)}m";
    var seconds = Math.Max(0, (int)Math.Floor(span.TotalSeconds));
    return $"{seconds}s";
  }
}
