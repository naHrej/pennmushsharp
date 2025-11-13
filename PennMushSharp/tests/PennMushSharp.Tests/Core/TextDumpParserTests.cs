using System.Text;
using PennMushSharp.Core.Persistence;
using Xunit;

namespace PennMushSharp.Tests.Core;

public sealed class TextDumpParserTests
{
  [Fact]
  public void Parse_ExtractsLocks()
  {
    var dump = "#5\nOne\n@lock/Control: #5\n@lock/Use: TRUE_BOOLEXP\n***END RECORD***\n#6\nTwo\n@lock/Control: #5\n***END OF DUMP***\n";
    var parser = new TextDumpParser();
    using var reader = new StringReader(dump);

    var records = parser.Parse(reader).ToList();

    Assert.Equal(2, records.Count);
    Assert.Equal(2, records[0].Locks.Count);
    Assert.Equal("#5", records[0].Locks["Control"]);
    Assert.Equal("TRUE_BOOLEXP", records[0].Locks["Use"]);
    Assert.Single(records[1].Locks);
  }

  [Fact]
  public void Parse_ModernFormat_ExtractsCoreFields()
  {
    const string dump = """
      !9
      name "Wizard9"
      owner #9
      flags "WIZARD SAFE ANSI"
      attrcount 0
      lockcount 0
      ***END OF DUMP***
      """;

    var parser = new TextDumpParser();
    using var reader = new StringReader(dump);

    var record = parser.Parse(reader).Single();
    Assert.Equal(9, record.DbRef);
    Assert.Equal("Wizard9", record.Name);
    Assert.Equal(9, record.Owner);
    Assert.Equal(new[] { "WIZARD", "SAFE", "ANSI" }, record.Flags);
  }

  [Fact]
  public void Parse_ModernFormat_ReadsAttributesAndLocks()
  {
    const string dump = """
      !9
      name "Wizard9"
      owner #9
      flags "WIZARD"
      lockcount 1
       type "Basic"
        creator #1
        key "=#9"
      attrcount 2
       name "ICLOC"
        owner #1
        flags "locked"
        value " "
       name "GREETING"
        owner #1
        flags "locked"
        value "Hello, World!"
      ***END OF DUMP***
      """;

    var parser = new TextDumpParser();
    using var reader = new StringReader(dump);

    var record = parser.Parse(reader).Single();
    Assert.Equal("=#9", record.Locks["Basic"]);
    Assert.Equal(" ", record.Attributes["ICLOC"]);
    Assert.Equal("Hello, World!", record.Attributes["GREETING"]);
  }
}
