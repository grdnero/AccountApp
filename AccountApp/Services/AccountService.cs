using System;
using System.Collections.Generic;
using System.Linq;
using AccountApp.Models;

namespace AccountApp.Services
{
    public class AccountService
    {
        private List<Account> accounts;

        public AccountService()
        {
            accounts = JsonStorage.LoadAccounts() ?? new List<Account>();
            //hardcoded admin account exists. (admin:admin)
            var admin = accounts.FirstOrDefault(a => a.Username == "admin");
            if (admin == null)
            {
                var adm = new Account
                {
                    Username = "admin",
                    LastLocation = "",
                    LoginCounter = 0,
                    TwoFactorEnabled = false,
                    LoginHistory = new System.Collections.Generic.List<string>()
                };
                // generate per-user salt and hash password with it
                adm.Salt = CryptoService.GenerateSaltBase64();
                adm.Password = CryptoService.HashPassword("admin", adm.Salt);
                accounts.Add(adm);
                Save();
            }
            else
            {
                if (string.IsNullOrEmpty(admin.Salt)) admin.Salt = CryptoService.GenerateSaltBase64();
                admin.Password = CryptoService.HashPassword("admin", admin.Salt);
                Save();
            }
        }

        public void Save() => JsonStorage.SaveAccounts(accounts);

        public Account? GetAccount(string? username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            return accounts.FirstOrDefault(a => a.Username == username);
        }

        public List<Account> GetAllAccounts() => accounts;

        public void SaveAccount(Account account)
        {
            var existingAccount = accounts.FirstOrDefault(a => a.Username == account.Username);
            if (existingAccount != null)
            {
                accounts.Remove(existingAccount);
            }
            accounts.Add(account);
            Save();
        }
    }
}
