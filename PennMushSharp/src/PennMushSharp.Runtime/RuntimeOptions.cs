namespace PennMushSharp.Runtime;

public sealed class RuntimeOptions
{
  /// <summary>
  /// Optional path to a PennMUSH text dump that should be loaded at startup.
  /// </summary>
  public string? InitialDumpPath { get; set; }

  public string ListenAddress { get; set; } = "127.0.0.1";
  public int ListenPort { get; set; } = 4201;

  public int DefaultAccountDbRef { get; set; } = 1;
  public string DefaultAccountName { get; set; } = "One";

  public string AccountStorePath { get; set; } = Path.Combine("data", "accounts.dump");
}
