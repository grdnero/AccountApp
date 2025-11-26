using System;
using System.Linq;
using System.Windows.Forms;
using AccountApp.Models;
using AccountApp.Services;

namespace AccountApp.Forms
{
    public class AdminForm : Form
    {
    private DataGridView grid = null!;
    private Button refreshButton = null!;
    private Button closeButton = null!;

        public AdminForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Admin - All Accounts";
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new System.Drawing.Size(900, 600);

            grid = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 520,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            refreshButton = new Button { Text = "Refresh", Width = 100, Left = 600, Top = 532 };
            closeButton = new Button { Text = "Close", Width = 100, Left = 720, Top = 532 };

            refreshButton.Click += (s, e) => LoadData();
            closeButton.Click += (s, e) => this.Close();

            this.Controls.Add(grid);
            this.Controls.Add(refreshButton);
            this.Controls.Add(closeButton);
        }

        private void LoadData()
        {
            var svc = new AccountService();
            var accounts = svc.GetAllAccounts();

            grid.Columns.Clear();
            grid.Rows.Clear();

            grid.Columns.Add("Username", "Username");
            grid.Columns.Add("TwoFactor", "2FA Enabled");
            grid.Columns.Add("LoginCounter", "Login Counter");
            grid.Columns.Add("LastLocation", "Last Location");
            grid.Columns.Add("LoginHistory", "Login History");
            grid.Columns.Add("RecoveryHashes", "Recovery Word Hashes");

            foreach (var a in accounts)
            {
                string history = a.LoginHistory != null ? string.Join("\\n", a.LoginHistory) : string.Empty;
                string rh = a.RecoveryWordHashes != null ? string.Join(", ", a.RecoveryWordHashes) : string.Empty;
                grid.Rows.Add(a.Username, a.TwoFactorEnabled, a.LoginCounter, a.LastLocation, history, rh);
            }
        }
    }
}
