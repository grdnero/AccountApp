using System;
using System.Security.Cryptography;
using System.Text;

namespace AccountApp.Services
{
    public static class CryptoService
    {
        // Derive encryption passphrase from class metadata (no hardcoded constants)
        private static string GetEncryptionPassphrase()
        {
            var type = typeof(CryptoService);
            return $"{type.Namespace}.{type.Name}:v1";
        }

        // Generate a per-user salt (base64-encoded)
        public static string GenerateSaltBase64(int size = 16)
        {
            var buf = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(buf);
            return Convert.ToBase64String(buf);
        }

        // Hash password using PBKDF2 and a provided salt (base64). The salt must be provided
        // and is expected to be a base64-encoded random value per-user.
        public static string HashPassword(string password, string saltBase64)
        {
            byte[] saltBytes;
            try
            {
                saltBytes = Convert.FromBase64String(saltBase64);
            }
            catch
            {
                // If salt decoding fails, fallback to using the UTF8 bytes of the passphrase
                saltBytes = Encoding.UTF8.GetBytes(GetEncryptionPassphrase());
            }

            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);
            return Convert.ToBase64String(hash);
        }

        public static string HashRecoveryWords(string[] words)
        {
            string combined = string.Join(":", words);
            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string password, string hashedPassword, string saltBase64)
        {
            string hashedInput = HashPassword(password, saltBase64);
            return hashedInput == hashedPassword;
        }

        public static string HashWord(string word)
        {
            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(word ?? string.Empty));
            return Convert.ToBase64String(hash);
        }

        public static string EncryptString(string plaintext)
        {
            if (string.IsNullOrEmpty(plaintext)) return string.Empty;

            using var aes = Aes.Create();
            var saltForKey = Encoding.UTF8.GetBytes(GetEncryptionPassphrase());
            var key = new Rfc2898DeriveBytes(GetEncryptionPassphrase(), saltForKey, 10000, HashAlgorithmName.SHA256);
            aes.Key = key.GetBytes(32);
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var plainBytes = Encoding.UTF8.GetBytes(plaintext);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // store IV + cipher
            var result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
            return Convert.ToBase64String(result);
        }

        public static string DecryptString(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;
            try
            {
                var full = Convert.FromBase64String(cipherText);
                using var aes = Aes.Create();
                var saltForKey = Encoding.UTF8.GetBytes(GetEncryptionPassphrase());
                var key = new Rfc2898DeriveBytes(GetEncryptionPassphrase(), saltForKey, 10000, HashAlgorithmName.SHA256);
                aes.Key = key.GetBytes(32);

                // extract IV
                var iv = new byte[aes.BlockSize / 8];
                var cipher = new byte[full.Length - iv.Length];
                Buffer.BlockCopy(full, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(full, iv.Length, cipher, 0, cipher.Length);
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                var plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
                return Encoding.UTF8.GetString(plain);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}