using PennMushSharp.Commands.Parsing;
using PennMushSharp.Core.Locks.Runtime;

namespace PennMushSharp.Commands;

public sealed class UnlockCommand : ICommand
{
  private readonly AttributeService _attributes;
  private readonly IMutableLockStore _lockStore;

  public UnlockCommand(AttributeService attributes, IMutableLockStore lockStore)
  {
    _attributes = attributes;
    _lockStore = lockStore;
  }

  public string Name => "@UNLOCK";

  public async ValueTask ExecuteAsync(ICommandContext context, CommandInvocation invocation, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(invocation.Argument))
    {
      await context.Output.WriteLineAsync("#-1 UNLOCK WHAT?", cancellationToken);
      return;
    }

    var left = invocation.Argument.Trim();
    var slash = left.LastIndexOf('/');
    if (slash < 0)
    {
      await context.Output.WriteLineAsync("#-1 YOU MUST SPECIFY <target>/<lock>.", cancellationToken);
      return;
    }

    var targetSpecifier = left[..slash].Trim();
    var lockName = left[(slash + 1)..].Trim();
    if (string.IsNullOrWhiteSpace(lockName))
    {
      await context.Output.WriteLineAsync("#-1 INVALID LOCK NAME.", cancellationToken);
      return;
    }

    if (!_attributes.TryResolveTarget(context.Actor, targetSpecifier, out var record, out var error) || record is null)
    {
      await context.Output.WriteLineAsync(error ?? "#-1 NO SUCH OBJECT.", cancellationToken);
      return;
    }

    if (!_attributes.CanModify(context.Actor, record))
    {
      await context.Output.WriteLineAsync("#-1 PERMISSION DENIED.", cancellationToken);
      return;
    }

    var normalizedLock = lockName.ToUpperInvariant();
    record.Locks.Remove(normalizedLock);
    _lockStore.RemoveLock(record.DbRef, normalizedLock);
    await context.Output.WriteLineAsync("Lock cleared.", cancellationToken);
  }
}
