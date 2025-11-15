using PennMushSharp.Commands.Parsing;
using Xunit;

namespace PennMushSharp.Tests.Commands;

public sealed class CommandParserTests
{
  [Fact]
  public void Parse_PreservesEscapedBrackets()
  {
    var parser = new CommandParser();
    var invocations = parser.Parse(@"@eval -\[Test\] [repeat(-,3)]");

    Assert.Single(invocations);
    Assert.Equal(@"@eval -\[Test\] [repeat(-,3)]", invocations[0].Raw);
    Assert.Equal(@"-\[Test\] [repeat(-,3)]", invocations[0].Argument);
  }

  [Theory]
  [InlineData("'Hello there", "SAY", "Hello there")]
  [InlineData("\"Howdy", "SAY", "Howdy")]
  [InlineData(":waves happily.", "POSE", "waves happily.")]
  [InlineData(";--grins", "SEMIPOSE", "--grins")]
  [InlineData("\\sparkles", "@EMIT", "sparkles")]
  public void Parse_SupportsShorthandCommands(string input, string expectedName, string expectedArgument)
  {
    var parser = new CommandParser();

    var invocations = parser.Parse(input);

    Assert.Single(invocations);
    Assert.Equal(expectedName, invocations[0].Name);
    Assert.Equal(expectedArgument, invocations[0].Argument);
  }
}
