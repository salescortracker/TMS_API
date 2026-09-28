using System.Security.Cryptography;
using System.Text;

namespace TMS.BusinessLayer.Common;

/// <summary>SHA-256, matching HASHBYTES('SHA2_256', ...) used in the SQL seed script.</summary>
public static class PasswordHasher
{
    public static byte[] Hash(string password) => SHA256.HashData(Encoding.UTF8.GetBytes(password));

    /// <summary>Accepts UTF-8 hashes (made by the API) and UTF-16 hashes (HASHBYTES over an NVARCHAR in SQL).</summary>
    public static bool Verify(string password, byte[]? hash) =>
        hash is not null &&
        (Hash(password).AsSpan().SequenceEqual(hash) ||
         SHA256.HashData(Encoding.Unicode.GetBytes(password)).AsSpan().SequenceEqual(hash));

    public static string GenerateTemporary()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        var suffix = new string(Enumerable.Range(0, 8).Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)]).ToArray());
        return "Tms@" + suffix;
    }
}
