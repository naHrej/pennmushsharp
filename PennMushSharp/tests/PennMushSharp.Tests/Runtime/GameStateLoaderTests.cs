using Microsoft.Extensions.Logging.Abstractions;
using PennMushSharp.Core.Locks.Runtime;
using PennMushSharp.Core.Persistence;
using PennMushSharp.Runtime;
using Xunit;

namespace PennMushSharp.Tests.Runtime;

public sealed class GameStateLoaderTests
{
  [Fact]
  public async Task LoadAsync_PopulatesGameState()
  {
    const string dump = """
      !1
      name "Sample"
      owner #1
      flags "WIZARD"
      attrcount 0
      lockcount 0
      ***END OF DUMP***
      """;

    var tempFile = Path.GetTempFileName();
    await File.WriteAllTextAsync(tempFile, dump);

    try
    {
      var parser = new TextDumpParser();
      var state = new InMemoryGameState(new InMemoryLockStore());
      var loader = new GameStateLoader(parser, state, NullLogger<GameStateLoader>.Instance);

      var count = await loader.LoadAsync(tempFile);

      Assert.Equal(1, count);
      Assert.True(state.TryGet(1, out var record));
      Assert.Equal("Sample", record?.Name);
    }
    finally
    {
      File.Delete(tempFile);
    }
  }
}
