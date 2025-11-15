using System;
using System.Collections.Generic;
using System.Globalization;

namespace PennMushSharp.Functions.Builtins;

internal static class FunctionEvaluationUtils
{
  private const string NumericError = "#-1 ARGUMENT MUST BE NUMERIC";
  private const string IntegerError = "#-1 ARGUMENT MUST BE INTEGER";
  private const string BooleanError = "#-1 ARGUMENT MUST BE BOOLEAN";
  private const string DivisionByZeroError = "#-1 DIVISION BY ZERO";
  private const string ValueOutOfRangeError = "#-1 VALUE OUT OF RANGE";
  private const string RangeError = "#-1 ARGUMENT OUT OF RANGE";
  private const string ImaginaryNumberError = "#-1 IMAGINARY NUMBER";

  public static bool TryParseDouble(string input, out double value) =>
    double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

  public static bool TryParseLong(string input, out long value) =>
    long.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

  public static bool TryParseInt(string input, out int value) =>
    int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

  public static bool TryParseBoolean(string? input, out bool value)
  {
    value = false;
    if (string.IsNullOrWhiteSpace(input))
      return true;

    var token = input.Trim();
    if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numeric))
    {
      value = numeric != 0;
      return true;
    }

    switch (char.ToUpperInvariant(token[0]))
    {
      case 'T':
      case 'Y':
        value = true;
        return true;
      case 'F':
      case 'N':
        value = false;
        return true;
      case 'O':
        if (token.Length > 1 && char.ToUpperInvariant(token[1]) == 'N')
        {
          value = true;
          return true;
        }
        if (token.Length > 1 && char.ToUpperInvariant(token[1]) == 'F')
        {
          value = false;
          return true;
        }
        break;
    }

    return false;
  }

  public static string EnsureNumeric(double value)
  {
    if (double.IsNaN(value) || double.IsInfinity(value))
      return ValueOutOfRangeError;

    var nearestInteger = Math.Round(value);
    if (Math.Abs(value - nearestInteger) < 1e-9)
      return nearestInteger.ToString(CultureInfo.InvariantCulture);

    var rounded = Math.Round(value, 6, MidpointRounding.AwayFromZero);
    return rounded.ToString("0.######", CultureInfo.InvariantCulture);
  }

  public static string EnsureNumeric(long value) => value.ToString(CultureInfo.InvariantCulture);

  public static string NumericArgumentError => NumericError;
  public static string IntegerArgumentError => IntegerError;
  public static string BooleanArgumentError => BooleanError;
  public static string DivisionByZero => DivisionByZeroError;
  public static string RangeErrorMessage => RangeError;
  public static string ImaginaryNumber => ImaginaryNumberError;

  public static double AngleToRadians(double angle, string? unit)
  {
    if (string.IsNullOrEmpty(unit))
      return angle;

    return char.ToUpperInvariant(unit[0]) switch
    {
      'D' => angle * (Math.PI / 180.0),
      'G' => angle * (Math.PI / 200.0),
      _ => angle
    };
  }

  public static double RadiansToAngle(double angle, string? unit)
  {
    if (string.IsNullOrEmpty(unit))
      return angle;

    return char.ToUpperInvariant(unit[0]) switch
    {
      'D' => angle * (180.0 / Math.PI),
      'G' => angle * (200.0 / Math.PI),
      _ => angle
    };
  }

  public static string Trim(string value, string? trimChars, TrimMode mode)
  {
    if (string.IsNullOrEmpty(trimChars))
    {
      return mode switch
      {
        TrimMode.Left => value.TrimStart(),
        TrimMode.Right => value.TrimEnd(),
        _ => value.Trim()
      };
    }

    var set = BuildCharSet(trimChars);
    return mode switch
    {
      TrimMode.Left => TrimStart(value, set),
      TrimMode.Right => TrimEnd(value, set),
      _ => TrimEnd(TrimStart(value, set), set)
    };
  }

  private static HashSet<char> BuildCharSet(string input)
  {
    var set = new HashSet<char>();
    foreach (var ch in input)
      set.Add(ch);
    return set;
  }

  private static string TrimStart(string value, HashSet<char> set)
  {
    var index = 0;
    while (index < value.Length && set.Contains(value[index]))
      index++;
    return index >= value.Length ? string.Empty : value[index..];
  }

  private static string TrimEnd(string value, HashSet<char> set)
  {
    var index = value.Length - 1;
    while (index >= 0 && set.Contains(value[index]))
      index--;
    return index < 0 ? string.Empty : value[..(index + 1)];
  }
}

internal enum TrimMode
{
  Both,
  Left,
  Right
}
