using Microsoft.Extensions.DependencyInjection;

namespace PennMushSharp.Commands;

public static class CommandCatalogBuilder
{
  public static CommandCatalog CreateDefault(IServiceProvider provider)
  {
    var catalog = new CommandCatalog();
    var registry = provider.GetRequiredService<ISessionRegistry>();
    catalog.Register(new LookCommand());
    catalog.Register(new WhoCommand(registry));
    return catalog;
  }
}
