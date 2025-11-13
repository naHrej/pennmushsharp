using PennMushSharp.Core;

namespace PennMushSharp.Commands;

/// <summary>
/// Registry for commands to keep early scaffolding testable while the exhaustive list is ported.
/// </summary>
public sealed class CommandCatalog
{
  private readonly Dictionary<string, ICommand> _commands = new(StringComparer.OrdinalIgnoreCase);

  public void Register(ICommand command) => _commands[command.Name] = command;

  public bool TryGet(string name, out ICommand? command) => _commands.TryGetValue(name, out command);
}

public interface ICommand
{
  string Name { get; }
  ValueTask ExecuteAsync(ICommandContext context, string arguments, CancellationToken cancellationToken = default);
}

public interface ICommandContext
{
  GameObject Actor { get; }
  IOutputWriter Output { get; }
}

public interface IOutputWriter
{
  ValueTask WriteLineAsync(string text, CancellationToken cancellationToken = default);
}
