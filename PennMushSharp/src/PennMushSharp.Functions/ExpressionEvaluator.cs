using System.Globalization;
using System.Text;

namespace PennMushSharp.Functions;

public sealed class ExpressionEvaluator : IExpressionEvaluator
{
  private readonly IFunctionEvaluator _functionEvaluator;

  public ExpressionEvaluator(IFunctionEvaluator functionEvaluator)
  {
    _functionEvaluator = functionEvaluator;
  }

  public ValueTask<string> EvaluateAsync(FunctionExecutionContext context, string input, CancellationToken cancellationToken = default)
    => EvaluateInternalAsync(context, input, cancellationToken, allowLeadingInvocation: true);

  private async ValueTask<string> EvaluateInternalAsync(FunctionExecutionContext context, string input, CancellationToken cancellationToken, bool allowLeadingInvocation)
  {
    if (string.IsNullOrEmpty(input))
      return input ?? string.Empty;

    var builder = new StringBuilder(input.Length);
    var firstTokenProcessed = false;
    var escaped = false;
    for (var i = 0; i < input.Length; i++)
    {
      var current = input[i];

      if (escaped)
      {
        builder.Append(current);
        escaped = false;
        if (!firstTokenProcessed && !char.IsWhiteSpace(current))
          firstTokenProcessed = true;
        continue;
      }

      if (current == '\\')
      {
        escaped = true;
        continue;
      }

      if (current == '%' && !IsRegisterSpecifier(input, i))
      {
        if (i + 1 < input.Length && RequiresImmediatePercentEscape(input[i + 1]))
        {
          builder.Append(input[++i]);
          firstTokenProcessed = true;
          continue;
        }
      }

      if (!firstTokenProcessed)
      {
        if (char.IsWhiteSpace(current))
        {
          builder.Append(current);
          continue;
        }

        if (allowLeadingInvocation && TryExtractInvocation(input, i, out var invocation, out var nextIndex))
        {
          var preprocessedInvocation = await EvaluateInternalAsync(context, invocation, cancellationToken, allowLeadingInvocation: false);
          var evaluated = await _functionEvaluator.EvaluateAsync(context, preprocessedInvocation, cancellationToken);
          builder.Append(evaluated);
          i = nextIndex - 1;
          firstTokenProcessed = true;
          continue;
        }

        firstTokenProcessed = true;
      }

      if (current == '[')
      {
        var (inner, delta) = ExtractInnerExpression(input, i + 1);
        if (inner is null)
        {
          builder.Append(current);
          continue;
        }

        var preprocessed = await EvaluateInternalAsync(context, inner, cancellationToken, allowLeadingInvocation: true);
        var evaluated = await _functionEvaluator.EvaluateAsync(context, preprocessed, cancellationToken);
        builder.Append(evaluated);
        i = delta;
        continue;
      }

      builder.Append(current);
    }

    if (escaped)
      builder.Append('\\');

    return ExpandRegisters(builder.ToString(), context);
  }

  private static (string? Expression, int EndIndex) ExtractInnerExpression(string input, int start)
  {
    var depth = 1;
    var builder = new StringBuilder();
    var escaped = false;
    for (var i = start; i < input.Length; i++)
    {
      var ch = input[i];
      if (escaped)
      {
        builder.Append(ch);
        escaped = false;
        continue;
      }

      if (ch == '\\')
      {
        escaped = true;
        continue;
      }

      if (ch == '[')
      {
        depth++;
        builder.Append(ch);
        continue;
      }

      if (ch == ']')
      {
        depth--;
        if (depth == 0)
          return (builder.ToString(), i);
        builder.Append(ch);
        continue;
      }

      builder.Append(ch);
    }

    return (null, input.Length - 1);
  }

  private static string ExpandRegisters(string input, FunctionExecutionContext context)
  {
    if (string.IsNullOrEmpty(input))
      return string.Empty;

    var builder = new StringBuilder(input.Length);
    var escaped = false;
    for (var i = 0; i < input.Length; i++)
    {
      var ch = input[i];

      if (escaped)
      {
        builder.Append(ch);
        escaped = false;
        continue;
      }

      if (ch == '\\')
      {
        escaped = true;
        continue;
      }

      if (ch != '%')
      {
        builder.Append(ch);
        continue;
      }

      if (i + 1 >= input.Length)
      {
        builder.Append('%');
        continue;
      }

      var next = input[i + 1];
      if (next == '%')
      {
        builder.Append('%');
        i++;
        continue;
      }

      if (next == 'q' || next == 'Q')
      {
        var (identifier, consumed) = ReadIdentifier(input, i + 2);
        if (identifier.Length == 0)
        {
          builder.Append("%q");
          i++;
          continue;
        }

        builder.Append(context.GetRegister(identifier) ?? string.Empty);
        i = consumed;
        continue;
      }

      if (char.IsDigit(next))
      {
        var (digits, consumed) = ReadNumber(input, i + 1);
        if (digits.Length == 0)
        {
          builder.Append('%');
          continue;
        }

        var index = int.Parse(digits, CultureInfo.InvariantCulture);
        builder.Append(context.GetArgument(index) ?? string.Empty);
        i = consumed;
        continue;
      }

      builder.Append(next);
      i++;
      continue;
    }

    if (escaped)
      builder.Append('\\');

    return builder.ToString();
  }

  private static bool TryExtractInvocation(string input, int startIndex, out string invocation, out int nextIndex)
  {
    invocation = string.Empty;
    nextIndex = startIndex;

    if (!IsFunctionNameStart(input[startIndex]))
      return false;

    var index = startIndex + 1;
    while (index < input.Length && IsFunctionNameChar(input[index]))
      index++;

    if (index >= input.Length || input[index] != '(')
      return false;

    var depth = 1;
    var inQuotes = false;
    var escape = false;
    var cursor = index + 1;
    while (cursor < input.Length)
    {
      var ch = input[cursor];
      if (escape)
      {
        escape = false;
        cursor++;
        continue;
      }

      if (ch == '\\')
      {
        escape = true;
        cursor++;
        continue;
      }

      if (ch == '"')
      {
        inQuotes = !inQuotes;
        cursor++;
        continue;
      }

      if (inQuotes)
      {
        cursor++;
        continue;
      }

      if (ch == '(')
      {
        depth++;
      }
      else if (ch == ')')
      {
        depth--;
        if (depth == 0)
        {
          invocation = input[startIndex..(cursor + 1)];
          nextIndex = cursor + 1;
          return true;
        }
      }

      cursor++;
    }

    return false;
  }

  private static bool IsFunctionNameStart(char ch) => char.IsLetter(ch) || ch == '@' || ch == '_';

  private static bool IsFunctionNameChar(char ch) =>
    char.IsLetterOrDigit(ch) || ch == '_' || ch == '-' || ch == '#';

  private static (string Identifier, int ConsumedIndex) ReadIdentifier(string input, int start)
  {
    var builder = new StringBuilder();
    var i = start;
    while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_' || input[i] == '-'))
    {
      builder.Append(input[i]);
      i++;
    }

    return (builder.ToString(), i - 1);
  }

  private static (string Digits, int ConsumedIndex) ReadNumber(string input, int start)
  {
    var builder = new StringBuilder();
    var i = start;
    while (i < input.Length && char.IsDigit(input[i]))
    {
      builder.Append(input[i]);
      i++;
    }

    return (builder.ToString(), i - 1);
  }

  private static bool IsRegisterSpecifier(string input, int percentIndex)
  {
    if (percentIndex + 1 >= input.Length)
      return false;

    var next = input[percentIndex + 1];
    return next == 'q' || next == 'Q' || char.IsDigit(next);
  }

  private static bool RequiresImmediatePercentEscape(char ch)
  {
    return ch is '[' or ']' or '(' or ')';
  }
}
