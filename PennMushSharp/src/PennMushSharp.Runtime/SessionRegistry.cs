using System.Collections.Concurrent;
using PennMushSharp.Commands;
using PennMushSharp.Core;

namespace PennMushSharp.Runtime;

public sealed class SessionRegistry : ISessionRegistry
{
  private readonly ConcurrentDictionary<Guid, SessionInfo> _sessions = new();

  public Guid Register(GameObject actor, string host)
  {
    var id = Guid.NewGuid();
    _sessions[id] = new SessionInfo
    {
      Actor = actor,
      ConnectedAtUtc = DateTime.UtcNow,
      LastCommandUtc = DateTime.UtcNow,
      Host = host,
      DescriptorCount = 1,
      CommandCount = 0
    };
    return id;
  }

  public void Unregister(Guid id)
  {
    _sessions.TryRemove(id, out _);
  }

  public void RecordActivity(Guid id)
  {
    if (_sessions.TryGetValue(id, out var session))
    {
      session.CommandCount++;
      session.LastCommandUtc = DateTime.UtcNow;
    }
  }

  public IReadOnlyCollection<SessionInfo> GetActiveSessions()
  {
    return _sessions.Values.ToList();
  }
}

