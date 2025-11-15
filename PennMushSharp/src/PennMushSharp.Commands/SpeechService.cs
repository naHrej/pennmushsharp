using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PennMushSharp.Core;
using PennMushSharp.Core.Locks.Runtime;

namespace PennMushSharp.Commands;

public sealed class SpeechService
{
  private const string SpeechLockName = "Speech";
  private const string PageLockName = "Page";
  private readonly ILockService _lockService;
  private readonly ISessionRegistry _sessionRegistry;

  public SpeechService(ISessionRegistry sessionRegistry, ILockService lockService)
  {
    _sessionRegistry = sessionRegistry ?? throw new ArgumentNullException(nameof(sessionRegistry));
    _lockService = lockService ?? throw new ArgumentNullException(nameof(lockService));
  }

  public async ValueTask<bool> EnsureCanSpeakAsync(GameObject speaker, IOutputWriter output, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(speaker);
    ArgumentNullException.ThrowIfNull(output);

    if (speaker.Location is null)
    {
      await output.WriteLineAsync("You have no location to speak from.", cancellationToken);
      return false;
    }

    if (!_lockService.Evaluate(new LockRequest(speaker.DbRef, speaker.Location.Value, SpeechLockName)))
    {
      await output.WriteLineAsync("You may not speak here.", cancellationToken);
      return false;
    }

    return true;
  }

  public async ValueTask<bool> EnsureCanContactAsync(GameObject speaker, GameObject target, IOutputWriter output, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(speaker);
    ArgumentNullException.ThrowIfNull(target);
    ArgumentNullException.ThrowIfNull(output);

    if (speaker.DbRef == target.DbRef)
      return true;

    if (IsHaven(target))
    {
      await output.WriteLineAsync($"I'm sorry, but {target.Name} wishes to be left alone now.", cancellationToken);
      return false;
    }

    if (!_lockService.Evaluate(new LockRequest(speaker.DbRef, target.DbRef, PageLockName)))
    {
      await output.WriteLineAsync($"I'm sorry, but {target.Name} wishes to be left alone now.", cancellationToken);
      return false;
    }

    return true;
  }

  public IReadOnlyList<SessionInfo> ResolveRoomRecipients(string? target, int? locationDbRef)
  {
    if (locationDbRef is null)
      return Array.Empty<SessionInfo>();
    return ResolveTargets(target, session => session.Actor.Location == locationDbRef);
  }

  public IReadOnlyList<SessionInfo> ResolveGlobalRecipients(string? target) =>
    ResolveTargets(target, static _ => true);

  private static bool IsHaven(GameObject obj) =>
    obj.Flags.Any(f => f.Equals("HAVEN", StringComparison.OrdinalIgnoreCase));

  private IReadOnlyList<SessionInfo> ResolveTargets(string? target, Func<SessionInfo, bool> predicate)
  {
    if (string.IsNullOrWhiteSpace(target))
      return Array.Empty<SessionInfo>();

    var tokens = target.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (tokens.Length == 0)
      return Array.Empty<SessionInfo>();

    var sessions = _sessionRegistry.GetActiveSessions();
    var results = new List<SessionInfo>();
    foreach (var token in tokens)
    {
      foreach (var session in sessions)
      {
        if (!predicate(session))
          continue;
        if (!MatchesToken(session.Actor, token))
          continue;
        if (!results.Contains(session))
          results.Add(session);
      }
    }

    return results;
  }

  private static bool MatchesToken(GameObject actor, string token)
  {
    if (actor.Name.Equals(token, StringComparison.OrdinalIgnoreCase))
      return true;

    if (token.StartsWith('#'))
      token = token[1..];

    return int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var dbRef) && dbRef == actor.DbRef;
  }
}
