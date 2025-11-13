using Microsoft.Extensions.Logging;
using PennMushSharp.Core.Persistence;

namespace PennMushSharp.Runtime;

public sealed class GameStateLoader
{
  private readonly TextDumpParser _parser;
  private readonly InMemoryGameState _state;
  private readonly ILogger<GameStateLoader> _logger;

  public GameStateLoader(TextDumpParser parser, InMemoryGameState state, ILogger<GameStateLoader> logger)
  {
    _parser = parser;
    _state = state;
    _logger = logger;
  }

  public async Task<int> LoadAsync(string path, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(path))
      return 0;

    if (!File.Exists(path))
    {
      _logger.LogWarning("Initial dump path '{DumpPath}' does not exist. Skipping bootstrap load.", path);
      return 0;
    }

    await using var stream = File.OpenRead(path);
    var count = 0;
    foreach (var record in _parser.Parse(stream))
    {
      cancellationToken.ThrowIfCancellationRequested();
      _state.Upsert(record);
      count++;
    }

    _logger.LogInformation("Loaded {ObjectCount} objects from dump '{DumpPath}'.", count, path);
    return count;
  }
}
