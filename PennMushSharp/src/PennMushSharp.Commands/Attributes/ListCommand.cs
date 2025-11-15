using PennMushSharp.Commands.Parsing;

namespace PennMushSharp.Commands;

public sealed class ListCommand : ICommand
{
  private readonly AttributeService _attributes;

  public ListCommand(AttributeService attributes)
  {
    _attributes = attributes;
  }

  public string Name => "@LIST";

  public async ValueTask ExecuteAsync(ICommandContext context, CommandInvocation invocation, CancellationToken cancellationToken = default)
  {
    var argument = invocation.Argument?.Trim();
    var (targetSpecifier, attributeFilter) = ParseArgument(argument);
    if (!_attributes.TryResolveTarget(context.Actor, targetSpecifier, out var record, out var error) || record is null)
    {
      await context.Output.WriteLineAsync(error ?? "#-1 NO SUCH OBJECT.", cancellationToken);
      return;
    }

    if (!string.IsNullOrWhiteSpace(attributeFilter))
    {
      var key = attributeFilter.Trim().ToUpperInvariant();
      if (record.Attributes.TryGetValue(key, out var attribute))
      {
        await context.Output.WriteLineAsync($"{attribute.Name} [{attribute.Value.Length}]: {attribute.Value}", cancellationToken);
      }
      else
      {
        await context.Output.WriteLineAsync("No such attribute.", cancellationToken);
      }
      return;
    }

    if (record.Attributes.Count == 0)
    {
      await context.Output.WriteLineAsync("No attributes.", cancellationToken);
      return;
    }

    await context.Output.WriteLineAsync($"Attributes for {record.Name ?? $"#{record.DbRef}"}:", cancellationToken);
    foreach (var attribute in record.Attributes.Values.OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase))
    {
      await context.Output.WriteLineAsync($"  {attribute.Name} [{attribute.Value.Length}]: {attribute.Value}", cancellationToken);
    }
  }

  private static (string Target, string? Attribute) ParseArgument(string? argument)
  {
    if (string.IsNullOrWhiteSpace(argument))
      return ("me", null);

    var slash = argument.IndexOf('/');
    if (slash < 0)
      return (argument, null);

    var target = argument[..slash].Trim();
    var attribute = argument[(slash + 1)..].Trim();
    if (target.Length == 0)
      target = "me";
    if (attribute.Length == 0)
      attribute = null;
    return (target, attribute);
  }
}
