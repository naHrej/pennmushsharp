using PennMushSharp.Core.Locks;
using PennMushSharp.Core.Locks.Runtime;
using Xunit;

namespace PennMushSharp.Tests.Core;

public sealed class LockEvaluatorTests
{
  [Fact]
  public void LockEvaluator_UsesMetadataDefaultWhenNoStoredLock()
  {
    var metadata = new TestMetadataService();
    var store = new EmptyStore();
    var engine = new RecordingEngine();

    var evaluator = new LockEvaluator(metadata, store, engine);
    var result = evaluator.Evaluate(new LockRequest(PlayerDbRef: 1, ThingDbRef: 2, LockName: "Control"));

    Assert.True(result);
    Assert.Equal("TRUE_BOOLEXP", engine.LastExpression);
  }

  [Fact]
  public void LockEvaluator_UsesStoredExpressionWhenAvailable()
  {
    var metadata = new TestMetadataService();
    var store = new PreloadedStore((2, "Control", new StoredLock("Control", "FALSE")));
    var engine = new RecordingEngine();

    var evaluator = new LockEvaluator(metadata, store, engine);
    var result = evaluator.Evaluate(new LockRequest(1, 2, "Control"));

    Assert.False(result);
    Assert.Equal("FALSE", engine.LastExpression);
  }

  private sealed class TestMetadataService : ILockMetadataService
  {
    private readonly LockDefinition _definition = new()
    {
      Name = "Control",
      DefaultKeyExpression = "TRUE_BOOLEXP",
      DefaultCreator = 1,
      DefaultFlags = 0,
    };

    public IEnumerable<LockDefinition> GetAllDefinitions() => new[] { _definition };

    public bool TryGetDefinition(string name, out LockDefinition? definition)
    {
      if (string.Equals(name, _definition.Name, StringComparison.OrdinalIgnoreCase))
      {
        definition = _definition;
        return true;
      }

      definition = null;
      return false;
    }
  }

  private sealed class EmptyStore : ILockStore
  {
    public bool TryGet(int thingDbRef, string lockName, out StoredLock storedLock)
    {
      storedLock = default;
      return false;
    }
  }

  private sealed class PreloadedStore : ILockStore
  {
    private readonly (int Thing, string Lock, StoredLock Value) _entry;

    public PreloadedStore((int, string, StoredLock) entry) => _entry = entry;

    public bool TryGet(int thingDbRef, string lockName, out StoredLock storedLock)
    {
      if (thingDbRef == _entry.Thing && string.Equals(lockName, _entry.Lock, StringComparison.OrdinalIgnoreCase))
      {
        storedLock = _entry.Value;
        return true;
      }

      storedLock = default;
      return false;
    }
  }

  private sealed class RecordingEngine : ILockExpressionEngine
  {
    public string? LastExpression { get; private set; }

    public bool Evaluate(int playerDbRef, int thingDbRef, string expression)
    {
      LastExpression = expression;
      return !string.Equals(expression, "FALSE", StringComparison.OrdinalIgnoreCase);
    }
  }
}
