using Microsoft.Extensions.Logging;
using PennMushSharp.Commands;
using PennMushSharp.Commands.Metadata;
using PennMushSharp.Commands.Parsing;
using PennMushSharp.Functions;

namespace PennMushSharp.Runtime;

public sealed class CommandDispatcher
{
  private readonly CommandCatalog _catalog;
  private readonly CommandParser _parser;
  private readonly AttributeAssignmentCommand _attributeCommand;
  private readonly ILogger<CommandDispatcher> _logger;
  private readonly IExpressionEvaluator _expressionEvaluator;

  public CommandDispatcher(
    CommandCatalog catalog,
    CommandParser parser,
    IExpressionEvaluator expressionEvaluator,
    AttributeAssignmentCommand attributeCommand,
    ILogger<CommandDispatcher> logger)
  {
    _catalog = catalog;
    _parser = parser;
    _expressionEvaluator = expressionEvaluator;
    _attributeCommand = attributeCommand;
    _logger = logger;
  }

  public async ValueTask DispatchAsync(ICommandContext context, string input, CancellationToken cancellationToken = default)
  {
    var invocations = _parser.Parse(input);
    if (invocations.Count == 0)
      return;

    foreach (var invocation in invocations)
    {
      _logger.LogTrace("Dispatching raw='{Raw}' argument='{Argument}' target='{Target}'", invocation.Raw, invocation.Argument, invocation.Target);
      if (!_catalog.TryGet(invocation.Name, out var command) || command is null)
      {
        if (_attributeCommand.CanHandle(invocation))
        {
          command = _attributeCommand;
        }
        else
        {
          _logger.LogWarning("Unknown command '{Command}' for actor #{Actor}", invocation.Name, context.Actor.DbRef);
          await context.Output.WriteLineAsync("Huh? (Try HELP)", cancellationToken);
          continue;
        }
      }

      CommandDefinition? metadata = null;
      if (CommandMetadataCatalog.TryGet(command.Name, out metadata) && metadata is not null)
      {
        if (metadata.WizardOnly)
        {
          await context.Output.WriteLineAsync("Permission denied.", cancellationToken);
          continue;
        }

        if (!ValidateSwitches(metadata.Switches, invocation.Switches, context, cancellationToken))
          continue;
      }

      var preparedInvocation = await PrepareInvocationAsync(context, invocation, metadata, cancellationToken);
      await command.ExecuteAsync(context, preparedInvocation, cancellationToken);
      context.ResetRegisters();
    }
  }

  private static bool ValidateSwitches(
    IReadOnlyList<CommandSwitchDefinition> definitions,
    IReadOnlyList<CommandSwitch> switches,
    ICommandContext context,
    CancellationToken cancellationToken)
  {
    foreach (var commandSwitch in switches)
    {
      var definition = definitions.FirstOrDefault(d => d.Name.Equals(commandSwitch.Name, StringComparison.OrdinalIgnoreCase));
      if (definition is null)
      {
        context.Output.WriteLineAsync($"Unknown switch /{commandSwitch.Name}", cancellationToken);
        return false;
      }

      if (definition.RequiresArgument && string.IsNullOrEmpty(commandSwitch.Argument))
      {
        context.Output.WriteLineAsync($"Switch /{definition.Name} requires an argument.", cancellationToken);
        return false;
      }
    }

    return true;
  }

  private async ValueTask<CommandInvocation> PrepareInvocationAsync(
    ICommandContext context,
    CommandInvocation invocation,
    CommandDefinition? metadata,
    CancellationToken cancellationToken)
  {
    var target = invocation.Target;
    var argument = invocation.Argument;
    var shouldEvalTarget = target is not null && ShouldEvaluateTarget(metadata, invocation);
    var shouldEvalArgument = argument is not null && ShouldEvaluateArgument(metadata, invocation);

    if (!shouldEvalTarget && !shouldEvalArgument)
      return invocation;

    var execContext = context.CreateFunctionContext(invocation.Argument);

    if (shouldEvalTarget && target is not null)
      target = await _expressionEvaluator.EvaluateAsync(execContext, target, cancellationToken);

    if (shouldEvalArgument && argument is not null)
      argument = await _expressionEvaluator.EvaluateAsync(execContext, argument, cancellationToken);

    if (target == invocation.Target && argument == invocation.Argument)
      return invocation;

    return invocation.With(target, argument);
  }

  private static bool ShouldEvaluateTarget(CommandDefinition? metadata, CommandInvocation invocation)
  {
    if (metadata is null)
      return !HasNoEvalSwitch(invocation);

    if ((metadata.TypeFlags & CommandTypeFlags.NoParse) != 0)
      return false;

    if (HasNoEvalSwitch(invocation))
    {
      var hasEqSplit = (metadata.TypeFlags & CommandTypeFlags.EqSplit) != 0;
      if (!hasEqSplit)
        return false;
    }

    return true;
  }

  private static bool ShouldEvaluateArgument(CommandDefinition? metadata, CommandInvocation invocation)
  {
    if ((metadata?.TypeFlags & CommandTypeFlags.RsNoParse) != 0)
      return false;

    if (HasNoEvalSwitch(invocation))
      return false;

    return true;
  }

  private static bool HasNoEvalSwitch(CommandInvocation invocation)
  {
    return invocation.Switches.Any(
      s => s.Name.Equals("NOEVAL", StringComparison.OrdinalIgnoreCase));
  }
}
