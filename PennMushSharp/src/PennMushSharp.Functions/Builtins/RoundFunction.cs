using System;
using System.Collections.Generic;
using System.Globalization;

namespace PennMushSharp.Functions.Builtins;

public sealed class RoundFunction : IFunction
{
  public string Name => "ROUND";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    if (arguments.Count == 0 || !FunctionEvaluationUtils.TryParseDouble(arguments[0], out var value))
      return ValueTask.FromResult(FunctionEvaluationUtils.NumericArgumentError);

    uint places = 0;
    if (arguments.Count >= 2)
    {
      if (!uint.TryParse(arguments[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out places))
        return ValueTask.FromResult(FunctionEvaluationUtils.IntegerArgumentError);
      if (places > 15)
        places = 15;
    }

    var pad = false;
    if (arguments.Count >= 3)
    {
      if (!FunctionEvaluationUtils.TryParseBoolean(arguments[2], out pad))
        return ValueTask.FromResult(FunctionEvaluationUtils.BooleanArgumentError);
    }

    var rounded = Math.Round(value, (int)places, MidpointRounding.AwayFromZero);

    if (places == 0)
      return ValueTask.FromResult(FunctionEvaluationUtils.EnsureNumeric(rounded));

    var formatted = rounded.ToString($"F{places}", CultureInfo.InvariantCulture);
    if (!pad)
      formatted = formatted.TrimEnd('0').TrimEnd('.');
    if (string.IsNullOrEmpty(formatted))
      formatted = "0";
    return ValueTask.FromResult(formatted);
  }
}
