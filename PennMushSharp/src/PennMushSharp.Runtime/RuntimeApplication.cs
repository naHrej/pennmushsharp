using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PennMushSharp.Commands;
using PennMushSharp.Commands.Parsing;
using PennMushSharp.Core.Locks;
using PennMushSharp.Core.Locks.Runtime;
using PennMushSharp.Core.Metadata;
using PennMushSharp.Core.Persistence;
using PennMushSharp.Functions;

namespace PennMushSharp.Runtime;

public static class RuntimeApplication
{
  public static IHost BuildHost(string[]? args = null)
  {
    var normalizedArgs = args ?? Array.Empty<string>();
    var builder = Host.CreateApplicationBuilder(normalizedArgs);
    var executableConfig = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    if (File.Exists(executableConfig))
      builder.Configuration.AddJsonFile(executableConfig, optional: true, reloadOnChange: false);
    if (normalizedArgs.Length > 0)
      builder.Configuration.AddCommandLine(normalizedArgs);
    ConfigureLogging(builder);
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
    services.AddSingleton<TextDumpWriter>();
    services.AddSingleton<InMemoryGameState>();
    services.AddSingleton<GameStateLoader>();
    services.AddSingleton<PasswordVerifier>();
    services.AddSingleton<IAccountRepository, TextDumpAccountRepository>();
    services.AddSingleton<AccountService>();
    services.AddSingleton<SessionRegistry>();
    services.AddSingleton<ISessionRegistry>(sp => sp.GetRequiredService<SessionRegistry>());
    services.AddSingleton<CommandParser>();
    services.AddSingleton(sp => CommandCatalogBuilder.CreateDefault(sp));
    services.AddSingleton<CommandDispatcher>();
    services.AddSingleton(CreateFunctionRegistry());
    services.AddSingleton<IFunctionEvaluator, FunctionEvaluator>();
    services.AddSingleton<IExpressionEvaluator, ExpressionEvaluator>();
    services.AddSingleton<TelnetServer>();
    services.AddHostedService(sp => sp.GetRequiredService<TelnetServer>());
    services.AddSingleton<IServerBootstrapper, DefaultServerBootstrapper>();
  }

  private static FunctionRegistry CreateFunctionRegistry()
  {
    var builder = new FunctionRegistryBuilder();
    builder.Add(new PennMushSharp.Functions.Builtins.SetqFunction());
    return builder.Build();
  }

  private static void ConfigureLogging(HostApplicationBuilder builder)
  {
    builder.Logging.ClearProviders();
    builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    builder.Logging.AddSimpleConsole(options =>
    {
      options.TimestampFormat = "HH:mm:ss.fff ";
      options.IncludeScopes = true;
      options.SingleLine = true;
    });
  }
}
