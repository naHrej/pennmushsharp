using PennMushSharp.Core.Persistence;
using PennMushSharp.Runtime;
using Xunit;

namespace PennMushSharp.Tests.Runtime;

public sealed class PasswordVerifierTests
{
  private const string Hashed = "2:sha512:Azb94a1b669c25bc6548da57fbddc76ebb54af4fa56e3be685d9c78173ab8f50bc2fc21e52677e83825099b84f8c3677097c47c60a6ca1fcc1d5feb60cd2b52d4d:1731490000";

  [Fact]
  public void Verify_ReturnsTrueForValidPasswords()
  {
    var record = new GameObjectRecord { DbRef = 9 };
    record.Attributes["XYXXY"] = Hashed;

    var verifier = new PasswordVerifier();

    Assert.True(verifier.Verify(record, "harness"));
    Assert.False(verifier.Verify(record, "wrong"));
  }
}
