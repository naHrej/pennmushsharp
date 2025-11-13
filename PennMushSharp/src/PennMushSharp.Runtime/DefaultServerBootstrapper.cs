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

  public DefaultServerBootstrapper(
    ILogger<DefaultServerBootstrapper> logger,
    IMetadataCatalogs catalogs,
    InMemoryGameState gameState,
    ILockService lockService,
    GameStateLoader loader,
    IOptions<RuntimeOptions> options,
    IAccountRepository repository)
  {
    _logger = logger;
    _catalogs = catalogs;
    _gameState = gameState;
    _lockService = lockService;
    _loader = loader;
    _options = options.Value;
    _repository = repository;
  }

  public async Task StartAsync(CancellationToken cancellationToken = default)
  {
    if (!string.IsNullOrWhiteSpace(_options.InitialDumpPath))
    {
      await _loader.LoadAsync(_options.InitialDumpPath, cancellationToken);
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
}
