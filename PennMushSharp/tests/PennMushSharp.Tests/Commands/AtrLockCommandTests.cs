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

public sealed class AtrLockCommandTests
{
  [Fact]
  public async Task AtrLock_TogglesFlag()
  {
    var state = new InMemoryGameState(new InMemoryLockStore());
    var record = new GameObjectRecord { DbRef = 1, Name = "One" };
    record.SetAttribute("DESC", "Hello");
    state.Upsert(record);
    var actor = GameObject.FromRecord(record);
    var command = new AtrLockCommand(new AttributeService(state));
    var context = new TestContext(actor);

    var invocation = new CommandInvocation("@ATRLOCK", Array.Empty<CommandSwitch>(), null, "me/DESC=on", "@ATRLOCK me/DESC=on");
    await command.ExecuteAsync(context, invocation);

    Assert.Equal("locked", record.Attributes["DESC"].Flags);

    invocation = new CommandInvocation("@ATRLOCK", Array.Empty<CommandSwitch>(), null, "me/DESC=off", "@ATRLOCK me/DESC=off");
    await command.ExecuteAsync(context, invocation);

    Assert.Equal(string.Empty, record.Attributes["DESC"].Flags);
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
    public ValueTask<string> EvaluateAsync(FunctionExecutionContext context, string expression, CancellationToken cancellationToken = default) =>
      ValueTask.FromResult(expression);
  }

  private sealed class PassThroughExpressionEvaluator : IExpressionEvaluator
  {
    public ValueTask<string> EvaluateAsync(FunctionExecutionContext context, string input, CancellationToken cancellationToken = default) =>
      ValueTask.FromResult(input);
  }
}
