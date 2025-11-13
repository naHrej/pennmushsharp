using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using PennMushSharp.Commands;
using PennMushSharp.Core;
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
    var dispatcher = new CommandDispatcher(catalog, NullLogger<CommandDispatcher>.Instance);

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

    public ValueTask ExecuteAsync(ICommandContext context, string arguments, CancellationToken cancellationToken = default)
    {
      Executed = true;
      Arguments = arguments;
      return ValueTask.CompletedTask;
    }
  }

  private sealed class TestContext : ICommandContext, IOutputWriter
  {
    public TestContext(GameObject actor)
    {
      Actor = actor;
    }

    public GameObject Actor { get; }
    public IOutputWriter Output => this;

    public ValueTask WriteLineAsync(string text, CancellationToken cancellationToken = default)
    {
      return ValueTask.CompletedTask;
    }
  }
}
