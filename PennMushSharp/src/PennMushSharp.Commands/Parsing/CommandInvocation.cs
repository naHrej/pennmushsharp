namespace PennMushSharp.Commands.Parsing;

public sealed class CommandInvocation
{
  public CommandInvocation(string name, IReadOnlyList<CommandSwitch> switches, string? target, string? argument, string raw)
  {
    Name = name;
    Switches = switches;
    Target = string.IsNullOrWhiteSpace(target) ? null : target;
    Argument = string.IsNullOrWhiteSpace(argument) ? null : argument;
    Raw = raw;
  }

  public string Name { get; }
  public IReadOnlyList<CommandSwitch> Switches { get; }
  public string? Target { get; }
  public string? Argument { get; }
  public string Raw { get; }

  public CommandInvocation With(string? target, string? argument)
  {
    return new CommandInvocation(Name, Switches, target, argument, Raw);
  }
}

public sealed class CommandSwitch
{
  public CommandSwitch(string name, string? argument)
  {
    Name = name;
    Argument = string.IsNullOrWhiteSpace(argument) ? null : argument;
  }

  public string Name { get; }
  public string? Argument { get; }
}
