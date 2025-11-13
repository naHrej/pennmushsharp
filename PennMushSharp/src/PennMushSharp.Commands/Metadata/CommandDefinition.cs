namespace PennMushSharp.Commands.Metadata;

public sealed class CommandDefinition
{
  public CommandDefinition(
    string name,
    string category,
    bool wizardOnly,
    IReadOnlyList<string> aliases,
    IReadOnlyList<CommandSwitchDefinition> switches,
    string handler,
    uint typeFlags,
    IReadOnlyList<string> flags,
    IReadOnlyList<string> powers)
  {
    Name = name;
    Category = category;
    WizardOnly = wizardOnly;
    Aliases = aliases;
    Switches = switches;
    Handler = handler;
    TypeFlags = typeFlags;
    Flags = flags;
    Powers = powers;
  }

  public string Name { get; }
  public string Category { get; }
  public bool WizardOnly { get; }
  public IReadOnlyList<string> Aliases { get; }
  public IReadOnlyList<CommandSwitchDefinition> Switches { get; }
  public string Handler { get; }
  public uint TypeFlags { get; }
  public IReadOnlyList<string> Flags { get; }
  public IReadOnlyList<string> Powers { get; }
}

public sealed class CommandSwitchDefinition
{
  public CommandSwitchDefinition(string name, bool requiresArgument)
  {
    Name = name;
    RequiresArgument = requiresArgument;
  }

  public string Name { get; }
  public bool RequiresArgument { get; }
}
