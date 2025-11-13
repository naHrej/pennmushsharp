using System.Text;
using PennMushSharp.Core.Locks.Runtime;
using PennMushSharp.Core.Persistence;
using Xunit;

namespace PennMushSharp.Tests.Core;

public sealed class TextDumpIntegrationTests
{
  [Fact]
  public void ParseAndLoadPopulatesGameState()
  {
    var dump = "#5\nOne\n@lock/Control: #5\n***END OF DUMP***\n";
    var parser = new TextDumpParser();
    var store = new InMemoryLockStore();
    var state = new InMemoryGameState(store);

    using var reader = new StringReader(dump);
    foreach (var record in parser.Parse(reader))
    {
      state.Upsert(record);
    }

    Assert.True(store.TryGet(5, "Control", out var stored));
    Assert.Equal("#5", stored.Expression);
  }
}
