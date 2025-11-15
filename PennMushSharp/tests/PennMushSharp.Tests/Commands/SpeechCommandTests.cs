using System;
using System.Collections.Generic;
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

public sealed class SpeechCommandTests
{
  [Fact]
  public async Task SayCommand_SendsMessageToRoom()
  {
    var registry = new FakeSessionRegistry();
    var actor = CreatePlayer(1, "One", 10);
    var other = CreatePlayer(2, "Two", 10);

    var actorOutput = new TestOutput();
    var otherOutput = new TestOutput();
    registry.Sessions.Add(CreateSession(actor, actorOutput));
    registry.Sessions.Add(CreateSession(other, otherOutput));

    var context = new TestContext(actor, actorOutput);
    var speech = CreateSpeechService(registry);
    var command = new SayCommand(registry, speech);

    var invocation = new CommandInvocation("SAY", Array.Empty<CommandSwitch>(), null, "Hello", "SAY Hello");
    await command.ExecuteAsync(context, invocation);

    Assert.Equal(new[] { "You say, \"Hello\"" }, actorOutput.Lines);
    Assert.Equal(new[] { "One says, \"Hello\"" }, otherOutput.Lines);
  }

  [Fact]
  public async Task PoseCommand_BroadcastsFormattedAction()
  {
    var registry = new FakeSessionRegistry();
    var actor = CreatePlayer(1, "One", 5);
    var other = CreatePlayer(2, "Two", 5);

    var actorOutput = new TestOutput();
    var otherOutput = new TestOutput();
    registry.Sessions.Add(CreateSession(actor, actorOutput));
    registry.Sessions.Add(CreateSession(other, otherOutput));

    var context = new TestContext(actor, actorOutput);
    var speech = CreateSpeechService(registry);
    var command = new PoseCommand(registry, speech);
    var invocation = new CommandInvocation("POSE", Array.Empty<CommandSwitch>(), null, "waves.", "POSE waves.");

    await command.ExecuteAsync(context, invocation);

    Assert.Equal(new[] { "You pose: One waves." }, actorOutput.Lines);
    Assert.Equal(new[] { "One waves." }, otherOutput.Lines);
  }

  [Fact]
  public async Task EmitCommand_SendsRawTextToRoom()
  {
    var registry = new FakeSessionRegistry();
    var actor = CreatePlayer(1, "One", 7);
    var other = CreatePlayer(2, "Two", 7);

    var actorOutput = new TestOutput();
    var otherOutput = new TestOutput();
    registry.Sessions.Add(CreateSession(actor, actorOutput));
    registry.Sessions.Add(CreateSession(other, otherOutput));

    var context = new TestContext(actor, actorOutput);
    var speech = CreateSpeechService(registry);
    var command = new EmitCommand(registry, speech);
    var invocation = new CommandInvocation("@EMIT", Array.Empty<CommandSwitch>(), null, "Shimmer", "@EMIT Shimmer");

    await command.ExecuteAsync(context, invocation);

    Assert.Equal(new[] { "Shimmer" }, actorOutput.Lines);
    Assert.Equal(new[] { "Shimmer" }, otherOutput.Lines);
  }

  [Fact]
  public async Task SemiposeCommand_OmitsSpaceBetweenNameAndAction()
  {
    var registry = new FakeSessionRegistry();
    var actor = CreatePlayer(1, "One", 9);
    var other = CreatePlayer(2, "Two", 9);
    var actorOutput = new TestOutput();
    var otherOutput = new TestOutput();
    registry.Sessions.Add(CreateSession(actor, actorOutput));
    registry.Sessions.Add(CreateSession(other, otherOutput));

    var context = new TestContext(actor, actorOutput);
    var speech = CreateSpeechService(registry);
    var command = new SemiposeCommand(registry, speech);
    var invocation = new CommandInvocation("SEMIPOSE", Array.Empty<CommandSwitch>(), null, "--grins", ";--grins");

    await command.ExecuteAsync(context, invocation);

    Assert.Equal(new[] { "You semipose: One--grins" }, actorOutput.Lines);
    Assert.Equal(new[] { "One--grins" }, otherOutput.Lines);
  }

  [Fact]
  public async Task WhisperCommand_DeliversToRoommates()
  {
    var registry = new FakeSessionRegistry();
    var actor = CreatePlayer(1, "One", 6);
    var other = CreatePlayer(2, "Two", 6);
    var actorOutput = new TestOutput();
    var otherOutput = new TestOutput();
    registry.Sessions.Add(CreateSession(actor, actorOutput));
    registry.Sessions.Add(CreateSession(other, otherOutput));

    var speech = CreateSpeechService(registry);
    var context = new TestContext(actor, actorOutput);
    var command = new WhisperCommand(registry, speech);
    var invocation = new CommandInvocation("WHISPER", Array.Empty<CommandSwitch>(), "Two", "Secret", "WHISPER Two=Secret");

    await command.ExecuteAsync(context, invocation);

    Assert.Equal(new[] { "You whisper, \"Secret\" to Two." }, actorOutput.Lines);
    Assert.Equal(new[] { "One whispers, \"Secret\"" }, otherOutput.Lines);
  }

  [Fact]
  public async Task WhisperCommand_FailsWhenTargetMissing()
  {
    var registry = new FakeSessionRegistry();
    var actor = CreatePlayer(1, "One", 6);
    var actorOutput = new TestOutput();
    registry.Sessions.Add(CreateSession(actor, actorOutput));

    var speech = CreateSpeechService(registry);
    var context = new TestContext(actor, actorOutput);
    var command = new WhisperCommand(registry, speech);
    var invocation = new CommandInvocation("WHISPER", Array.Empty<CommandSwitch>(), "Ghost", "Secret", "WHISPER Ghost=Secret");

    await command.ExecuteAsync(context, invocation);

    Assert.Equal(new[] { "#-1 TARGET NOT FOUND IN THIS ROOM." }, actorOutput.Lines);
  }

  [Fact]
  public async Task PemitCommand_TargetsNamedPlayer()
  {
    var registry = new FakeSessionRegistry();
    var actor = CreatePlayer(1, "One", 10);
    var other = CreatePlayer(2, "Two", 10);

    var actorOutput = new TestOutput();
    var otherOutput = new TestOutput();
    registry.Sessions.Add(CreateSession(actor, actorOutput));
    registry.Sessions.Add(CreateSession(other, otherOutput));

    var context = new TestContext(actor, actorOutput);
    var speech = CreateSpeechService(registry);
    var command = new PemitCommand(registry, speech);
    var invocation = new CommandInvocation("@PEMIT", Array.Empty<CommandSwitch>(), "Two", "Secret", "@PEMIT Two=Secret");

    await command.ExecuteAsync(context, invocation);

    Assert.Empty(actorOutput.Lines);
    Assert.Equal(new[] { "Secret" }, otherOutput.Lines);
  }

  [Fact]
  public async Task PemitCommand_HereTargetsRoom()
  {
    var registry = new FakeSessionRegistry();
    var actor = CreatePlayer(1, "One", 3);
    var other = CreatePlayer(2, "Two", 3);

    var actorOutput = new TestOutput();
    var otherOutput = new TestOutput();
    registry.Sessions.Add(CreateSession(actor, actorOutput));
    registry.Sessions.Add(CreateSession(other, otherOutput));

    var context = new TestContext(actor, actorOutput);
    var speech = CreateSpeechService(registry);
    var command = new PemitCommand(registry, speech);
    var invocation = new CommandInvocation("@PEMIT", Array.Empty<CommandSwitch>(), "here", "Announcement", "@PEMIT here=Announcement");

    await command.ExecuteAsync(context, invocation);

    Assert.Equal(new[] { "Announcement" }, actorOutput.Lines);
    Assert.Equal(new[] { "Announcement" }, otherOutput.Lines);
  }

  [Fact]
  public async Task PemitCommand_ErrorsWhenTargetMissing()
  {
    var registry = new FakeSessionRegistry();
    var actor = CreatePlayer(1, "One", 3);
    var actorOutput = new TestOutput();
    registry.Sessions.Add(CreateSession(actor, actorOutput));

    var context = new TestContext(actor, actorOutput);
    var speech = CreateSpeechService(registry);
    var command = new PemitCommand(registry, speech);
    var invocation = new CommandInvocation("@PEMIT", Array.Empty<CommandSwitch>(), "Ghost", "Ping", "@PEMIT Ghost=Ping");

    await command.ExecuteAsync(context, invocation);

    Assert.Equal(new[] { "#-1 TARGET NOT CONNECTED." }, actorOutput.Lines);
  }

  [Fact]
  public async Task PageCommand_SendsToTargetsAnywhere()
  {
    var registry = new FakeSessionRegistry();
    var actor = CreatePlayer(1, "One", 2);
    var other = CreatePlayer(2, "Two", 5);
    var actorOutput = new TestOutput();
    var otherOutput = new TestOutput();
    registry.Sessions.Add(CreateSession(actor, actorOutput));
    registry.Sessions.Add(CreateSession(other, otherOutput));

    var speech = CreateSpeechService(registry);
    var context = new TestContext(actor, actorOutput);
    var command = new PageCommand(registry, speech);
    var invocation = new CommandInvocation("PAGE", Array.Empty<CommandSwitch>(), "Two", "Ping", "PAGE Two=Ping");

    await command.ExecuteAsync(context, invocation);

    Assert.Equal(new[] { "You page Two with \"Ping\"" }, actorOutput.Lines);
    Assert.Equal(new[] { "One pages: \"Ping\"" }, otherOutput.Lines);
  }

  [Fact]
  public async Task PageCommand_RespectsHaven()
  {
    var registry = new FakeSessionRegistry();
    var actor = CreatePlayer(1, "One", 2);
    var haven = CreatePlayer(2, "Two", 5, "HAVEN");
    var actorOutput = new TestOutput();
    registry.Sessions.Add(CreateSession(actor, actorOutput));
    registry.Sessions.Add(CreateSession(haven, new TestOutput()));

    var speech = CreateSpeechService(registry);
    var context = new TestContext(actor, actorOutput);
    var command = new PageCommand(registry, speech);
    var invocation = new CommandInvocation("PAGE", Array.Empty<CommandSwitch>(), "Two", "Ping", "PAGE Two=Ping");

    await command.ExecuteAsync(context, invocation);

    Assert.Equal(new[] { "I'm sorry, but Two wishes to be left alone now." }, actorOutput.Lines);
  }

  [Fact]
  public async Task PemitCommand_RespectsHavenFlag()
  {
    var registry = new FakeSessionRegistry();
    var actor = CreatePlayer(1, "One", 4);
    var haven = CreatePlayer(2, "Two", 4, "HAVEN");

    var actorOutput = new TestOutput();
    registry.Sessions.Add(CreateSession(actor, actorOutput));
    registry.Sessions.Add(CreateSession(haven, new TestOutput()));

    var speech = CreateSpeechService(registry);
    var context = new TestContext(actor, actorOutput);
    var command = new PemitCommand(registry, speech);
    var invocation = new CommandInvocation("@PEMIT", Array.Empty<CommandSwitch>(), "Two", "Secret", "@PEMIT Two=Secret");

    await command.ExecuteAsync(context, invocation);

    Assert.Equal(new[] { "I'm sorry, but Two wishes to be left alone now." }, actorOutput.Lines);
  }

  private static SpeechService CreateSpeechService(ISessionRegistry registry, ILockService? lockService = null) =>
    new SpeechService(registry, lockService ?? new AllowAllLockService());

  private static SessionInfo CreateSession(GameObject actor, IOutputWriter output)
  {
    return new SessionInfo
    {
      Actor = actor,
      ConnectedAtUtc = DateTime.UtcNow,
      LastCommandUtc = DateTime.UtcNow,
      Host = "local",
      Output = output,
      DescriptorCount = 1,
      CommandCount = 0
    };
  }

  private static GameObject CreatePlayer(int dbRef, string name, int? location, params string[] flags)
  {
    var record = new GameObjectRecord
    {
      DbRef = dbRef,
      Name = name,
      Type = GameObjectType.Player,
      Location = location
    };
    foreach (var flag in flags)
      record.Flags.Add(flag);
    return GameObject.FromRecord(record);
  }

  private sealed class FakeSessionRegistry : ISessionRegistry
  {
    public List<SessionInfo> Sessions { get; } = new();
    public IReadOnlyCollection<SessionInfo> GetActiveSessions() => Sessions;
  }

  private sealed class AllowAllLockService : ILockService
  {
    public bool Evaluate(LockRequest request) => true;
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

  private sealed class TestContext : ICommandContext
  {
    private readonly RegisterSet _registers = new();

    public TestContext(GameObject actor, IOutputWriter output)
    {
      Actor = actor;
      Output = output;
      Functions = new PassThroughFunctionEvaluator();
      Expressions = new PassThroughExpressionEvaluator();
    }

    public GameObject Actor { get; }
    public IOutputWriter Output { get; }
    public IFunctionEvaluator Functions { get; }
    public IExpressionEvaluator Expressions { get; }

    public FunctionExecutionContext CreateFunctionContext(string? rawArguments)
    {
      return FunctionExecutionContext.FromRegisters(Actor, _registers, rawArguments);
    }

    public void ResetRegisters()
    {
      _registers.ClearAll();
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
