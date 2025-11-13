namespace PennMushSharp.Extraction;

public static class RepoLocator
{
  public static string FindRepositoryRoot()
  {
    var current = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (current != null)
    {
      if (Directory.Exists(Path.Combine(current.FullName, "PennMushSharp")) &&
          Directory.Exists(Path.Combine(current.FullName, "pennmush")))
      {
        return current.FullName;
      }

      current = current.Parent;
    }

    throw new InvalidOperationException("Could not locate repository root containing PennMushSharp and pennmush directories.");
  }
}
