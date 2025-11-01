using System;
using System.Security.Cryptography;
using System.Text;

namespace WebAPI.Helpers;

public class StringHelper
{
    public static string GenerateRandomString(int length, bool onlyAlphaNumeric = false)
    {
        if (length <= 0)
            throw new ArgumentException("Length must be greater than 0.");

        if (!onlyAlphaNumeric)
        {
            // Trả về Base64 (mặc định)
            var randomBytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes)
                         .Substring(0, length)
                         .Replace("+", "")
                         .Replace("/", "")
                         .Replace("=", "");
        }
        else
        {
            // Chỉ chứa chữ và số
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var result = new StringBuilder();
            using var rng = RandomNumberGenerator.Create();
            var buffer = new byte[sizeof(uint)];

            while (result.Length < length)
            {
                rng.GetBytes(buffer);
                uint num = BitConverter.ToUInt32(buffer, 0);
                result.Append(chars[(int)(num % (uint)chars.Length)]);
            }

            return result.ToString();
        }
    }
}
