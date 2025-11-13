using PennMushSharp.Core;
using PennMushSharp.Core.Persistence;

namespace PennMushSharp.Runtime;

public sealed class AccountService
{
  private readonly InMemoryGameState _gameState;
  private readonly PasswordVerifier _passwordVerifier;
  private readonly IAccountRepository _repository;

  public AccountService(
    InMemoryGameState gameState,
    PasswordVerifier passwordVerifier,
    IAccountRepository repository)
  {
    _gameState = gameState;
    _passwordVerifier = passwordVerifier;
    _repository = repository;
  }

  public bool TryConnect(string name, string password, out GameObject actor)
  {
    actor = default!;
    if (!_gameState.TryGet(name, out var record) || record is null)
      return false;

    if (!_passwordVerifier.Verify(record, password))
      return false;

    actor = GameObject.FromRecord(record);
    return true;
  }

  public GameObject Create(string name, string password)
  {
    var hash = _passwordVerifier.HashPassword(password);
    var record = _gameState.Allocate(name, hash);
    _repository.Save(record);
    return GameObject.FromRecord(record);
  }
}
