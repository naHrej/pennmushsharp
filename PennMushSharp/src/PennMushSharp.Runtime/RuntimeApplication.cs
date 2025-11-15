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
using PennMushSharp.Functions.Builtins;

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
    services.AddSingleton<SpeechService>();
    services.AddSingleton<AttributeService>();
    services.AddSingleton<AttributeAssignmentCommand>();
    services.AddSingleton(sp => CreateFunctionRegistry(sp));
    services.AddSingleton<IFunctionEvaluator, FunctionEvaluator>();
    services.AddSingleton<IExpressionEvaluator, ExpressionEvaluator>();
    services.AddSingleton<TelnetServer>();
    services.AddHostedService(sp => sp.GetRequiredService<TelnetServer>());
    services.AddSingleton<IServerBootstrapper, DefaultServerBootstrapper>();
  }

  private static FunctionRegistry CreateFunctionRegistry(IServiceProvider provider)
  {
    var metadata = MetadataCatalogs.Default.Functions;
    var gameState = provider.GetRequiredService<InMemoryGameState>();
    var attributeService = provider.GetRequiredService<AttributeService>();
    var builder = new FunctionRegistryBuilder(metadata);
    builder.Add(new SetqFunction());
    builder.Add(new SetrFunction());
    builder.Add(new AddFunction());
    builder.Add(new SubFunction());
    builder.Add(new MulFunction());
    builder.Add(new DivFunction());
    builder.Add(new ModFunction());
    builder.Add(new AbsFunction());
    builder.Add(new MinFunction());
    builder.Add(new MaxFunction());
    builder.Add(new CeilFunction());
    builder.Add(new FloorFunction());
    builder.Add(new PiFunction());
    builder.Add(new PowerFunction());
    builder.Add(new SqrtFunction());
    builder.Add(new UpcaseFunction());
    builder.Add(new DowncaseFunction());
    builder.Add(new StrlenFunction());
    builder.Add(new TrimFunction());
    builder.Add(new LeftTrimFunction());
    builder.Add(new RightTrimFunction());
    builder.Add(new LeftFunction());
    builder.Add(new RightFunction());
    builder.Add(new MidFunction());
    builder.Add(new RepeatFunction());
    builder.Add(new RandFunction());
    builder.Add(new SinFunction());
    builder.Add(new CosFunction());
    builder.Add(new TanFunction());
    builder.Add(new AsinFunction());
    builder.Add(new AcosFunction());
    builder.Add(new AtanFunction());
    builder.Add(new Atan2Function());
    builder.Add(new LogFunction());
    builder.Add(new LnFunction());
    builder.Add(new RootFunction());
    builder.Add(new CtuFunction());
    builder.Add(new GetFunction(gameState));
    builder.Add(new XGetFunction(gameState));
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
