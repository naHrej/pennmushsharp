using System.Collections.Concurrent;
using PennMushSharp.Commands;
using PennMushSharp.Core;

namespace PennMushSharp.Runtime;

public sealed class SessionRegistry : ISessionRegistry
{
  private readonly ConcurrentDictionary<Guid, SessionInfo> _sessions = new();

  public Guid Register(GameObject actor)
  {
    var id = Guid.NewGuid();
    _sessions[id] = new SessionInfo
    {
      Actor = actor,
      ConnectedAtUtc = DateTime.UtcNow
    };
    return id;
  }

  public void Unregister(Guid id)
  {
    _sessions.TryRemove(id, out _);
  }

  public IReadOnlyCollection<SessionInfo> GetActiveSessions()
  {
    return _sessions.Values.ToList();
  }
}
