using System;
using System.Security.Cryptography;

namespace WebAPI.Helpers;

public static class PasswordHasher
{
    // Thông số mặc định
    private const int SaltSize = 16; // 16 bytes
    private const int HashSize = 32; // 32 bytes = 256 bits
    private const int Iterations = 100_000; // số vòng PBKDF2

    // Tạo hash: trả về chuỗi lưu vào DB (format: iterations.saltBase64.hashBase64)
    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) throw new ArgumentException("Password required", nameof(password));

        // 1. tạo salt ngẫu nhiên
        byte[] salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);

        // 2. derive key (PBKDF2)
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(HashSize);

        // 3. encode và trả về theo format dễ lưu/parse
        string saltB64 = Convert.ToBase64String(salt);
        string hashB64 = Convert.ToBase64String(hash);
        return $"{Iterations}.{saltB64}.{hashB64}";
    }

    // Verify password: so sánh password nhập vào với giá trị lưu trong DB
    public static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password)) throw new ArgumentException("Password required", nameof(password));
        if (string.IsNullOrEmpty(storedHash)) return false;

        // storedHash format: iterations.salt.hash
        var parts = storedHash.Split('.', 3);
        if (parts.Length != 3) return false;

        if (!int.TryParse(parts[0], out int iterations)) return false;
        byte[] salt = Convert.FromBase64String(parts[1]);
        byte[] storedHashBytes = Convert.FromBase64String(parts[2]);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        byte[] computedHash = pbkdf2.GetBytes(storedHashBytes.Length);

        // So sánh an toàn (constant-time)
        return CryptographicOperations.FixedTimeEquals(computedHash, storedHashBytes);
    }
}
