using System.Text.RegularExpressions;

namespace PennMushSharp.Extraction;

public static class CommentUtilities
{
  private static readonly Regex BlockComments = new(@"/\*.*?\*/", RegexOptions.Singleline | RegexOptions.Compiled);
  private static readonly Regex LineComments = new(@"//.*?$", RegexOptions.Multiline | RegexOptions.Compiled);
  private static readonly Regex Continuations = new(@"\\\r?\n", RegexOptions.Compiled);

  public static string RemoveComments(string text)
  {
    var withoutBlock = BlockComments.Replace(text, string.Empty);
    return LineComments.Replace(withoutBlock, string.Empty);
  }

  public static string CombineContinuations(string text) => Continuations.Replace(text, string.Empty);
}
