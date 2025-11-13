using PennMushSharp.Core.Persistence;

namespace PennMushSharp.Runtime;

public static class GameStateSeeder
{
  private const string? DefaultHashedPassword = null;

  public static void EnsureDefaultWizard(InMemoryGameState state, int dbRef, string name)
  {
    if (state.TryGet(dbRef, out _))
      return;

    var record = new GameObjectRecord
    {
      DbRef = dbRef,
      Name = name,
      Owner = dbRef
    };
    if (!string.IsNullOrEmpty(DefaultHashedPassword))
      record.SetAttribute("XYXXY", DefaultHashedPassword, dbRef);
    state.Upsert(record);
  }
}
