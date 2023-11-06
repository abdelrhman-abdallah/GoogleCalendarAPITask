namespace CalenderAPITask.Helper
{
using System;
using System.Security.Cryptography;

public class PasswordHasher
{
    private const int SaltSize = 16; // Adjust the salt size based on your security requirements
    private const int Iterations = 10000; // Adjust the iteration count based on your security requirements

    public static (string hash, string salt) HashPassword(string password, byte[]? salt = null)
    {
            if (salt == null)
            {
                salt = GenerateSalt();
            }

            byte[] hash = GenerateHash(password, salt);

            return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    private static byte[] GenerateSalt()
    {
        byte[] salt = new byte[SaltSize];
        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        
        return salt;
    }

    private static byte[] GenerateHash(string password, byte[] salt)
    {
         var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
         return pbkdf2.GetBytes(32);
        
    }
}
}
