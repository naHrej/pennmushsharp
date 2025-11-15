using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PennMushSharp.Tests.Characterization;

public sealed class CharacterizationScenario
{
  private CharacterizationScenario(string name, string user, string password, IReadOnlyList<string> commands, string? disconnectCommand)
  {
    Name = name;
    User = user;
    Password = password;
    Commands = commands;
    DisconnectCommand = disconnectCommand;
  }

  public string Name { get; }
  public string User { get; }
  public string Password { get; }
  public IReadOnlyList<string> Commands { get; }
  public string? DisconnectCommand { get; }

  public static IEnumerable<CharacterizationScenario> LoadAll(string root)
  {
    if (!Directory.Exists(root))
      yield break;

    foreach (var path in Directory.EnumerateFiles(root, "*.scenario").OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
      yield return Load(path);
  }

  public static CharacterizationScenario Load(string path)
  {
    var name = default(string?);
    var user = default(string?);
    var password = string.Empty;
    var disconnect = default(string?);
    var commands = new List<string>();
    var inCommands = false;

    foreach (var raw in File.ReadLines(path, Encoding.UTF8))
    {
      var line = raw.Trim();
      if (string.IsNullOrEmpty(line) || line.StartsWith("#", StringComparison.Ordinal))
        continue;

      if (line.StartsWith("COMMANDS=(", StringComparison.Ordinal))
      {
        inCommands = true;
        continue;
      }

      if (inCommands)
      {
        if (line == ")")
        {
          inCommands = false;
          continue;
        }

        commands.Add(Unquote(line));
        continue;
      }

      if (TryParseAssignment(line, "NAME", out var value))
      {
        name = value;
        continue;
      }

      if (TryParseAssignment(line, "USER", out value))
      {
        user = value;
        continue;
      }

      if (TryParseAssignment(line, "PASSWORD", out value))
      {
        password = value ?? string.Empty;
        continue;
      }

      if (TryParseAssignment(line, "DISCONNECT_COMMAND", out value))
      {
        disconnect = value;
        continue;
      }
    }

    if (string.IsNullOrWhiteSpace(name))
      throw new InvalidOperationException($"Scenario '{path}' does not specify NAME.");
    if (commands.Count == 0)
      throw new InvalidOperationException($"Scenario '{path}' does not specify COMMANDS.");

    return new CharacterizationScenario(
      name,
      string.IsNullOrWhiteSpace(user) ? "One" : user!,
      password ?? string.Empty,
      commands,
      disconnect);
  }

  private static bool TryParseAssignment(string line, string key, out string? value)
  {
    value = null;
    var prefix = key + "=";
    if (!line.StartsWith(prefix, StringComparison.Ordinal))
      return false;

    value = Unquote(line[prefix.Length..].Trim());
    return true;
  }

  private static string Unquote(string input)
  {
    if (string.IsNullOrEmpty(input))
      return input;

    if (input[^1] == ')')
      input = input[..^1].TrimEnd();

    if (input.Length >= 2)
    {
      if ((input[0] == '"' && input[^1] == '"') || (input[0] == '\'' && input[^1] == '\''))
        return input[1..^1];
    }

    return input;
  }
}
