using PennMushSharp.Core.Persistence;

namespace PennMushSharp.Runtime;

public static class GameStateSeeder
{
  private const string DefaultHashedPassword = "2:sha512:Azb94a1b669c25bc6548da57fbddc76ebb54af4fa56e3be685d9c78173ab8f50bc2fc21e52677e83825099b84f8c3677097c47c60a6ca1fcc1d5feb60cd2b52d4d:1731490000";

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
    record.Attributes["XYXXY"] = DefaultHashedPassword;
    state.Upsert(record);
  }
}
