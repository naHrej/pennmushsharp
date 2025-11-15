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

public sealed class ListCommandTests
{
  [Fact]
  public async Task ListCommand_PrintsAllAttributes()
  {
    var state = new InMemoryGameState(new InMemoryLockStore());
    var record = new GameObjectRecord { DbRef = 1, Name = "One" };
    record.SetAttribute("DESC", "Hello");
    record.SetAttribute("TITLE", "Wizard");
    state.Upsert(record);

    var command = new ListCommand(new AttributeService(state));
    var actor = GameObject.FromRecord(record);
    var context = new TestContext(actor);
    await command.ExecuteAsync(context, new CommandInvocation("@LIST", Array.Empty<CommandSwitch>(), null, null, "@LIST"));

    Assert.Contains("Attributes for One:", context.Output.Lines);
    Assert.Contains("  DESC [5]: Hello", context.Output.Lines);
    Assert.Contains("  TITLE [6]: Wizard", context.Output.Lines);
  }

  [Fact]
  public async Task ListCommand_PrintsSingleAttributeWhenFiltered()
  {
    var state = new InMemoryGameState(new InMemoryLockStore());
    var record = new GameObjectRecord { DbRef = 1, Name = "One" };
    record.SetAttribute("DESC", "Hello");
    state.Upsert(record);

    var command = new ListCommand(new AttributeService(state));
    var actor = GameObject.FromRecord(record);
    var context = new TestContext(actor);
    await command.ExecuteAsync(context, new CommandInvocation("@LIST", Array.Empty<CommandSwitch>(), null, "me/DESC", "@LIST me/DESC"));

    Assert.Single(context.Output.Lines);
    Assert.Equal("DESC [5]: Hello", context.Output.Lines[0]);
  }

  private sealed class TestContext : ICommandContext
  {
    private readonly RegisterSet _registers = new();
    public TestContext(GameObject actor)
    {
      Actor = actor;
      Output = new TestOutput();
    }

    public GameObject Actor { get; }
    public TestOutput Output { get; }
    IOutputWriter ICommandContext.Output => Output;
    public IFunctionEvaluator Functions { get; } = new PassThroughFunctionEvaluator();
    public IExpressionEvaluator Expressions { get; } = new PassThroughExpressionEvaluator();
    public FunctionExecutionContext CreateFunctionContext(string? rawArguments) =>
      FunctionExecutionContext.FromRegisters(Actor, _registers, rawArguments);
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
      => ValueTask.FromResult(expression);
  }

  private sealed class PassThroughExpressionEvaluator : IExpressionEvaluator
  {
    public ValueTask<string> EvaluateAsync(FunctionExecutionContext context, string input, CancellationToken cancellationToken = default)
      => ValueTask.FromResult(input);
  }
}
