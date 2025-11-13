using System.Security.Cryptography;
using System.Text;
using PennMushSharp.Core.Persistence;

namespace PennMushSharp.Runtime;

public sealed class PasswordVerifier
{
  public bool Verify(GameObjectRecord record, string password)
  {
    if (record.Attributes.TryGetValue("XYXXY", out var stored) && !string.IsNullOrWhiteSpace(stored.Value))
    {
      return VerifyHashedPassword(stored.Value, password);
    }

    return string.IsNullOrEmpty(password);
  }

  public string HashPassword(string password)
  {
    const string algorithm = "sha512";
    var salt = GenerateSalt();
    var hash = ComputeSha512(salt + password);
    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    return $"2:{algorithm}:{salt}{hash}:{timestamp}";
  }

  private static string GenerateSalt()
  {
    const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    Span<char> salt = stackalloc char[2];
    var random = RandomNumberGenerator.Create();
    Span<byte> buffer = stackalloc byte[2];
    random.GetBytes(buffer);
    salt[0] = chars[buffer[0] % chars.Length];
    salt[1] = chars[buffer[1] % chars.Length];
    return new string(salt);
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
