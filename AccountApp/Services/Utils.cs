using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using AccountApp.Models;

namespace AccountApp.Services
{
    public static class Utils
    {
        public static List<string> GenerateRecoveryWords(int count = 8)
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var path = Path.Combine(baseDir, "Assets", "wordlist.txt");
                if (File.Exists(path))
                {
                    var allWords = File.ReadAllLines(path)
                        .Select(w => w.Trim())
                        .Where(w => w.Length >= 4 && w.Length <= 5)
                        .Where(w => !string.IsNullOrWhiteSpace(w))
                        .Select(w => w.ToLowerInvariant())
                        .Distinct()
                        .ToList();

                    if (allWords.Count >= count)
                    {
                        var result = new List<string>(count);
                        using var rng = RandomNumberGenerator.Create();
                        while (result.Count < count)
                        {
                            // pick random index
                            var idxBytes = new byte[4];
                            rng.GetBytes(idxBytes);
                            int idx = Math.Abs(BitConverter.ToInt32(idxBytes, 0)) % allWords.Count;
                            var candidate = allWords[idx];
                            if (!result.Contains(candidate)) result.Add(candidate);
                        }
                        return result;
                    }
                }
            }
            catch
            {
                // ignore and fallback
            }

            // fallback: generate simple unique 4-5 letter words using a secure RNG
            var fallback = new List<string>(count);
            using var rng2 = RandomNumberGenerator.Create();
            const int minLen = 4, maxLen = 5;
            var letters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            while (fallback.Count < count)
            {
                // pick a length between minLen and maxLen
                var lenByte = new byte[1];
                rng2.GetBytes(lenByte);
                int len = minLen + (lenByte[0] % (maxLen - minLen + 1));

                var chars = new char[len];
                var buf = new byte[len];
                rng2.GetBytes(buf);
                for (int i = 0; i < len; i++) chars[i] = letters[buf[i] % letters.Length];

                var word = new string(chars);
                if (!fallback.Contains(word)) fallback.Add(word);
            }
            return fallback;
        }

    // Application-level current location and clock (can be changed for testing)
    public static string CurrentLocation { get; set; } = "Default";

    private static DateTime? _overrideNow = null;
    public static DateTime GetNow() => _overrideNow ?? DateTime.Now;
    public static void SetNow(DateTime dt) => _overrideNow = dt;
    public static void ResetNow() => _overrideNow = null;

        public static string GenerateRandomPassword(int length = 12)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var rnd = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[rnd.Next(s.Length)]).ToArray());
        }

        public static string CalculateAuthKey(Account account)
        {
            // Use the stored hashed recovery words as the auth key input so the key
            // can be reproduced after application restart (recovery words are not stored).
            string input = account.HashedRecoveryWords ?? string.Empty;

            // Step 2-4: process ASCII
            var nums = new List<int>();
            foreach (char c in input)
            {
                int val = (int)c;
                if (val >= 65 && val <= 90) val += 20;
                else if (val >= 97 && val <= 122) val -= 64;
                val = (~val) & 0x7F; // flip 7 bits
                nums.Add(val + 16);
            }

            // Step 5: sum + login counter
            int total = nums.Sum() + account.LoginCounter;

            // Step 6: convert to 4-digit pin
            string result = total.ToString();
            return result.Length > 4 ? result[..4] : result.PadLeft(4, '0');
        }
    }
}
