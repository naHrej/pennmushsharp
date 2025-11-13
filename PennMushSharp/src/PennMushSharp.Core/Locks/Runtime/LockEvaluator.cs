using PennMushSharp.Core.Locks;

namespace PennMushSharp.Core.Locks.Runtime;

public sealed class LockEvaluator : ILockService
{
  private readonly ILockMetadataService _metadataService;
  private readonly ILockStore _store;
  private readonly ILockExpressionEngine _engine;

  public LockEvaluator(ILockMetadataService metadataService, ILockStore store, ILockExpressionEngine engine)
  {
    _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
    _store = store ?? throw new ArgumentNullException(nameof(store));
    _engine = engine ?? throw new ArgumentNullException(nameof(engine));
  }

  public bool Evaluate(LockRequest request)
  {
    if (!_metadataService.TryGetDefinition(request.LockName, out var definition) || definition is null)
      return false; // Unknown locks fail closed for now.

    var expression = definition.DefaultKeyExpression;
    if (_store.TryGet(request.ThingDbRef, definition.Name, out var stored))
      expression = stored.Expression;

    return _engine.Evaluate(request.PlayerDbRef, request.ThingDbRef, expression);
  }
}
