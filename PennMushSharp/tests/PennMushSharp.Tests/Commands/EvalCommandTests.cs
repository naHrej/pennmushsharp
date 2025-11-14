using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using PennMushSharp.Commands;
using PennMushSharp.Commands.Parsing;
using PennMushSharp.Core;
using PennMushSharp.Core.Metadata;
using PennMushSharp.Functions;
using PennMushSharp.Functions.Builtins;
using Xunit;

namespace PennMushSharp.Tests.Commands;

public sealed class EvalCommandTests
{
  [Fact]
  public async Task ExecuteAsync_PreservesEscapedBrackets()
  {
    var command = new EvalCommand(NullLogger<EvalCommand>.Instance);
    var context = new TestContext(CreateEvaluator());
    var invocation = new CommandInvocation(
      "@EVAL",
      new List<CommandSwitch>(),
      null,
      @"-\[Test\] [repeat(-,3)]",
      "@EVAL -\\[Test\\] [repeat(-,3)]");

    await command.ExecuteAsync(context, invocation);

    Assert.StartsWith("-[Test]", Assert.Single(context.CapturedOutput));
  }

  private static ExpressionEvaluator CreateEvaluator()
  {
    var metadata = MetadataCatalogs.Default.Functions;
    var builder = new FunctionRegistryBuilder(metadata);
    builder.Add(new SetqFunction());
    builder.Add(new RepeatFunction());
    var registry = builder.Build();
    return new ExpressionEvaluator(new FunctionEvaluator(registry));
  }

  private sealed class TestContext : ICommandContext
  {
    private readonly ExpressionEvaluator _expressions;

    private readonly GameObject _actor;

    public TestContext(ExpressionEvaluator expressions)
    {
      _expressions = expressions;
      _actor = new GameObject(
        1,
        "One",
        GameObjectType.Player,
        owner: 1,
        location: null,
        flags: Array.Empty<string>(),
        attributes: new Dictionary<string, string>(),
        locks: new Dictionary<string, string>());
      Functions = new PassThroughFunctions();
    }

    public GameObject Actor => _actor;
    public IOutputWriter Output => _writer;
    public IFunctionEvaluator Functions { get; }
    public IExpressionEvaluator Expressions => _expressions;

    public FunctionExecutionContext CreateFunctionContext(string? rawArguments) =>
      FunctionExecutionContext.FromArguments(Actor, rawArguments);

    public void ResetRegisters()
    {
    }

    public IReadOnlyList<string> CapturedOutput => _writer.Lines;

    private readonly ListWriter _writer = new();

    private sealed class ListWriter : IOutputWriter
    {
      public List<string> Lines { get; } = new();

      public ValueTask WriteLineAsync(string text, CancellationToken cancellationToken = default)
      {
        Lines.Add(text);
        return ValueTask.CompletedTask;
      }
    }

    private sealed class PassThroughFunctions : IFunctionEvaluator
    {
      public ValueTask<string> EvaluateAsync(FunctionExecutionContext context, string expression, CancellationToken cancellationToken = default)
      {
        return ValueTask.FromResult(expression);
      }
    }
  }
}
