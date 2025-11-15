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

public sealed class SetCommandTests
{
  [Fact]
  public async Task SetCommand_WritesAttribute()
  {
    var state = new InMemoryGameState(new InMemoryLockStore());
    var record = new GameObjectRecord { DbRef = 1, Name = "One" };
    state.Upsert(record);
    var actor = GameObject.FromRecord(record);
    var command = new SetCommand(new AttributeService(state));
    var context = new TestContext(actor);
    var invocation = new CommandInvocation("@SET", Array.Empty<CommandSwitch>(), null, "me/DESC=Hello", "@SET me/DESC=Hello");

    await command.ExecuteAsync(context, invocation);

    Assert.Equal("Hello", record.Attributes["DESC"].Value);
  }

  [Fact]
  public async Task SetCommand_ClearsAttributeWhenValueEmpty()
  {
    var state = new InMemoryGameState(new InMemoryLockStore());
    var record = new GameObjectRecord { DbRef = 1, Name = "One" };
    record.SetAttribute("DESC", "Old");
    state.Upsert(record);
    var actor = GameObject.FromRecord(record);
    var command = new SetCommand(new AttributeService(state));
    var context = new TestContext(actor);
    var invocation = new CommandInvocation("@SET", Array.Empty<CommandSwitch>(), null, "me/DESC=", "@SET me/DESC=");

    await command.ExecuteAsync(context, invocation);

    Assert.False(record.Attributes.ContainsKey("DESC"));
  }

  private sealed class TestContext : ICommandContext, IOutputWriter
  {
    private readonly RegisterSet _registers = new();

    public TestContext(GameObject actor)
    {
      Actor = actor;
    }

    public GameObject Actor { get; }
    public IOutputWriter Output => this;
    public IFunctionEvaluator Functions { get; } = new PassThroughFunctionEvaluator();
    public IExpressionEvaluator Expressions { get; } = new PassThroughExpressionEvaluator();
    public FunctionExecutionContext CreateFunctionContext(string? rawArguments) =>
      FunctionExecutionContext.FromRegisters(Actor, _registers, rawArguments);

    public void ResetRegisters() => _registers.ClearAll();

    public ValueTask WriteLineAsync(string text, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
  }

  private sealed class PassThroughFunctionEvaluator : IFunctionEvaluator
  {
    public ValueTask<string> EvaluateAsync(FunctionExecutionContext context, string expression, CancellationToken cancellationToken = default) =>
      ValueTask.FromResult(expression);
  }

  private sealed class PassThroughExpressionEvaluator : IExpressionEvaluator
  {
    public ValueTask<string> EvaluateAsync(FunctionExecutionContext context, string input, CancellationToken cancellationToken = default) =>
      ValueTask.FromResult(input);
  }
}
