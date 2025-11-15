using PennMushSharp.Commands.Parsing;

namespace PennMushSharp.Commands;

public sealed class AttributeAssignmentCommand : ICommand
{
  private readonly AttributeService _attributes;

  public AttributeAssignmentCommand(AttributeService attributes)
  {
    _attributes = attributes;
  }

  public string Name => "ATTRIBUTE_ASSIGN";

  public bool CanHandle(CommandInvocation invocation) =>
    invocation.Name.Length > 1 && invocation.Name[0] == '&';

  public async ValueTask ExecuteAsync(ICommandContext context, CommandInvocation invocation, CancellationToken cancellationToken = default)
  {
    var attributeName = invocation.Name[1..].Trim();
    if (attributeName.Length == 0)
    {
      await context.Output.WriteLineAsync("#-1 WHAT ATTRIBUTE DO YOU WANT TO SET?", cancellationToken);
      return;
    }

    if (string.IsNullOrWhiteSpace(invocation.Argument))
    {
      await context.Output.WriteLineAsync("#-1 ATTRIBUTE CHANGE REQUIRES <target>=<value>.", cancellationToken);
      return;
    }

    var equalsIndex = invocation.Argument.IndexOf('=');
    string targetSpecifier;
    string value;
    if (equalsIndex < 0)
    {
      targetSpecifier = "me";
      value = invocation.Argument;
    }
    else
    {
      targetSpecifier = invocation.Argument[..equalsIndex].Trim();
      value = invocation.Argument[(equalsIndex + 1)..];
    }

    if (!_attributes.TryResolveTarget(context.Actor, targetSpecifier, out var record, out var error) || record is null)
    {
      await context.Output.WriteLineAsync(error ?? "#-1 NO SUCH OBJECT.", cancellationToken);
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
