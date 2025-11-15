using System.Collections.Generic;
using System.Linq;
using PennMushSharp.Commands.Parsing;

namespace PennMushSharp.Commands;

public sealed class PageCommand : ICommand
{
  private readonly ISessionRegistry _sessionRegistry;
  private readonly SpeechService _speech;

  public PageCommand(ISessionRegistry sessionRegistry, SpeechService speech)
  {
    _sessionRegistry = sessionRegistry;
    _speech = speech;
  }

  public string Name => "PAGE";

  public async ValueTask ExecuteAsync(ICommandContext context, CommandInvocation invocation, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(invocation.Target) || string.IsNullOrWhiteSpace(invocation.Argument))
    {
      await context.Output.WriteLineAsync("#-1 PAGE WHAT WHOM?", cancellationToken);
      return;
    }

    var recipients = _speech.ResolveGlobalRecipients(invocation.Target);
    if (recipients.Count == 0)
    {
      await context.Output.WriteLineAsync("#-1 TARGET NOT CONNECTED.", cancellationToken);
      return;
    }

    foreach (var session in recipients)
    {
      if (!await _speech.EnsureCanContactAsync(context.Actor, session.Actor, context.Output, cancellationToken))
        return;
    }

    var actorLine = $"You page {DescribeRecipients(recipients)} with \"{invocation.Argument}\"";
    await context.Output.WriteLineAsync(actorLine, cancellationToken);

    foreach (var session in recipients)
      await session.Output.WriteLineAsync($"{context.Actor.Name} pages: \"{invocation.Argument}\"", cancellationToken);
  }

  private static string DescribeRecipients(IReadOnlyCollection<SessionInfo> recipients)
  {
    if (recipients.Count == 1)
      return recipients.First().Actor.Name;
    return string.Join(", ", recipients.Select(r => r.Actor.Name));
  }
}
