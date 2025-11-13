namespace PennMushSharp.Core;

/// <summary>
/// Minimal placeholder for the PennMUSH object structure. This will expand to represent
/// dbrefs, attributes, locks, flags, and persistence metadata as the port progresses.
/// </summary>
public sealed record GameObject(int DbRef, string Name);
