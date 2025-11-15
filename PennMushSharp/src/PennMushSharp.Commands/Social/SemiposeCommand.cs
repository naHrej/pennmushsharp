using PennMushSharp.Commands.Parsing;

namespace PennMushSharp.Commands;

public sealed class SemiposeCommand : ICommand
{
  private readonly ISessionRegistry _sessionRegistry;
  private readonly SpeechService _speech;

  public SemiposeCommand(ISessionRegistry sessionRegistry, SpeechService speech)
  {
    _sessionRegistry = sessionRegistry;
    _speech = speech;
  }

  public string Name => "SEMIPOSE";

  public async ValueTask ExecuteAsync(ICommandContext context, CommandInvocation invocation, CancellationToken cancellationToken = default)
  {
    var action = invocation.Argument;
    if (string.IsNullOrEmpty(action))
    {
      await context.Output.WriteLineAsync("Semipose what?", cancellationToken);
      return;
    }

    if (!await _speech.EnsureCanSpeakAsync(context.Actor, context.Output, cancellationToken))
      return;

    var selfLine = $"You semipose: {context.Actor.Name}{action}";
    var roomLine = $"{context.Actor.Name}{action}";

    await context.Output.WriteLineAsync(selfLine, cancellationToken);

    var location = context.Actor.Location!.Value;
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
