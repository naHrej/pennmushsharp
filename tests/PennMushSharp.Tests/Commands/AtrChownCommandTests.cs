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

public sealed class AtrChownCommandTests
{
  [Fact]
  public async Task AtrChown_ChangesOwner()
  {
    var state = new InMemoryGameState(new InMemoryLockStore());
    var first = new GameObjectRecord { DbRef = 1, Name = "One" };
    var second = new GameObjectRecord { DbRef = 2, Name = "Two" };
    first.SetAttribute("DESC", "hi");
    state.Upsert(first);
    state.Upsert(second);
    var actor = GameObject.FromRecord(first);
    var command = new AtrChownCommand(new AttributeService(state));
    var context = new TestContext(actor);
    var invocation = new CommandInvocation("@ATRCHOWN", Array.Empty<CommandSwitch>(), null, "me/DESC=Two", "@ATRCHOWN me/DESC=Two");

    await command.ExecuteAsync(context, invocation);

    Assert.Equal(2, first.Attributes["DESC"].Owner);
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
