using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using PennMushSharp.Commands;
using PennMushSharp.Commands.Parsing;
using PennMushSharp.Core;
using PennMushSharp.Core.Locks.Runtime;
using PennMushSharp.Core.Persistence;
using PennMushSharp.Functions;
using PennMushSharp.Runtime;
using Xunit;

namespace PennMushSharp.Tests.Runtime;

public sealed class CommandDispatcherTests
{
  [Fact]
  public async Task DispatchAsync_InvokesRegisteredCommand()
  {
    var catalog = new CommandCatalog();
    var command = new CaptureCommand();
    catalog.Register(command);
    catalog.RegisterAlias("cap", command);
    var parser = new CommandParser();
    var attributeCommand = new AttributeAssignmentCommand(new AttributeService(new InMemoryGameState(new InMemoryLockStore())));
    var dispatcher = new CommandDispatcher(
      catalog,
      parser,
      new PassThroughExpressionEvaluator(),
      attributeCommand,
      NullLogger<CommandDispatcher>.Instance);

    var actor = new GameObject(
      dbRef: 1,
      name: "One",
      type: GameObjectType.Player,
      owner: 1,
      location: null,
      flags: Array.Empty<string>(),
      attributes: new Dictionary<string, string>(),
      locks: new Dictionary<string, string>());
    var context = new TestContext(actor);
    await dispatcher.DispatchAsync(context, "capture payload");

    Assert.True(command.Executed);
    Assert.Equal("payload", command.Arguments);
  }

  private sealed class CaptureCommand : ICommand
  {
    public string Name => "CAPTURE";
    public bool Executed { get; private set; }
    public string? Arguments { get; private set; }

    public ValueTask ExecuteAsync(ICommandContext context, CommandInvocation invocation, CancellationToken cancellationToken = default)
    {
      Executed = true;
      Arguments = invocation.Argument;
      return ValueTask.CompletedTask;
    }
  }

  private sealed class TestContext : ICommandContext, IOutputWriter
  {
    private readonly RegisterSet _registers = new();

    public TestContext(GameObject actor)
    {
      Actor = actor;
      Expressions = new PassThroughExpressionEvaluator();
    }

    public GameObject Actor { get; }
    public IOutputWriter Output => this;
    public IFunctionEvaluator Functions { get; } = new PassThroughFunctionEvaluator();
    public IExpressionEvaluator Expressions { get; }
    public FunctionExecutionContext CreateFunctionContext(string? rawArguments) =>
      FunctionExecutionContext.FromRegisters(Actor, _registers, rawArguments);

    public void ResetRegisters() => _registers.ClearAll();

    public ValueTask WriteLineAsync(string text, CancellationToken cancellationToken = default)
    {
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
