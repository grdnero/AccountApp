using System;
using System.Linq;
using System.Windows.Forms;
using AccountApp.Models;

namespace AccountApp.Services
{
    public class AuthService
    {
        private readonly AccountService _accountService;

        public AuthService(AccountService accountService)
        {
            _accountService = accountService;
        }

        // Expose account lookup for UI flows like forgot-password
        public Account? GetAccount(string username)
        {
            return _accountService.GetAccount(username);
        }

        // Expose save for UI helpers
        public void SaveAccount(Account account)
        {
            _accountService.SaveAccount(account);
        }

        public Account? Login(string username, string password)
        {
            var account = _accountService.GetAccount(username);
            if (account == null) return null;

            // check lockout
            if (account.LockedUntil.HasValue && account.LockedUntil.Value > Utils.GetNow())
            {
                var remaining = account.LockedUntil.Value - Utils.GetNow();
                MessageBox.Show($"Account locked. Try again in {remaining.Seconds} seconds.", "Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            if (!CryptoService.VerifyPassword(password, account.Password, account.Salt)) 
            {
                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            var currentLocation = Utils.CurrentLocation;
            
            // Skip location verification for admin user
            // Skip location verification for admin user
            // Only treat location as different if this is not the first login (has prior history),
            // and the stored last location is non-empty and differs from the current location.
            bool locationDifferent = !string.Equals(account.Username, "admin", StringComparison.OrdinalIgnoreCase) &&
                                   !string.Equals(account.LastLocation, currentLocation, StringComparison.OrdinalIgnoreCase) &&
                                   !string.IsNullOrWhiteSpace(account.LastLocation) &&
                                   account.LoginHistory != null && account.LoginHistory.Count > 0;

            // determine how much to increment the login counter on successful login
            int incrementAmount = locationDifferent ? 2 : 1;

            if (locationDifferent)
            {
                if (account.RecoveryWordHashes == null || account.RecoveryWordHashes.Count < 4)
                {
                    MessageBox.Show("Recovery words not set up properly. Please contact administrator.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                // Always do slot-based challenge (4 random slots)
                var rnd = new Random();
                var indices = new System.Collections.Generic.HashSet<int>();
                while (indices.Count < 4) indices.Add(rnd.Next(0, account.RecoveryWordHashes.Count));

                using var dlg = new Form();
                dlg.Text = "Location verification";
                dlg.ClientSize = new System.Drawing.Size(360, 260);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterScreen;

                var boxes = new System.Collections.Generic.List<TextBox>();
                int y = 20;
                foreach (var idx in indices)
                {
                    var lbl = new Label { Left = 20, Top = y, Text = $"What is at slot {idx + 1}?", AutoSize = true };
                    var tb = new TextBox { Left = 160, Top = y - 3, Width = 170 };
                    dlg.Controls.Add(lbl);
                    dlg.Controls.Add(tb);
                    boxes.Add(tb);
                    y += 40;
                }

                var ok = new Button { Text = "OK", Left = 160, Top = y, DialogResult = DialogResult.OK };
                var cancel = new Button { Text = "Cancel", Left = 240, Top = y, DialogResult = DialogResult.Cancel };
                dlg.Controls.Add(ok);
                dlg.Controls.Add(cancel);
                dlg.AcceptButton = ok;
                dlg.CancelButton = cancel;

                if (dlg.ShowDialog() != DialogResult.OK) return null;

                int counter = 0;
                bool allCorrect = true;
                foreach (var idx in indices)
                {
                    var provided = boxes[counter++].Text.Trim();
                    if (CryptoService.HashWord(provided) != account.RecoveryWordHashes[idx])
                    {
                        allCorrect = false;
                        break;
                    }
                }

                if (!allCorrect)
                {
                    account.LockedUntil = Utils.GetNow().AddSeconds(10);
                    _accountService.SaveAccount(account);
                    MessageBox.Show("Incorrect recovery words. Account locked for 10 seconds.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            } // end of location check for non-admin users

            if (account.TwoFactorEnabled)
            {
                using var dlg = new Form();
                dlg.Text = "2FA Verification";
                dlg.ClientSize = new System.Drawing.Size(300, 150);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterScreen;

                var centerPanel = new Panel { Size = new System.Drawing.Size(260, 120), Anchor = AnchorStyles.None, Location = new System.Drawing.Point(20, 15) };
                var label = new Label { AutoSize = true, Text = "Enter Authentication Key:", Location = new System.Drawing.Point(0, 10) };
                var textBox = new TextBox { Width = 250, Location = new System.Drawing.Point(0, 35) };
                var buttonOk = new Button { Text = "OK", Location = new System.Drawing.Point(80, 70), DialogResult = DialogResult.OK };
                var buttonCancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(160, 70), DialogResult = DialogResult.Cancel };
                centerPanel.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
                dlg.Controls.Add(centerPanel);
                dlg.AcceptButton = buttonOk;
                dlg.CancelButton = buttonCancel;

                while (true)
                {
                    if (dlg.ShowDialog() != DialogResult.OK) return null;

                    // Compute expected key for the current counter
                    string expectedNow = GenerateAuthKey(account);

                    // Also compute expected key for the previous counter (allow one-step desync)
                    int prevCounter = (account.LoginCounter - 1 + 64) % 64;
                    var tmp = new Account { HashedRecoveryWords = account.HashedRecoveryWords ?? string.Empty, LoginCounter = prevCounter };
                    string expectedPrev = GenerateAuthKey(tmp);

                    if (textBox.Text == expectedNow || textBox.Text == expectedPrev)
                    {
                        // don't update the counter here — update once after all checks so
                        // counter increment is consistent for 2FA and non-2FA logins
                        break;
                    }

                    // Generic failure notice for invalid 2FA input
                    MessageBox.Show("Invalid 2FA code.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox.Clear();
                    textBox.Focus();
                }
            }

            account.LastLocation = currentLocation;
            if (account.LoginHistory == null) account.LoginHistory = new System.Collections.Generic.List<string>();
            account.LoginHistory.Add(Utils.GetNow().ToString("yyyy-MM-dd HH:mm:ss"));

            // Update login counter consistently for all successful logins
            account.LoginCounter = (account.LoginCounter + incrementAmount) % 64;

            _accountService.SaveAccount(account);

            return account;
        }

        public Account? Register(string username, string password)
        {
            if (_accountService.GetAccount(username) != null)
            {
                return null; // Username already exists
            }

            var recoveryWords = Utils.GenerateRecoveryWords();
            var account = new Account
            {
                Username = username,
                LastLocation = "Default",
                LoginCounter = 0,
                RecoveryWords = recoveryWords,
                RecoveryWordHashes = recoveryWords.Select(w => CryptoService.HashWord(w)).ToList(),
                HashedRecoveryWords = CryptoService.HashRecoveryWords(recoveryWords.ToArray()),
                LoginHistory = new System.Collections.Generic.List<string>()
            };

            // generate per-user salt and hash password
            account.Salt = CryptoService.GenerateSaltBase64();
            account.Password = CryptoService.HashPassword(password, account.Salt);

            _accountService.SaveAccount(account);
            return account;
        }

        public string GenerateAuthKey(Account account)
        {
            // Use Utils helper to ensure consistency
            return Utils.CalculateAuthKey(account);
        }

        public bool VerifyRecoveryWords(Account account, string[] providedWords)
        {
            // Instead of comparing plaintext words (we don't store them), hash the provided
            // words in order and compare to the stored hash.
            try
            {
                string providedHash = CryptoService.HashRecoveryWords(providedWords);
                return providedHash == account.HashedRecoveryWords;
            }
            catch
            {
                return false; 
            }
        }
    }
}
