using System.Security.Cryptography;
using System.Text;
using PennMushSharp.Core.Persistence;

namespace PennMushSharp.Runtime;

public sealed class PasswordVerifier
{
  public bool Verify(GameObjectRecord record, string password)
  {
    if (record.Attributes.TryGetValue("XYXXY", out var stored) && !string.IsNullOrWhiteSpace(stored))
    {
      return VerifyHashedPassword(stored, password);
    }

    return false;
  }

  private static bool VerifyHashedPassword(string stored, string password)
  {
    var segments = stored.Split(':', StringSplitOptions.None);
    if (segments.Length < 4)
      return false;

    var version = segments[0];
    var algorithm = segments[1];
    var payload = segments[2];

    if (!string.Equals(version, "2", StringComparison.Ordinal) ||
        !string.Equals(algorithm, "sha512", StringComparison.OrdinalIgnoreCase) ||
        payload.Length < 2)
      return false;

    var salt = payload.Substring(0, 2);
    var expectedHash = payload.Substring(2);
    var candidate = ComputeSha512(salt + password);
    return string.Equals(candidate, expectedHash, StringComparison.OrdinalIgnoreCase);
  }

  private static string ComputeSha512(string input)
  {
    using var sha = SHA512.Create();
    var bytes = Encoding.UTF8.GetBytes(input);
    var hash = sha.ComputeHash(bytes);
    return Convert.ToHexString(hash).ToLowerInvariant();
  }
}
