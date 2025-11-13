using PennMushSharp.Commands.Parsing;
using Xunit;

namespace PennMushSharp.Tests.Commands;

public sealed class CommandParserTests
{
  private readonly CommandParser _parser = new();

  [Fact]
  public void Parse_SingleCommand()
  {
    var invocations = _parser.Parse("look here");

    Assert.Single(invocations);
    var invocation = invocations[0];
    Assert.Equal("look", invocation.Name);
    Assert.Empty(invocation.Switches);
    Assert.Null(invocation.Target);
    Assert.Equal("here", invocation.Argument);
  }

  [Fact]
  public void Parse_CommandWithSwitchesAndArgument()
  {
    var invocations = _parser.Parse("@dig/teleport/quiet room=desc");

    var command = Assert.Single(invocations);
    Assert.Equal("@dig", command.Name);
    Assert.Collection(command.Switches,
      s => { Assert.Equal("teleport", s.Name); Assert.Null(s.Argument); },
      s => { Assert.Equal("quiet", s.Name); Assert.Null(s.Argument); });
    Assert.Equal("room", command.Target);
    Assert.Equal("desc", command.Argument);
  }

  [Fact]
  public void Parse_SwitchArguments()
  {
    var invocations = _parser.Parse("@page/silent:room target=Hello");

    var command = Assert.Single(invocations);
    var switchInfo = Assert.Single(command.Switches);
    Assert.Equal("silent", switchInfo.Name);
    Assert.Equal("room", switchInfo.Argument);
  }

  [Fact]
  public void Parse_StackedCommands()
  {
    var invocations = _parser.Parse("say Hello;look here");

    Assert.Equal(2, invocations.Count);
    Assert.Equal("say", invocations[0].Name);
    Assert.Equal("Hello", invocations[0].Argument);
    Assert.Equal("look", invocations[1].Name);
    Assert.Equal("here", invocations[1].Argument);
  }

  [Fact]
  public void Parse_StackedWithAmpersand()
  {
    var invocations = _parser.Parse("say Hello & drop torch");

    Assert.Equal(2, invocations.Count);
    Assert.Equal("say", invocations[0].Name);
    Assert.Equal("Hello", invocations[0].Argument);
    Assert.Equal("drop", invocations[1].Name);
    Assert.Equal("torch", invocations[1].Argument);
  }
}
