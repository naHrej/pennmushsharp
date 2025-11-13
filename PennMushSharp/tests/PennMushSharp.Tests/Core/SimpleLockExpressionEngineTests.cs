using PennMushSharp.Core.Locks.Runtime;
using Xunit;

namespace PennMushSharp.Tests.Core;

public sealed class SimpleLockExpressionEngineTests
{
  private readonly ILockExpressionEngine _engine = SimpleLockExpressionEngine.Instance;

  [Fact]
  public void Evaluate_TrueBoolexp()
  {
    Assert.True(_engine.Evaluate(1, 2, "TRUE_BOOLEXP"));
  }

  [Fact]
  public void Evaluate_FalseBoolexp()
  {
    Assert.False(_engine.Evaluate(1, 2, "FALSE"));
  }

  [Fact]
  public void Evaluate_PlayerDbref()
  {
    Assert.True(_engine.Evaluate(5, 2, "#5"));
    Assert.False(_engine.Evaluate(5, 2, "#6"));
  }

  [Fact]
  public void Evaluate_ComplexExpression()
  {
    Assert.True(_engine.Evaluate(5, 2, "(#5 & TRUE_BOOLEXP) | #7"));
    Assert.False(_engine.Evaluate(5, 2, "!(#5)"));
  }
}
