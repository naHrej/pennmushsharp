namespace PennMushSharp.Core;

public enum GameObjectType
{
  Unknown = 0,
  Room = 1,
  Thing = 2,
  Exit = 4,
  Player = 8,
  Garbage = 16,
  Program = 32
}

internal static class GameObjectTypeExtensions
{
  public static GameObjectType FromPennMushCode(int code) => code switch
  {
    1 => GameObjectType.Room,
    2 => GameObjectType.Thing,
    4 => GameObjectType.Exit,
    8 => GameObjectType.Player,
    16 => GameObjectType.Garbage,
    32 => GameObjectType.Program,
    _ => GameObjectType.Unknown
  };

  public static int ToPennMushCode(this GameObjectType type) =>
    type is GameObjectType.Room or GameObjectType.Thing or GameObjectType.Exit or GameObjectType.Player or GameObjectType.Garbage or GameObjectType.Program
      ? (int)type
      : 0;
}
