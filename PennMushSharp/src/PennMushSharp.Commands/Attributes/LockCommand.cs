using PennMushSharp.Commands.Parsing;
using PennMushSharp.Core.Locks.Runtime;
using PennMushSharp.Core.Persistence;

namespace PennMushSharp.Commands;

public sealed class LockCommand : ICommand
{
  private readonly AttributeService _attributes;
  private readonly IMutableLockStore _lockStore;

  public LockCommand(AttributeService attributes, IMutableLockStore lockStore)
  {
    _attributes = attributes;
    _lockStore = lockStore;
  }

  public string Name => "@LOCK";

  public async ValueTask ExecuteAsync(ICommandContext context, CommandInvocation invocation, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(invocation.Argument))
    {
      await context.Output.WriteLineAsync("#-1 LOCK WHAT?", cancellationToken);
      return;
    }

    var equals = invocation.Argument.IndexOf('=');
    string left;
    string expression = string.Empty;
    if (equals < 0)
    {
      left = invocation.Argument.Trim();
    }
    else
    {
      left = invocation.Argument[..equals].Trim();
      expression = invocation.Argument[(equals + 1)..].Trim();
    }

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
    if (string.IsNullOrWhiteSpace(expression))
    {
      record.Locks.Remove(normalizedLock);
      _lockStore.RemoveLock(record.DbRef, normalizedLock);
      await context.Output.WriteLineAsync("Lock cleared.", cancellationToken);
      return;
    }

    record.SetLock(normalizedLock, expression, context.Actor.DbRef);
    _lockStore.SetLock(record.DbRef, new StoredLock(normalizedLock, expression));
    await context.Output.WriteLineAsync("Lock set.", cancellationToken);
  }
}
