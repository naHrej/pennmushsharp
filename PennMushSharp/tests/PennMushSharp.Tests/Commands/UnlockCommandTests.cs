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

public sealed class UnlockCommandTests
{
  [Fact]
  public async Task UnlockCommand_RemovesLock()
  {
    var state = new InMemoryGameState(new InMemoryLockStore());
    var record = new GameObjectRecord { DbRef = 1, Name = "One" };
    record.SetLock("ENTER", "me", 1);
    state.Upsert(record);
    var attributeService = new AttributeService(state);
    var lockStore = new InMemoryLockStore();
    lockStore.SetLock(1, new StoredLock("ENTER", "me"));
    var command = new UnlockCommand(attributeService, lockStore);
    var context = new TestContext(GameObject.FromRecord(record));
    var invocation = new CommandInvocation("@UNLOCK", Array.Empty<CommandSwitch>(), null, "me/Enter", "@UNLOCK me/Enter");

    await command.ExecuteAsync(context, invocation);

    Assert.False(record.Locks.ContainsKey("ENTER"));
  }

  private sealed class TestContext : ICommandContext, IOutputWriter
  {
    private readonly RegisterSet _registers = new();
    public TestContext(GameObject actor) => Actor = actor;
    public GameObject Actor { get; }
    public IOutputWriter Output => this;
    public IFunctionEvaluator Functions { get; } = new PassThroughFunctionEvaluator();
    public IExpressionEvaluator Expressions { get; } = new PassThroughExpressionEvaluator();
    public FunctionExecutionContext CreateFunctionContext(string? rawArguments) => FunctionExecutionContext.FromRegisters(Actor, _registers, rawArguments);
    public void ResetRegisters() => _registers.ClearAll();
    public ValueTask WriteLineAsync(string text, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
  }

  private sealed class PassThroughFunctionEvaluator : IFunctionEvaluator
  {
    public ValueTask<string> EvaluateAsync(FunctionExecutionContext context, string expression, CancellationToken cancellationToken = default)
      => ValueTask.FromResult(expression);
  }

  private sealed class PassThroughExpressionEvaluator : IExpressionEvaluator
  {
    public ValueTask<string> EvaluateAsync(FunctionExecutionContext context, string input, CancellationToken cancellationToken = default)
      => ValueTask.FromResult(input);
  }
}
