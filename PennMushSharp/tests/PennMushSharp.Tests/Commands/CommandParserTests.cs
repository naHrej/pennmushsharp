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
}
