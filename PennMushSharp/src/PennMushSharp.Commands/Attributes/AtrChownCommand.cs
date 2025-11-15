using PennMushSharp.Commands.Parsing;

namespace PennMushSharp.Commands;

public sealed class AtrChownCommand : ICommand
{
  private readonly AttributeService _attributes;

  public AtrChownCommand(AttributeService attributes)
  {
    _attributes = attributes;
  }

  public string Name => "@ATRCHOWN";

  public async ValueTask ExecuteAsync(ICommandContext context, CommandInvocation invocation, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(invocation.Argument))
    {
      await context.Output.WriteLineAsync("#-1 ATRCHOWN WHAT?", cancellationToken);
      return;
    }

    var equalsIndex = invocation.Argument.IndexOf('=');
    if (equalsIndex < 0)
    {
      await context.Output.WriteLineAsync("#-1 ATRCHOWN REQUIRES <target>/<attr>=<player>", cancellationToken);
      return;
    }

    var left = invocation.Argument[..equalsIndex].Trim();
    var newOwnerName = invocation.Argument[(equalsIndex + 1)..].Trim();
    if (string.IsNullOrWhiteSpace(newOwnerName))
    {
      await context.Output.WriteLineAsync("#-1 WHO SHOULD OWN THE ATTRIBUTE?", cancellationToken);
      return;
    }

    var slash = left.LastIndexOf('/');
    if (slash < 0)
    {
      await context.Output.WriteLineAsync("#-1 YOU MUST SPECIFY <target>/<attr>.", cancellationToken);
      return;
    }

    var targetSpecifier = left[..slash].Trim();
    var attributeName = left[(slash + 1)..].Trim().ToUpperInvariant();
    if (attributeName.Length == 0)
    {
      await context.Output.WriteLineAsync("#-1 INVALID ATTRIBUTE NAME.", cancellationToken);
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

    if (!record.Attributes.TryGetValue(attributeName, out var attribute))
    {
      await context.Output.WriteLineAsync("#-1 ATTRIBUTE NOT FOUND.", cancellationToken);
      return;
    }

    if (!_attributes.TryResolveTarget(context.Actor, newOwnerName, out var newOwnerRecord, out error) || newOwnerRecord is null)
    {
      await context.Output.WriteLineAsync(error ?? "#-1 NO SUCH PLAYER.", cancellationToken);
      return;
    }

    attribute.Owner = newOwnerRecord.DbRef;
    await context.Output.WriteLineAsync($"Attribute {attribute.Name} ownership changed to {newOwnerRecord.Name}.", cancellationToken);
  }
}
