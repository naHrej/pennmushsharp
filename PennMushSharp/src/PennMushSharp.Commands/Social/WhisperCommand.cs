using System.Collections.Generic;
using System.Linq;
using PennMushSharp.Commands.Parsing;

namespace PennMushSharp.Commands;

public sealed class WhisperCommand : ICommand
{
  private readonly ISessionRegistry _sessionRegistry;
  private readonly SpeechService _speech;

  public WhisperCommand(ISessionRegistry sessionRegistry, SpeechService speech)
  {
    _sessionRegistry = sessionRegistry;
    _speech = speech;
  }

  public string Name => "WHISPER";

  public async ValueTask ExecuteAsync(ICommandContext context, CommandInvocation invocation, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(invocation.Target) || string.IsNullOrWhiteSpace(invocation.Argument))
    {
      await context.Output.WriteLineAsync("#-1 WHISPER REQUIRES A TARGET AND MESSAGE.", cancellationToken);
      return;
    }

    if (!await _speech.EnsureCanSpeakAsync(context.Actor, context.Output, cancellationToken))
      return;

    var recipients = _speech.ResolveRoomRecipients(invocation.Target, context.Actor.Location);
    if (recipients.Count == 0)
    {
      await context.Output.WriteLineAsync("#-1 TARGET NOT FOUND IN THIS ROOM.", cancellationToken);
      return;
    }

    foreach (var session in recipients)
    {
      if (session.Actor.Location != context.Actor.Location)
        continue;

      if (!await _speech.EnsureCanContactAsync(context.Actor, session.Actor, context.Output, cancellationToken))
        return;
    }

    await context.Output.WriteLineAsync($"You whisper, \"{invocation.Argument}\" to {DescribeRecipients(recipients)}.", cancellationToken);
    foreach (var session in recipients)
      await session.Output.WriteLineAsync($"{context.Actor.Name} whispers, \"{invocation.Argument}\"", cancellationToken);
  }

  private static string DescribeRecipients(IReadOnlyCollection<SessionInfo> recipients)
  {
    if (recipients.Count == 1)
      return recipients.First().Actor.Name;
    return string.Join(", ", recipients.Select(r => r.Actor.Name));
  }
}
