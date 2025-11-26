using System;
using System.Windows.Forms;
using AccountApp.Models;
using AccountApp.Services;

namespace AccountApp.Forms
{
    public class ForgotPasswordForm : Form
    {
        private readonly AuthService _authService;
    private TextBox usernameTextBox = null!;
        private Button submitButton = null!;
        private Button cancelButton = null!;

        public ForgotPasswordForm(AuthService authService)
        {
            _authService = authService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Forgot Password";
            this.ClientSize = new System.Drawing.Size(480, 220);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            var lblUser = new Label { Left = 20, Top = 20, Text = "Username:", AutoSize = true };
            usernameTextBox = new TextBox { Left = 120, Top = 18, Width = 320 };

            submitButton = new Button { Text = "Submit", Left = 260, Top = 140, DialogResult = DialogResult.None };
            cancelButton = new Button { Text = "Cancel", Left = 340, Top = 140, DialogResult = DialogResult.Cancel };

            submitButton.Click += SubmitButton_Click;
            cancelButton.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblUser, usernameTextBox, submitButton, cancelButton });
        }

        private void SubmitButton_Click(object? sender, EventArgs e)
        {
            string username = usernameTextBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Please enter username.", "Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var account = _authService.GetAccount(username);
            if (account == null)
            {
                MessageBox.Show("Account not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Respect account lockout
            if (account.LockedUntil.HasValue && account.LockedUntil.Value > Utils.GetNow())
            {
                var remaining = account.LockedUntil.Value - Utils.GetNow();
                MessageBox.Show($"Account locked. Try again in {Math.Ceiling(remaining.TotalSeconds)} seconds.", "Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (account.RecoveryWordHashes == null || account.RecoveryWordHashes.Count < 4)
            {
                MessageBox.Show("Recovery words are not properly configured for this account.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Pick 4 random slots
            var rnd = new Random();
            var indices = new System.Collections.Generic.HashSet<int>();
            while (indices.Count < 4) indices.Add(rnd.Next(0, account.RecoveryWordHashes.Count));

            using var dlg = new Form();
            dlg.Text = "Password Recovery - Verify Slots";
            dlg.ClientSize = new System.Drawing.Size(480, 260);
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.StartPosition = FormStartPosition.CenterParent;

            var boxes = new System.Collections.Generic.List<TextBox>();
            int y = 20;
            foreach (var idx in indices)
            {
                var lbl = new Label { Left = 20, Top = y, Text = $"What is at slot {idx + 1}?", AutoSize = true };
                var tb = new TextBox { Left = 200, Top = y - 3, Width = 240 }; // larger input
                dlg.Controls.Add(lbl);
                dlg.Controls.Add(tb);
                boxes.Add(tb);
                y += 40;
            }

            var ok = new Button { Text = "OK", Left = 200, Top = y, DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Cancel", Left = 280, Top = y, DialogResult = DialogResult.Cancel };
            dlg.Controls.Add(ok);
            dlg.Controls.Add(cancel);
            dlg.AcceptButton = ok;
            dlg.CancelButton = cancel;

            if (dlg.ShowDialog() != DialogResult.OK) return;

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
                _authService.SaveAccount(account);
                var remaining = account.LockedUntil.Value - Utils.GetNow();
                MessageBox.Show($"Incorrect words. Account locked for {Math.Ceiling(remaining.TotalSeconds)} seconds.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // All correct: generate a strong random password with printable ASCII chars
            var newPass = GenerateRandomPassword(16);
            if (string.IsNullOrEmpty(account.Salt)) account.Salt = CryptoService.GenerateSaltBase64();
            account.Password = CryptoService.HashPassword(newPass, account.Salt);
            _authService.SaveAccount(account);

            try
            {
                System.Windows.Forms.Clipboard.SetText(newPass);
            }
            catch { }
            MessageBox.Show($"Your new password is:\n{newPass}\n\n(The password has been copied to the clipboard.)\nPlease save it securely.", "Password Reset", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private static string GenerateRandomPassword(int length)
        {
            var poolChars = new System.Text.StringBuilder();
            for (int c = 33; c <= 126; c++) poolChars.Append((char)c);
            var pool = poolChars.ToString();

            var data = new byte[length];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(data);
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = pool[data[i] % pool.Length];
            }
            return new string(result);
        }
    }
}
    