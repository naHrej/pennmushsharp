using System.Text;

namespace PennMushSharp.Extraction;

public static class PreprocessorUtilities
{
  public static string RemoveDirectives(string text)
  {
    var sb = new StringBuilder();
    using var reader = new StringReader(text);
    string? line;
    while ((line = reader.ReadLine()) is not null)
    {
      var trimmed = line.AsSpan().TrimStart();
      if (!trimmed.IsEmpty && trimmed[0] == '#')
        continue;
      sb.AppendLine(line);
    }
    return sb.ToString();
  }
}
