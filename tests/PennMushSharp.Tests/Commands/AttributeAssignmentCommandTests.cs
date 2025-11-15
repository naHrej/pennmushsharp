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

public sealed class AttributeAssignmentCommandTests
{
  [Fact]
  public async Task SetsAttributeOnSelf()
  {
    var state = CreateState();
    var actorRecord = CreateRecord(1, "One");
    state.Upsert(actorRecord);
    var actor = GameObject.FromRecord(actorRecord);
    var command = new AttributeAssignmentCommand(new AttributeService(state));
    var context = new TestContext(actor);
    var invocation = new CommandInvocation("&DESC", Array.Empty<CommandSwitch>(), null, "me=Hello world", "&DESC me=Hello world");

    await command.ExecuteAsync(context, invocation);

    Assert.Equal("Hello world", actorRecord.Attributes["DESC"].Value);
  }

  [Fact]
  public async Task DeniesWhenActorDoesNotOwnTarget()
  {
    var state = CreateState();
    var actorRecord = CreateRecord(1, "One");
    var targetRecord = CreateRecord(2, "Two", owner: 2);
    state.Upsert(actorRecord);
    state.Upsert(targetRecord);
    var actor = GameObject.FromRecord(actorRecord);
    var command = new AttributeAssignmentCommand(new AttributeService(state));
    var context = new TestContext(actor);
    var invocation = new CommandInvocation("&DESC", Array.Empty<CommandSwitch>(), null, "Two=Hello", "&DESC Two=Hello");

    await command.ExecuteAsync(context, invocation);

    Assert.False(targetRecord.Attributes.ContainsKey("DESC"));
    Assert.Contains("#-1 PERMISSION DENIED.", context.Output.Lines);
  }

  private static InMemoryGameState CreateState() => new(new InMemoryLockStore());

  private static GameObjectRecord CreateRecord(int dbRef, string name, int? owner = null)
  {
    return new GameObjectRecord
    {
      DbRef = dbRef,
      Name = name,
      Owner = owner ?? dbRef
    };
  }

  private sealed class TestContext : ICommandContext
  {
    private readonly RegisterSet _registers = new();

    public TestContext(GameObject actor)
    {
      Actor = actor;
      Output = new TestOutput();
      Functions = new PassThroughFunctionEvaluator();
      Expressions = new PassThroughExpressionEvaluator();
    }

    public GameObject Actor { get; }
    public TestOutput Output { get; }
    IOutputWriter ICommandContext.Output => Output;
    public IFunctionEvaluator Functions { get; }
    public IExpressionEvaluator Expressions { get; }

    public FunctionExecutionContext CreateFunctionContext(string? rawArguments)
    {
      return FunctionExecutionContext.FromRegisters(Actor, _registers, rawArguments);
    }

    public void ResetRegisters() => _registers.ClearAll();
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
