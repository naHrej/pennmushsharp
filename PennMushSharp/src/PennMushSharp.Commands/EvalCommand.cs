using Microsoft.Extensions.Logging;
using PennMushSharp.Commands.Parsing;
using PennMushSharp.Core;
using PennMushSharp.Functions;

namespace PennMushSharp.Commands;

public sealed class EvalCommand : ICommand
{
  private readonly ILogger<EvalCommand> _logger;

  public EvalCommand(ILogger<EvalCommand> logger)
  {
    _logger = logger;
  }

  public string Name => "@EVAL";

  public async ValueTask ExecuteAsync(ICommandContext context, CommandInvocation invocation, CancellationToken cancellationToken = default)
  {
    var expression = invocation.Argument;
    if (string.IsNullOrWhiteSpace(expression))
    {
      await context.Output.WriteLineAsync("Usage: @EVAL <expression>", cancellationToken);
      return;
    }

    _logger.LogTrace("Eval expression before evaluation: {Expression}", expression);

    var execContext = context.CreateFunctionContext(invocation.Argument);
    var result = await context.Expressions.EvaluateAsync(execContext, expression, cancellationToken);
    _logger.LogTrace("Eval evaluated result: {Result}", result);
    await context.Output.WriteLineAsync(result, cancellationToken);
  }
}
