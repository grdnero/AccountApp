using System;
using System.Windows.Forms;
using AccountApp.Forms;
using AccountApp.Services;

namespace AccountApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var accountService = new AccountService();
            var authService = new AuthService(accountService);
            var loginForm = new LoginForm(authService);

            Application.Run(loginForm);
        }
    }
}
