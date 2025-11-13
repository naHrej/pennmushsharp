using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PennMushSharp.Functions;

public sealed class FunctionEvaluator : IFunctionEvaluator
{
  private readonly FunctionRegistry _registry;

  public FunctionEvaluator(FunctionRegistry registry)
  {
    _registry = registry;
  }

  public async ValueTask<string> EvaluateAsync(FunctionExecutionContext context, string expression, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(expression))
      return string.Empty;

    var trimmed = expression.Trim();
    if (!TryParseInvocation(trimmed, out var name, out var argumentSegment))
      return trimmed;

    if (!_registry.TryGet(name, out var function) || function is null)
      return $"#-1 FUNCTION ({name}) NOT FOUND";

    var arguments = ParseArguments(argumentSegment);
    return await function.InvokeAsync(context, arguments, cancellationToken);
  }

  private static bool TryParseInvocation(string expression, out string name, out string arguments)
  {
    name = string.Empty;
    arguments = string.Empty;

    var openParen = expression.IndexOf('(');
    if (openParen < 0 || !expression.EndsWith(')'))
      return false;

    name = expression[..openParen].Trim();
    if (name.Length == 0)
      return false;

    arguments = expression[(openParen + 1)..^1];
    return true;
  }

  private static IReadOnlyList<string> ParseArguments(string argumentSegment)
  {
    if (string.IsNullOrEmpty(argumentSegment))
      return Array.Empty<string>();

    var results = new List<string>();
    var builder = new StringBuilder();
    var depth = 0;
    var braceDepth = 0;
    var inQuotes = false;
    var escape = false;

    void Commit()
    {
      results.Add(builder.ToString().Trim());
      builder.Clear();
    }

    foreach (var ch in argumentSegment)
    {
      if (escape)
      {
        builder.Append(ch);
        escape = false;
        continue;
      }

      if (ch == '\\')
      {
        escape = true;
        continue;
      }

      if (ch == '"')
        inQuotes = !inQuotes;
      else if (ch == '(')
        depth++;
      else if (ch == ')' && depth > 0)
        depth--;
      else if (ch == '{')
        braceDepth++;
      else if (ch == '}' && braceDepth > 0)
        braceDepth--;

      if (ch == ',' && depth == 0 && braceDepth == 0 && !inQuotes)
      {
        Commit();
        continue;
      }

      builder.Append(ch);
    }

    Commit();
    return results
      .Select(arg => arg.Trim())
      .ToArray();
  }
}
