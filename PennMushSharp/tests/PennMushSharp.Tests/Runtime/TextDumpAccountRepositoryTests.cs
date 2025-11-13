using Microsoft.Extensions.Options;
using PennMushSharp.Core.Persistence;
using PennMushSharp.Runtime;
using Xunit;

namespace PennMushSharp.Tests.Runtime;

public sealed class TextDumpAccountRepositoryTests
{
  [Fact]
  public void SaveAndLoadRoundTrips()
  {
    var tempFile = Path.Combine(Path.GetTempPath(), $"accounts-{Guid.NewGuid():N}.dump");
    try
    {
      var options = Options.Create(new RuntimeOptions { AccountStorePath = tempFile });
      var parser = new TextDumpParser();
      var writer = new TextDumpWriter();
      var repository = new TextDumpAccountRepository(options, parser, writer);

      var record = new GameObjectRecord
      {
        DbRef = 5,
        Name = "Tester",
        Owner = 5
      };
      record.SetAttribute("XYXXY", "hash", owner: 5);

      repository.Save(record);

      var records = repository.LoadAll();
      Assert.Single(records);
      Assert.Equal(5, records.First().DbRef);
      Assert.Equal("Tester", records.First().Name);
      Assert.Equal("hash", records.First().Attributes["XYXXY"].Value);
    }
    finally
    {
      if (File.Exists(tempFile))
        File.Delete(tempFile);
    }
  }
}
