using PennMushSharp.Commands.Parsing;

namespace PennMushSharp.Commands;

public sealed class SetCommand : ICommand
{
  private readonly AttributeService _attributes;

  public SetCommand(AttributeService attributes)
  {
    _attributes = attributes;
  }

  public string Name => "@SET";

  public async ValueTask ExecuteAsync(ICommandContext context, CommandInvocation invocation, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(invocation.Argument))
    {
      await context.Output.WriteLineAsync("#-1 SET WHAT?", cancellationToken);
      return;
    }

    var equalsIndex = invocation.Argument.IndexOf('=');
    if (equalsIndex < 0)
    {
      await context.Output.WriteLineAsync("#-1 SET REQUIRES <target>/<attr>=<value>.", cancellationToken);
      return;
    }

    var left = invocation.Argument[..equalsIndex].Trim();
    var value = invocation.Argument[(equalsIndex + 1)..];

    var slash = left.LastIndexOf('/');
    if (slash < 0)
    {
      await context.Output.WriteLineAsync("#-1 YOU MUST SPECIFY <target>/<attr>.", cancellationToken);
      return;
    }

    var targetSpecifier = left[..slash].Trim();
    var attributeName = left[(slash + 1)..].Trim();
    if (string.IsNullOrWhiteSpace(attributeName))
    {
      await context.Output.WriteLineAsync("#-1 INVALID ATTRIBUTE NAME.", cancellationToken);
      return;
    }

    if (!_attributes.TryResolveTarget(context.Actor, targetSpecifier, out var record, out var error) || record is null)
    {
      await context.Output.WriteLineAsync(error ?? "#-1 NO SUCH OBJECT.", cancellationToken);
      return;
    }

    if (string.IsNullOrWhiteSpace(value))
    {
      if (!_attributes.TryRemoveAttribute(context.Actor, record, attributeName, out error))
      {
        await context.Output.WriteLineAsync(error ?? "#-1 PERMISSION DENIED.", cancellationToken);
        return;
      }

      await context.Output.WriteLineAsync("Attribute cleared.", cancellationToken);
      return;
    }

    if (!_attributes.TrySetAttribute(context.Actor, record, attributeName, value, out error))
    {
      await context.Output.WriteLineAsync(error ?? "#-1 PERMISSION DENIED.", cancellationToken);
      return;
    }

    await context.Output.WriteLineAsync("Attribute set.", cancellationToken);
  }
}
