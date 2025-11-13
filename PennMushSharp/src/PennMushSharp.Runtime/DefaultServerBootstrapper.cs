using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PennMushSharp.Core.Locks.Runtime;
using PennMushSharp.Core.Metadata;
using PennMushSharp.Core.Persistence;

namespace PennMushSharp.Runtime;

public sealed class DefaultServerBootstrapper : IServerBootstrapper
{
  private readonly ILogger<DefaultServerBootstrapper> _logger;
  private readonly IMetadataCatalogs _catalogs;
  private readonly InMemoryGameState _gameState;
  private readonly ILockService _lockService;
  private readonly GameStateLoader _loader;
  private readonly RuntimeOptions _options;
  private readonly IAccountRepository _repository;
  private readonly string _contentRoot;

  public DefaultServerBootstrapper(
    ILogger<DefaultServerBootstrapper> logger,
    IMetadataCatalogs catalogs,
    InMemoryGameState gameState,
    ILockService lockService,
    GameStateLoader loader,
    IOptions<RuntimeOptions> options,
    IAccountRepository repository,
    IHostEnvironment environment)
  {
    _logger = logger;
    _catalogs = catalogs;
    _gameState = gameState;
    _lockService = lockService;
    _loader = loader;
    _options = options.Value;
    _repository = repository;
    _contentRoot = environment.ContentRootPath;
  }

  public async Task StartAsync(CancellationToken cancellationToken = default)
  {
    var dumpPath = ResolvePath(_options.InitialDumpPath);
    if (string.IsNullOrWhiteSpace(dumpPath))
    {
      _logger.LogInformation("No initial dump configured. Skipping bootstrap load.");
    }
    else
    {
      _logger.LogInformation("Resolved initial dump path to {DumpPath}", dumpPath);
      await _loader.LoadAsync(dumpPath, cancellationToken);
    }

    GameStateSeeder.EnsureDefaultWizard(_gameState, _options.DefaultAccountDbRef, _options.DefaultAccountName);
    foreach (var record in _repository.LoadAll())
      _gameState.Upsert(record);

    _logger.LogInformation(
      "Runtime bootstrap complete. Flags={FlagCount}, Attributes={AttributeCount}, Locks={LockCount}, Functions={FunctionCount}",
      _catalogs.Flags.Flags.Count,
      _catalogs.Attributes.Attributes.Count,
      _catalogs.Locks.Locks.Count,
      _catalogs.Functions.Functions.Count);

    _logger.LogDebug("Lock service ready: {ServiceType}", _lockService.GetType().Name);
    _logger.LogTrace("In-memory state type: {Type}", _gameState.GetType().Name);
  }

  private string? ResolvePath(string? configured)
  {
    if (string.IsNullOrWhiteSpace(configured))
      return null;
    if (Path.IsPathRooted(configured))
      return configured;
    return Path.GetFullPath(Path.Combine(_contentRoot, configured));
  }
}
