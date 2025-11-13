using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PennMushSharp.Commands;
using PennMushSharp.Core.Locks;
using PennMushSharp.Core.Locks.Runtime;
using PennMushSharp.Core.Metadata;
using PennMushSharp.Core.Persistence;

namespace PennMushSharp.Runtime;

public static class RuntimeApplication
{
  public static IHost BuildHost(string[]? args = null)
  {
    var builder = Host.CreateApplicationBuilder(args ?? Array.Empty<string>());
    ConfigureServices(builder.Services, builder.Configuration);
    return builder.Build();
  }

  private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
  {
    services.Configure<RuntimeOptions>(configuration.GetSection("PennMushSharp"));
    services.AddSingleton<IMetadataCatalogs>(_ => MetadataCatalogs.Default);
    services.AddSingleton<ILockMetadataService>(_ => LockMetadataService.CreateDefault());
    services.AddSingleton<IMutableLockStore, InMemoryLockStore>();
    services.AddSingleton<ILockStore>(sp => sp.GetRequiredService<IMutableLockStore>());
    services.AddSingleton<ILockExpressionEngine>(_ => SimpleLockExpressionEngine.Instance);
    services.AddSingleton<ILockService, LockEvaluator>();
    services.AddSingleton<TextDumpParser>();
    services.AddSingleton<InMemoryGameState>();
    services.AddSingleton<GameStateLoader>();
    services.AddSingleton<PasswordVerifier>();
    services.AddSingleton<SessionRegistry>();
    services.AddSingleton<ISessionRegistry>(sp => sp.GetRequiredService<SessionRegistry>());
    services.AddSingleton(sp => CommandCatalogBuilder.CreateDefault(sp));
    services.AddSingleton<CommandDispatcher>();
    services.AddSingleton<TelnetServer>();
    services.AddHostedService(sp => sp.GetRequiredService<TelnetServer>());
    services.AddSingleton<IServerBootstrapper, DefaultServerBootstrapper>();
  }
}
