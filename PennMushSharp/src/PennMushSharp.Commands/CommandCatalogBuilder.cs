using Microsoft.Extensions.DependencyInjection;
using PennMushSharp.Core.Persistence;

namespace PennMushSharp.Commands;

public static class CommandCatalogBuilder
{
  public static CommandCatalog CreateDefault(IServiceProvider provider)
  {
    var catalog = new CommandCatalog();
    var registry = provider.GetRequiredService<ISessionRegistry>();
    var gameState = provider.GetRequiredService<InMemoryGameState>();
    catalog.Register(new LookCommand(gameState));
    catalog.Register(new WhoCommand(registry));
    return catalog;
  }
}
