namespace PennMushSharp.Commands.Metadata;

/// <summary>
/// Mirrors PennMUSH command type flags (command.h) so we can reason about parser semantics.
/// </summary>
public static class CommandTypeFlags
{
  public const uint EqSplit = 0x00000001;
  public const uint Args = 0x00000010;
  public const uint ArgSpace = 0x00000020;
  public const uint NoParse = 0x00000040;
  public const uint RsBrace = 0x00000080;
  public const uint RsArgs = 0x00000100;
  public const uint RsSpace = 0x00000200;
  public const uint RsNoParse = 0x00000400;
}
