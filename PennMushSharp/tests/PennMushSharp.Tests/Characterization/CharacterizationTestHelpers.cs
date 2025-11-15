using System.IO;

namespace PennMushSharp.Tests.Characterization;

internal static class CharacterizationTestHelpers
{
  public static string RepositoryRoot { get; } = LocateRepositoryRoot();
  public static string CharacterizationRoot { get; } = Path.Combine(RepositoryRoot, "tests", "characterization");
  public static string GoldenDirectory { get; } = Path.Combine(CharacterizationRoot, "golden");
  public static string ScenarioDirectory { get; } = Path.Combine(CharacterizationRoot, "scenarios");

  private static string LocateRepositoryRoot()
  {
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "PennMushSharp.sln")))
      directory = directory.Parent;

    if (directory is null)
      throw new InvalidOperationException("Unable to find PennMushSharp.sln relative to the test binary.");

    return directory.FullName;
  }
}
