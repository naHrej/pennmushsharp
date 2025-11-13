using System.Threading;
using System.Threading.Tasks;
using PennMushSharp.Commands;
using PennMushSharp.Commands.Parsing;
using PennMushSharp.Core;
using PennMushSharp.Functions;
using Xunit;

namespace PennMushSharp.Tests.Commands;

public sealed class EvalCommandTests
{
  [Fact]
  public async Task ExecuteAsync_EvaluatesExpression()
  {
    var command = new EvalCommand();
    var actor = new GameObject(1, "One", GameObjectType.Player, 1, null, Array.Empty<string>(), new Dictionary<string, string>(), new Dictionary<string, string>());
    var output = new TestOutput();
    var context = new TestContext(actor, output);
    var invocation = new CommandInvocation("@EVAL", Array.Empty<CommandSwitch>(), null, "Hello", "@eval Hello");

    await command.ExecuteAsync(context, invocation);

    Assert.Equal("Hello", Assert.Single(output.Lines));
  }

  private sealed class TestContext : ICommandContext
  {
    public TestContext(GameObject actor, IOutputWriter output)
    {
      Actor = actor;
      Output = output;
      Functions = new PassThroughEvaluator();
    }

    public GameObject Actor { get; }
    public IOutputWriter Output { get; }
    public IFunctionEvaluator Functions { get; }
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

  private sealed class PassThroughEvaluator : IFunctionEvaluator
  {
    public ValueTask<string> EvaluateAsync(GameObject actor, string expression, CancellationToken cancellationToken = default)
    {
      return ValueTask.FromResult(expression);
    }
  }
}
