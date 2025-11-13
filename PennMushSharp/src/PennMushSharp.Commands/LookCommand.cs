using PennMushSharp.Core;

namespace PennMushSharp.Commands;

public sealed class LookCommand : ICommand
{
  public string Name => "LOOK";

  public ValueTask ExecuteAsync(ICommandContext context, string arguments, CancellationToken cancellationToken = default)
  {
    return context.Output.WriteLineAsync($"You are {context.Actor.Name} standing in Room Zero. The full room model is still under construction.", cancellationToken);
  }
}
