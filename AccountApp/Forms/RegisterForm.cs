using System;
using System.Windows.Forms;
using AccountApp.Models;
using AccountApp.Services;

namespace AccountApp.Forms
{
    public partial class RegisterForm : Form
    {
        private readonly AuthService _authService;

        public RegisterForm(AuthService authService)
        {
            _authService = authService;
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            try
            {
                string username = usernameTextBox.Text;
                string password = passwordTextBox.Text;
                string confirmPassword = confirmPasswordTextBox.Text;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Please fill in all fields.", "Registration Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (password != confirmPassword)
                {
                    MessageBox.Show("Passwords do not match.", "Registration Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var account = _authService.Register(username, password);
                if (account != null)
                {
                    var recoveryWordsText = string.Join(", ", account.RecoveryWords);
                    ShowRecoveryWordsDialog(recoveryWordsText);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Username already exists.", "Registration Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ShowRecoveryWordsDialog(string recoveryWords)
        {
            using (var form = new Form())
            {
                form.Text = "Recovery Words";
                form.ClientSize = new System.Drawing.Size(500, 200);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MinimizeBox = false;
                form.MaximizeBox = false;

                var label = new Label 
                { 
                    Left = 20, 
                    Top = 20, 
                    Text = "Your recovery words (save these in a secure place):",
                    Width = 460,
                    Height = 20
                };

                var textBox = new TextBox 
                { 
                    Left = 20, 
                    Top = 50, 
                    Width = 460, 
                    Height = 80,
                    Multiline = true,
                    ReadOnly = true,
                    Text = recoveryWords,
                    ScrollBars = ScrollBars.Vertical
                };

                var copyButton = new Button 
                { 
                    Text = "Copy to Clipboard", 
                    Left = 150, 
                    Top = 140,
                    Width = 130,
                    Height = 30,
                    DialogResult = DialogResult.None
                };

                var okButton = new Button 
                { 
                    Text = "OK", 
                    Left = 290, 
                    Top = 140,
                    Width = 60,
                    Height = 30,
                    DialogResult = DialogResult.OK 
                };

                copyButton.Click += (s, e) =>
                {
                    try
                    {
                        System.Windows.Forms.Clipboard.SetText(recoveryWords);
                        MessageBox.Show("Recovery words copied to clipboard!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to copy: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                form.Controls.AddRange(new Control[] { label, textBox, copyButton, okButton });
                form.AcceptButton = okButton;
                form.ShowDialog();
            }
        }
    }
}