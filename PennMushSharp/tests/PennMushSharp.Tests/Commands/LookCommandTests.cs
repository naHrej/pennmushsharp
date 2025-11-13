using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PennMushSharp.Commands;
using PennMushSharp.Commands.Parsing;
using PennMushSharp.Core;
using PennMushSharp.Core.Locks.Runtime;
using PennMushSharp.Core.Persistence;
using PennMushSharp.Functions;
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

    var invocation = new CommandInvocation("LOOK", Array.Empty<CommandSwitch>(), null, null, "LOOK");
    await command.ExecuteAsync(context, invocation);

    Assert.Equal("The Void (#10)", output.Lines[0]);
    Assert.Equal("You see Number One.", output.Lines[1]);
    Assert.Equal("Contents: Candle", output.Lines[2]);
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
      Next = 3
    };

    var candle = new GameObjectRecord
    {
      DbRef = 3,
      Name = "Candle",
      Type = GameObjectType.Thing,
      Location = room.DbRef,
      Next = -1
    };

    state.Upsert(room);
    state.Upsert(player);
    state.Upsert(candle);
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
    private readonly RegisterSet _registers = new();

    public TestContext(GameObject actor, IOutputWriter output)
    {
      Actor = actor;
      Output = output;
      Functions = new PassThroughFunctionEvaluator();
      Expressions = new PassThroughExpressionEvaluator();
    }

    public GameObject Actor { get; }
    public IOutputWriter Output { get; }
    public IFunctionEvaluator Functions { get; }
    public IExpressionEvaluator Expressions { get; }

    public FunctionExecutionContext CreateFunctionContext(string? rawArguments)
    {
      return FunctionExecutionContext.FromRegisters(Actor, _registers, rawArguments);
    }

    public void ResetRegisters()
    {
      _registers.ClearAll();
    }
  }

  private sealed class PassThroughFunctionEvaluator : IFunctionEvaluator
  {
    public ValueTask<string> EvaluateAsync(FunctionExecutionContext context, string expression, CancellationToken cancellationToken = default)
    {
      return ValueTask.FromResult(expression);
    }
  }

  private sealed class PassThroughExpressionEvaluator : IExpressionEvaluator
  {
    public ValueTask<string> EvaluateAsync(FunctionExecutionContext context, string input, CancellationToken cancellationToken = default)
    {
      return ValueTask.FromResult(input);
    }
  }
}
