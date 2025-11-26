using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using AccountApp.Models;

namespace AccountApp.Services
{
    public static class JsonStorage
    {
        private static readonly string FilePath = "accounts.json";

        public static List<Account> LoadAccounts()
        {
            if (!File.Exists(FilePath)) return new List<Account>();
            string json = File.ReadAllText(FilePath);
            var list = JsonSerializer.Deserialize<List<Account>>(json) ?? new List<Account>();

            // Decrypt LastLocation after loading
            foreach (var acc in list)
            {
                if (!string.IsNullOrEmpty(acc.LastLocation))
                {
                    var dec = CryptoService.DecryptString(acc.LastLocation);
                    acc.LastLocation = string.IsNullOrEmpty(dec) ? acc.LastLocation : dec;
                }
            }

            return list;
        }

        public static void SaveAccounts(List<Account> accounts)
        {
            // Create a sanitized copy where LastLocation is encrypted (LoginHistory is stored plain)
            var copy = new System.Collections.Generic.List<Account>();
            foreach (var acc in accounts)
            {
                var c = new Account
                {
                    Username = acc.Username,
                    Password = acc.Password,
                    Salt = acc.Salt,
                    RecoveryWords = acc.RecoveryWords,
                    RecoveryWordHashes = acc.RecoveryWordHashes,
                    LockedUntil = acc.LockedUntil,
                    HashedRecoveryWords = acc.HashedRecoveryWords,
                    LoginCounter = acc.LoginCounter,
                    LastLocation = string.IsNullOrEmpty(acc.LastLocation) ? string.Empty : CryptoService.EncryptString(acc.LastLocation),
                    TwoFactorEnabled = acc.TwoFactorEnabled,
                    LoginHistory = acc.LoginHistory ?? new System.Collections.Generic.List<string>()
                };

                copy.Add(c);
            }

            string json = JsonSerializer.Serialize(copy, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
    }
}
