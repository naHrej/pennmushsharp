using System.IO;
using System.Text;
using PennMushSharp.Core;
using PennMushSharp.Core.Persistence;
using Xunit;

namespace PennMushSharp.Tests.Core;

public sealed class TextDumpWriterTests
{
  [Fact]
  public void WriteAndParse_RoundTripsCoreFields()
  {
    var record = new GameObjectRecord
    {
      DbRef = 42,
      Name = "Sample Room",
      Type = GameObjectType.Room,
      Owner = 9,
      Location = 1,
      Pennies = 100
    };
    record.Flags.Add("ROOM");
    record.SetLock("Control", "=#9", creator: 9, flags: "default");
    record.SetAttribute("DESC", "Hello", owner: 9, flags: "locked");

    using var stream = new MemoryStream();
    var writer = new TextDumpWriter();
    writer.Write(new[] { record }, stream);
    stream.Position = 0;

    var parser = new TextDumpParser();
    using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
    var roundTrip = parser.Parse(reader).Single();

    Assert.Equal(record.DbRef, roundTrip.DbRef);
    Assert.Equal(record.Name, roundTrip.Name);
    Assert.Equal(record.Type, roundTrip.Type);
    Assert.Equal(record.Owner, roundTrip.Owner);
    Assert.Equal(record.Location, roundTrip.Location);
    Assert.Equal("=#9", roundTrip.Locks["Control"].Key);
    Assert.Equal("Hello", roundTrip.Attributes["DESC"].Value);
  }
}
