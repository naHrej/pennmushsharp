using PennMushSharp.Commands.Parsing;

namespace PennMushSharp.Commands;

public sealed class SayCommand : ICommand
{
  private readonly ISessionRegistry _sessionRegistry;
  private readonly SpeechService _speech;

  public SayCommand(ISessionRegistry sessionRegistry, SpeechService speech)
  {
    _sessionRegistry = sessionRegistry;
    _speech = speech;
  }

  public string Name => "SAY";

  public async ValueTask ExecuteAsync(ICommandContext context, CommandInvocation invocation, CancellationToken cancellationToken = default)
  {
    var message = invocation.Argument;
    if (string.IsNullOrWhiteSpace(message))
    {
      await context.Output.WriteLineAsync("Say what?", cancellationToken);
      return;
    }

    if (!await _speech.EnsureCanSpeakAsync(context.Actor, context.Output, cancellationToken))
      return;

    await context.Output.WriteLineAsync($"You say, \"{message}\"", cancellationToken);

    var location = context.Actor.Location;
    foreach (var session in _sessionRegistry.GetActiveSessions())
    {
      if (session.Actor.DbRef == context.Actor.DbRef)
        continue;
      if (session.Actor.Location != location)
        continue;

      await session.Output.WriteLineAsync($"{context.Actor.Name} says, \"{message}\"", cancellationToken);
    }
  }
}
