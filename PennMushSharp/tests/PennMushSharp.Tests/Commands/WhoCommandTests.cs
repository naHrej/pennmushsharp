using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PennMushSharp.Commands;
using PennMushSharp.Commands.Parsing;
using PennMushSharp.Core;
using PennMushSharp.Core.Persistence;
using PennMushSharp.Functions;
using Xunit;

namespace PennMushSharp.Tests.Commands;

public sealed class WhoCommandTests
{
  [Fact]
  public async Task ExecuteAsync_FormatsColumns()
  {
    var now = DateTime.UtcNow;
    var playerRecord = new GameObjectRecord
    {
      DbRef = 1,
      Name = "One",
      Location = 0,
      Type = GameObjectType.Player
    };
    var player = GameObject.FromRecord(playerRecord);

    var registry = new FakeRegistry(new[]
    {
      new SessionInfo
      {
        Actor = player,
        ConnectedAtUtc = now.AddSeconds(-5),
        LastCommandUtc = now.AddSeconds(-2),
        CommandCount = 3,
        DescriptorCount = 20,
        Host = "test.host",
        Output = new TestOutput()
      }
    });

    var command = new WhoCommand(registry);
    var output = new TestOutput();
    var context = new TestContext(player, output);

    var invocation = new CommandInvocation("WHO", Array.Empty<CommandSwitch>(), null, null, "WHO");
    await command.ExecuteAsync(context, invocation);

    Assert.Equal("Player Name       Loc #    On For  Idle  Cmds Des  Host", output.Lines[0]);
    Assert.Contains("One", output.Lines[1]);
    Assert.Contains("#0", output.Lines[1]);
    Assert.Contains("test.host", output.Lines[1]);
    Assert.Equal("There is one player connected.", output.Lines[^1]);
  }

  private sealed class FakeRegistry : ISessionRegistry
  {
    private readonly IReadOnlyCollection<SessionInfo> _sessions;

    public FakeRegistry(IReadOnlyCollection<SessionInfo> sessions)
    {
      _sessions = sessions;
    }

    public IReadOnlyCollection<SessionInfo> GetActiveSessions() => _sessions;
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
