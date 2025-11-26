using System;
using System.Windows.Forms;
using AccountApp.Models;
using AccountApp.Services;

namespace AccountApp.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _authService;

        public LoginForm(AuthService authService)
        {
            _authService = authService;
            InitializeComponent();
            this.Load += LoginForm_Load;
        }

        private void LoginForm_Load(object? sender, EventArgs e)
        {
            UpdateStatusLabels();
        }

        private void UpdateStatusLabels()
        {
            try
            {
                this.locationLabel.Text = $"Location: {Services.Utils.CurrentLocation}";
                this.timeLabel.Text = $"Time: {Services.Utils.GetNow():yyyy-MM-dd HH:mm:ss}";
            }
            catch
            {
                // ignore
            }
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            try
            {
                string username = usernameTextBox.Text;
                string password = passwordTextBox.Text;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Please enter both username and password.", "Login Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var account = _authService.Login(username, password);
                if (account != null)
                {
                    var accountMenuForm = new AccountMenuForm(account);
                    this.Hide();
                    accountMenuForm.FormClosed += (s, args) => this.Show();
                    accountMenuForm.ShowDialog();
                    // Clear the form fields after returning
                    usernameTextBox.Clear();
                    passwordTextBox.Clear();
                }
                else
                {
                    // Generic failure message (do not expose hashes or internal state)
                    MessageBox.Show("Invalid username or password.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            var registerForm = new RegisterForm(_authService);
            this.Hide();
            registerForm.FormClosed += (s, args) => this.Show();
            registerForm.ShowDialog();
        }

        private void ForgotButton_Click(object sender, EventArgs e)
        {
            // Open a simple prompt to start forgot-password flow
            var dlg = new Forms.ForgotPasswordForm(_authService);
            this.Hide();
            dlg.FormClosed += (s, args) => this.Show();
            dlg.ShowDialog();
            UpdateStatusLabels();
        }

        private void ChangeLocationButton_Click(object sender, EventArgs e)
        {
            // prompt for a new location
            var input = Microsoft.VisualBasic.Interaction.InputBox("Enter current location (e.g. City, Country):", "Change Location", Services.Utils.CurrentLocation);
            if (!string.IsNullOrWhiteSpace(input))
            {
                Services.Utils.CurrentLocation = input.Trim();
                UpdateStatusLabels();
            }
        }

        private void ChangeDateTimeButton_Click(object sender, EventArgs e)
        {
            // prompt for a new date/time in yyyy-MM-dd HH:mm:ss or empty to reset
            var input = Microsoft.VisualBasic.Interaction.InputBox("Enter date/time (yyyy-MM-dd HH:mm:ss) or leave empty to use system time:", "Change Date/Time", "");
            if (string.IsNullOrWhiteSpace(input))
            {
                Services.Utils.ResetNow();
            }
            else
            {
                if (DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out var dt))
                {
                    Services.Utils.SetNow(dt);
                }
                else
                {
                    MessageBox.Show("Invalid date/time format. Use yyyy-MM-dd HH:mm:ss.", "Invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            UpdateStatusLabels();
        }
    }
}