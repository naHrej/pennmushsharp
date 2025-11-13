using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PennMushSharp.Commands;
using PennMushSharp.Core;
using PennMushSharp.Core.Locks.Runtime;
using PennMushSharp.Core.Persistence;
using Xunit;

namespace PennMushSharp.Tests.Commands;

public sealed class LookCommandTests
{
  [Fact]
  public async Task ExecuteAsync_ShowsRoomDescriptionAndContents()
  {
    var state = BuildState();
    var command = new LookCommand(state);
    var actor = GameObject.FromRecord(GetRequired(state, 2));
    var output = new TestOutput();
    var context = new TestContext(actor, output);

    await command.ExecuteAsync(context, string.Empty);

    Assert.Contains("The Void", output.Lines[0]);
    Assert.Contains("You see Number One.", output.Lines[1]);
    Assert.Contains("Contents:", output.Lines[3]);
    Assert.Contains("One", output.Lines[4]);
  }

  private static InMemoryGameState BuildState()
  {
    var lockStore = new InMemoryLockStore();
    var state = new InMemoryGameState(lockStore);

    var room = new GameObjectRecord
    {
      DbRef = 10,
      Name = "The Void",
      Type = GameObjectType.Room,
      Contents = 2
    };
    room.SetAttribute("DESCRIBE", "You see Number One.");

    var player = new GameObjectRecord
    {
      DbRef = 2,
      Name = "One",
      Type = GameObjectType.Player,
      Location = room.DbRef,
      Next = -1
    };

    state.Upsert(room);
    state.Upsert(player);
    return state;
  }

  private static GameObjectRecord GetRequired(InMemoryGameState state, int dbRef)
  {
    if (!state.TryGet(dbRef, out var record) || record is null)
      throw new InvalidOperationException($"Missing record #{dbRef}");
    return record;
  }

  private sealed class TestOutput : IOutputWriter
  {
    public List<string> Lines { get; } = new();
    public ValueTask WriteLineAsync(string text, CancellationToken cancellationToken = default)
    {
      Lines.Add(text);
      return ValueTask.CompletedTask;
    }
  }

  private sealed class TestContext : ICommandContext
  {
    public TestContext(GameObject actor, IOutputWriter output)
    {
      Actor = actor;
      Output = output;
    }

    public GameObject Actor { get; }
    public IOutputWriter Output { get; }
  }
}
