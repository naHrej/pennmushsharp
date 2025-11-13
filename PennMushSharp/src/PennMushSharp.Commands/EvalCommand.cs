using PennMushSharp.Commands.Parsing;
using PennMushSharp.Core;
using PennMushSharp.Functions;

namespace PennMushSharp.Commands;

public sealed class EvalCommand : ICommand
{
  public string Name => "@EVAL";

  public async ValueTask ExecuteAsync(ICommandContext context, CommandInvocation invocation, CancellationToken cancellationToken = default)
  {
    var expression = invocation.Argument;
    if (string.IsNullOrWhiteSpace(expression))
    {
      await context.Output.WriteLineAsync("Usage: @EVAL <expression>", cancellationToken);
      return;
    }

    var execContext = context.CreateFunctionContext(invocation.Argument);
    var result = await context.Expressions.EvaluateAsync(execContext, expression, cancellationToken);
    await context.Output.WriteLineAsync(result, cancellationToken);
  }
}
