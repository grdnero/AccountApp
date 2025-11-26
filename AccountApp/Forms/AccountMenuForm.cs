using System;
using System.Windows.Forms;
using AccountApp.Models;
using AccountApp.Services;

namespace AccountApp.Forms
{
    public partial class AccountMenuForm : Form
    {
        private readonly Account _account;
        private readonly AccountService _accountService;

        public AccountMenuForm(Account account)
        {
            _account = account;
            _accountService = new AccountService();
            InitializeComponent();
            this.Text = $"Account Menu - {_account.Username}";
            toggle2FAButton.Text = _account.TwoFactorEnabled ? "Disable 2FA" : "Enable 2FA";
            // show admin button only for admin user
            adminButton.Visible = string.Equals(_account.Username, "admin", StringComparison.OrdinalIgnoreCase);
            UpdateInfoTextBox();
        }

        private void AdminButton_Click(object sender, EventArgs e)
        {
            using var adminForm = new AdminForm();
            adminForm.ShowDialog(this);
        }

        private void UpdateInfoTextBox()
        {
            if (_account.Username == "admin")
            {
                // Admin sees all accounts with decrypted locations (passwords remain hashed)
                var svc = new AccountService();
                var all = svc.GetAllAccounts();
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("ADMIN VIEW - All accounts:");
                foreach (var a in all)
                {
                    sb.AppendLine("-----------------------------");
                    sb.AppendLine($"Username: {a.Username}");
                    sb.AppendLine($"Password (hashed): {a.Password}");
                    sb.AppendLine($"2FA Enabled: {a.TwoFactorEnabled}");
                    sb.AppendLine($"Login Counter: {a.LoginCounter}");
                    sb.AppendLine($"Last Location: {a.LastLocation}");
                    sb.AppendLine("Login History:");
                    if (a.LoginHistory != null) sb.AppendLine(string.Join("\r\n", a.LoginHistory));
                    if (a.RecoveryWordHashes != null && a.RecoveryWordHashes.Count > 0)
                        sb.AppendLine($"Recovery word hashes: {string.Join(", ", a.RecoveryWordHashes)}");
                    sb.AppendLine();
                }
                infoTextBox.Text = sb.ToString();
            }
            else
            {
                infoTextBox.Text = 
                    $"Username: {_account.Username}\r\n" +
                    $"2FA Enabled: {_account.TwoFactorEnabled}\r\n" +
                    $"Login Counter: {_account.LoginCounter}\r\n" +
                    $"Current Location: {_account.LastLocation}\r\n\r\n" +
                    $"Login History:\r\n{string.Join("\r\n", _account.LoginHistory)}";
            }
        }

        private void ChangePasswordButton_Click(object sender, EventArgs e)
        {
            using (var form = new Form())
            {
                form.Text = "Change Password";
                form.ClientSize = new System.Drawing.Size(300, 150);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MinimizeBox = false;
                form.MaximizeBox = false;

                var label = new Label { Left = 20, Top = 20, Text = "New Password:" };
                var textBox = new TextBox { Left = 20, Top = 50, Width = 250, PasswordChar = '•' };
                var buttonOk = new Button { Text = "OK", Left = 100, Top = 80, DialogResult = DialogResult.OK };
                var buttonCancel = new Button { Text = "Cancel", Left = 180, Top = 80, DialogResult = DialogResult.Cancel };

                form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;

                if (form.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(textBox.Text))
                {
                    // Store hashed password so VerifyPassword works correctly
                    if (string.IsNullOrEmpty(_account.Salt)) _account.Salt = Services.CryptoService.GenerateSaltBase64();
                    _account.Password = Services.CryptoService.HashPassword(textBox.Text, _account.Salt);
                    _accountService.SaveAccount(_account);
                    UpdateInfoTextBox();
                    MessageBox.Show("Password changed successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void Toggle2FAButton_Click(object sender, EventArgs e)
        {
            if (_account.TwoFactorEnabled)
            {
                using (var form = new Form())
                {
                    form.Text = "Disable 2FA";
                    form.ClientSize = new System.Drawing.Size(300, 150);
                    form.FormBorderStyle = FormBorderStyle.FixedDialog;
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.MinimizeBox = false;
                    form.MaximizeBox = false;

                    var label = new Label { Left = 20, Top = 20, Text = "Enter Authentication Key:" };
                    var textBox = new TextBox { Left = 20, Top = 50, Width = 250 };
                    var buttonOk = new Button { Text = "OK", Left = 100, Top = 80, DialogResult = DialogResult.OK };
                    var buttonCancel = new Button { Text = "Cancel", Left = 180, Top = 80, DialogResult = DialogResult.Cancel };

                    form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
                    form.AcceptButton = buttonOk;
                    form.CancelButton = buttonCancel;

                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        // Get the current auth key using utility method
                        string currentAuthKey = Services.Utils.CalculateAuthKey(_account);
                        if (textBox.Text == currentAuthKey)
                        {
                            _account.TwoFactorEnabled = false;
                            _accountService.SaveAccount(_account);
                            toggle2FAButton.Text = "Enable 2FA";
                            UpdateInfoTextBox();
                            MessageBox.Show("Two-factor authentication has been disabled.", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Invalid authentication key.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                _account.TwoFactorEnabled = true;
                _accountService.SaveAccount(_account);
                toggle2FAButton.Text = "Disable 2FA";
                UpdateInfoTextBox();
                
                // Show the initial authentication key with copy button
                string authKey = Services.Utils.CalculateAuthKey(_account);
                Show2FAKeyDialog(authKey);
            }
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Show2FAKeyDialog(string authKey)
        {
            using (var form = new Form())
            {
                form.Text = "2FA Authentication Key";
                form.ClientSize = new System.Drawing.Size(450, 180);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MinimizeBox = false;
                form.MaximizeBox = false;

                var label = new Label 
                { 
                    Left = 20, 
                    Top = 20, 
                    Text = "Two-factor authentication has been enabled.",
                    Width = 410,
                    Height = 20
                };

                var keyLabel = new Label 
                { 
                    Left = 20, 
                    Top = 50, 
                    Text = "Your authentication key:",
                    Width = 410,
                    Height = 20
                };

                var textBox = new TextBox 
                { 
                    Left = 20, 
                    Top = 75, 
                    Width = 320, 
                    Height = 27,
                    Text = authKey,
                    ReadOnly = true
                };

                var copyButton = new Button 
                { 
                    Text = "Copy", 
                    Left = 350, 
                    Top = 75,
                    Width = 80,
                    Height = 27,
                    DialogResult = DialogResult.None
                };

                var okButton = new Button 
                { 
                    Text = "OK", 
                    Left = 195, 
                    Top = 120,
                    Width = 60,
                    Height = 30,
                    DialogResult = DialogResult.OK 
                };

                copyButton.Click += (s, e) =>
                {
                    try
                    {
                        System.Windows.Forms.Clipboard.SetText(authKey);
                        MessageBox.Show("Authentication key copied to clipboard!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to copy: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                form.Controls.AddRange(new Control[] { label, keyLabel, textBox, copyButton, okButton });
                form.AcceptButton = okButton;
                form.ShowDialog();
            }
        }
    }
}