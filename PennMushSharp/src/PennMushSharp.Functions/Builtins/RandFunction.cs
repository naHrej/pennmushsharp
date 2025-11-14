using System;
using System.Collections.Generic;

namespace PennMushSharp.Functions.Builtins;

public sealed class RandFunction : IFunction
{
  public string Name => "RAND";

  public ValueTask<string> InvokeAsync(FunctionExecutionContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
  {
    switch (arguments.Count)
    {
      case 0:
        return ValueTask.FromResult(FunctionEvaluationUtils.EnsureNumeric(Random.Shared.NextDouble()));
      case 1:
        if (!FunctionEvaluationUtils.TryParseLong(arguments[0], out var upper) || upper == 0)
          return ValueTask.FromResult(FunctionEvaluationUtils.IntegerArgumentError);
        return ValueTask.FromResult(GenerateSingleBound(upper));
      default:
        if (!FunctionEvaluationUtils.TryParseLong(arguments[0], out var first)
          || !FunctionEvaluationUtils.TryParseLong(arguments[1], out var second))
        {
          return ValueTask.FromResult(FunctionEvaluationUtils.IntegerArgumentError);
        }

        return ValueTask.FromResult(GenerateTwoBounds(first, second));
    }
  }

  private static string GenerateSingleBound(long bound)
  {
    long low = 0;
    long high;
    long offset = 0;

    if (bound > 0)
    {
      high = bound - 1;
    }
    else
    {
      offset = Math.Abs(bound) - 1;
      high = Math.Abs(bound) - 1;
    }

    var random = RandomInclusive(low, high);
    return FunctionEvaluationUtils.EnsureNumeric(random - offset);
  }

  private static string GenerateTwoBounds(long a, long b)
  {
    long low = Math.Min(a, b);
    long high = Math.Max(a, b);
    long offset = 0;

    if (low < 0)
    {
      offset = Math.Abs(low);
      high = Math.Abs(high + offset);
      low = 0;
    }

    var random = RandomInclusive(low, high);
    return FunctionEvaluationUtils.EnsureNumeric(random - offset);
  }

  private static long RandomInclusive(long low, long high)
  {
    if (low > high)
      (low, high) = (high, low);
    return Random.Shared.NextInt64(low, high + 1);
  }
}
