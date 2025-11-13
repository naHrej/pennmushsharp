namespace PennMushSharp.Functions;

public interface IExpressionEvaluator
{
  ValueTask<string> EvaluateAsync(FunctionExecutionContext context, string input, CancellationToken cancellationToken = default);
}
