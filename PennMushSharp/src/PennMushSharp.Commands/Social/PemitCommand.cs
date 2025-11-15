using System.Linq;
using PennMushSharp.Commands.Parsing;

namespace PennMushSharp.Commands;

public sealed class PemitCommand : ICommand
{
  private readonly ISessionRegistry _sessionRegistry;
  private readonly SpeechService _speech;

  public PemitCommand(ISessionRegistry sessionRegistry, SpeechService speech)
  {
    _sessionRegistry = sessionRegistry;
    _speech = speech;
  }

  public string Name => "@PEMIT";

  public async ValueTask ExecuteAsync(ICommandContext context, CommandInvocation invocation, CancellationToken cancellationToken = default)
  {
    var message = invocation.Argument;
    if (string.IsNullOrWhiteSpace(message))
    {
      await context.Output.WriteLineAsync("#-1 WHAT DO YOU WANT TO EMIT?", cancellationToken);
      return;
    }

    var targetToken = invocation.Target ?? "ME";
    if (IsSelfTarget(targetToken))
    {
      await context.Output.WriteLineAsync(message, cancellationToken);
      return;
    }

    if (IsHereTarget(targetToken))
    {
      if (!await _speech.EnsureCanSpeakAsync(context.Actor, context.Output, cancellationToken))
        return;

      var delivered = await SendToLocationAsync(context.Actor.Location!.Value, message, cancellationToken);
      if (!delivered)
        await context.Output.WriteLineAsync("#-1 NOBODY IS LISTENING.", cancellationToken);
      return;
    }

    var recipients = _speech.ResolveGlobalRecipients(targetToken);
    if (recipients.Count == 0)
    {
      await context.Output.WriteLineAsync("#-1 TARGET NOT CONNECTED.", cancellationToken);
      return;
    }

    var deliveredCount = 0;
    foreach (var session in recipients)
    {
      if (!await _speech.EnsureCanContactAsync(context.Actor, session.Actor, context.Output, cancellationToken))
        return;

      await session.Output.WriteLineAsync(message, cancellationToken);
      deliveredCount++;
    }

    if (deliveredCount == 0)
      await context.Output.WriteLineAsync("#-1 TARGET NOT CONNECTED.", cancellationToken);
  }

  private async ValueTask<bool> SendToLocationAsync(int locationDbRef, string message, CancellationToken cancellationToken)
  {
    var delivered = false;
    foreach (var session in _sessionRegistry.GetActiveSessions())
    {
      if (session.Actor.Location != locationDbRef)
        continue;

      delivered = true;
      await session.Output.WriteLineAsync(message, cancellationToken);
    }
    return delivered;
  }

  private static bool IsSelfTarget(string token)
  {
    return token.Equals("ME", StringComparison.OrdinalIgnoreCase) ||
      token.Equals("SELF", StringComparison.OrdinalIgnoreCase);
  }

  private static bool IsHereTarget(string token) =>
    token.Equals("HERE", StringComparison.OrdinalIgnoreCase);

}
