using PennMushSharp.Commands.Parsing;

namespace PennMushSharp.Commands;

public sealed class AtrLockCommand : ICommand
{
  private readonly AttributeService _attributes;

  public AtrLockCommand(AttributeService attributes)
  {
    _attributes = attributes;
  }

  public string Name => "@ATRLOCK";

  public async ValueTask ExecuteAsync(ICommandContext context, CommandInvocation invocation, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(invocation.Argument))
    {
      await context.Output.WriteLineAsync("#-1 ATRLOCK WHAT?", cancellationToken);
      return;
    }

    var equalsIndex = invocation.Argument.IndexOf('=');
    if (equalsIndex < 0)
    {
      await context.Output.WriteLineAsync("#-1 ATRLOCK REQUIRES <target>/<attr>=<ON|OFF>.", cancellationToken);
      return;
    }

    var targetAndAttr = invocation.Argument[..equalsIndex].Trim();
    var flag = invocation.Argument[(equalsIndex + 1)..].Trim();
    if (flag.Length == 0)
    {
      await context.Output.WriteLineAsync("#-1 WHAT DO YOU WANT TO DO WITH THE LOCK?", cancellationToken);
      return;
    }

    var slash = targetAndAttr.LastIndexOf('/');
    if (slash < 0)
    {
      await context.Output.WriteLineAsync("#-1 YOU MUST SPECIFY <target>/<attr>.", cancellationToken);
      return;
    }

    var targetSpecifier = targetAndAttr[..slash].Trim();
    var attributeName = targetAndAttr[(slash + 1)..].Trim().ToUpperInvariant();
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

    if (!record.Attributes.TryGetValue(attributeName, out var attribute))
    {
      await context.Output.WriteLineAsync("#-1 ATTRIBUTE NOT FOUND.", cancellationToken);
      return;
    }

    if (!_attributes.CanModify(context.Actor, record))
    {
      await context.Output.WriteLineAsync("#-1 PERMISSION DENIED.", cancellationToken);
      return;
    }

    var enable = flag.Equals("ON", StringComparison.OrdinalIgnoreCase);
    attribute.Flags = enable ? "locked" : string.Empty;
    await context.Output.WriteLineAsync($"Attribute {attribute.Name} lock {(enable ? "enabled" : "disabled")}.", cancellationToken);
  }
}
