using System;
using System.IO;
using System.Linq;
using PennMushSharp.Core;
using PennMushSharp.Core.Persistence;
using Xunit;

namespace PennMushSharp.Tests.Core;

public sealed class StockDumpCompatibilityTests
{
  [Fact]
  public void StockDump_ParsesPlayerOneWithoutPassword()
  {
    var dumpPath = Path.Combine(LocateRepositoryRoot(), "pennmush", "game", "data", "indb");
    if (!File.Exists(dumpPath))
      return; // upstream repo missing; skip at runtime

    using var stream = File.OpenRead(dumpPath);
    var parser = new TextDumpParser();
    var records = parser.Parse(stream).ToList();

    Assert.NotEmpty(records);
    var playerOne = records.Single(r => string.Equals(r.Name, "One", StringComparison.OrdinalIgnoreCase));
    Assert.Equal(GameObjectType.Player, playerOne.Type);
    Assert.Contains("WIZARD", playerOne.Flags);
    Assert.False(playerOne.Attributes.ContainsKey("XYXXY"));
  }

  private static string LocateRepositoryRoot()
  {
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "PennMushSharp.sln")))
      directory = directory.Parent;

    if (directory is null)
      throw new InvalidOperationException("Unable to locate repo root for stock dump test.");

    return directory.FullName;
  }
}
