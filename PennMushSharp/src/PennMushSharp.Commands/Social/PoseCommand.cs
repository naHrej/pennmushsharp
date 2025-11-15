using PennMushSharp.Commands.Parsing;

namespace PennMushSharp.Commands;

public sealed class PoseCommand : ICommand
{
  private readonly ISessionRegistry _sessionRegistry;
  private readonly SpeechService _speech;

  public PoseCommand(ISessionRegistry sessionRegistry, SpeechService speech)
  {
    _sessionRegistry = sessionRegistry;
    _speech = speech;
  }

  public string Name => "POSE";

  public async ValueTask ExecuteAsync(ICommandContext context, CommandInvocation invocation, CancellationToken cancellationToken = default)
  {
    var action = invocation.Argument;
    if (string.IsNullOrWhiteSpace(action))
    {
      await context.Output.WriteLineAsync("Pose what?", cancellationToken);
      return;
    }

    if (!await _speech.EnsureCanSpeakAsync(context.Actor, context.Output, cancellationToken))
      return;

    var actorLine = $"You pose: {context.Actor.Name} {action}";
    var roomLine = $"{context.Actor.Name} {action}";

    await context.Output.WriteLineAsync(actorLine, cancellationToken);

    var location = context.Actor.Location;
    foreach (var session in _sessionRegistry.GetActiveSessions())
    {
      if (session.Actor.DbRef == context.Actor.DbRef)
        continue;
      if (session.Actor.Location != location)
        continue;

      await session.Output.WriteLineAsync(roomLine, cancellationToken);
    }
  }
}
