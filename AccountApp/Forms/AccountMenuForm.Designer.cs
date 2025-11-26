namespace AccountApp.Forms
{
    partial class AccountMenuForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

    private System.Windows.Forms.TableLayoutPanel centerPanel;
    private System.Windows.Forms.FlowLayoutPanel buttonPanel;

        private void InitializeComponent()
        {
            this.centerPanel = new System.Windows.Forms.TableLayoutPanel();
            this.buttonPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.infoTextBox = new System.Windows.Forms.TextBox();
            this.changePasswordButton = new System.Windows.Forms.Button();
            this.toggle2FAButton = new System.Windows.Forms.Button();
            this.logoutButton = new System.Windows.Forms.Button();
            this.adminButton = new System.Windows.Forms.Button();
            this.centerPanel.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // infoTextBox
            // 
            this.infoTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoTextBox.Multiline = true;
            this.infoTextBox.Name = "infoTextBox";
            this.infoTextBox.ReadOnly = true;
            this.infoTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.infoTextBox.TabIndex = 0;
            // 
            // changePasswordButton
            // 
            this.changePasswordButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.changePasswordButton.Name = "changePasswordButton";
            this.changePasswordButton.Size = new System.Drawing.Size(150, 30);
            this.changePasswordButton.TabIndex = 1;
            this.changePasswordButton.Text = "Change Password";
            this.changePasswordButton.Click += new System.EventHandler(this.ChangePasswordButton_Click);
            // 
            // toggle2FAButton
            // 
            this.toggle2FAButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.toggle2FAButton.Name = "toggle2FAButton";
            this.toggle2FAButton.Size = new System.Drawing.Size(150, 30);
            this.toggle2FAButton.TabIndex = 2;
            this.toggle2FAButton.Text = "Enable 2FA";
            this.toggle2FAButton.Click += new System.EventHandler(this.Toggle2FAButton_Click);
            // 
            // logoutButton
            // 
            this.logoutButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.logoutButton.Name = "logoutButton";
            this.logoutButton.Size = new System.Drawing.Size(150, 30);
            this.logoutButton.TabIndex = 3;
            this.logoutButton.Text = "Logout";
            this.logoutButton.Click += new System.EventHandler(this.LogoutButton_Click);
            // 
            // adminButton
            // 
            this.adminButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.adminButton.Name = "adminButton";
            this.adminButton.Size = new System.Drawing.Size(150, 30);
            this.adminButton.TabIndex = 4;
            this.adminButton.Text = "Admin";
            this.adminButton.Click += new System.EventHandler(this.AdminButton_Click);
            // 
            // AccountMenuForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            // Set up the center table layout (one column, two rows)
            this.centerPanel.ColumnCount = 1;
            this.centerPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.centerPanel.RowCount = 2;
            this.centerPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.centerPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.centerPanel.Dock = System.Windows.Forms.DockStyle.Fill;

            // configure buttonPanel (hosts the three action buttons)
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.buttonPanel.Padding = new System.Windows.Forms.Padding(10);
            this.buttonPanel.WrapContents = false;

            // Add controls in correct order so infoTextBox is below and does not cover buttons
            this.centerPanel.Controls.Add(this.infoTextBox, 0, 0);
            // add admin button at the right-most position (shown only for admin users)
            this.buttonPanel.Controls.Add(this.logoutButton);
            this.buttonPanel.Controls.Add(this.toggle2FAButton);
            this.buttonPanel.Controls.Add(this.changePasswordButton);
            this.buttonPanel.Controls.Add(this.adminButton);
            this.centerPanel.Controls.Add(this.buttonPanel, 0, 1);

            // Add the center panel to the form
            this.Controls.Add(this.centerPanel);
            
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.Name = "AccountMenuForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.buttonPanel.ResumeLayout(false);
            this.centerPanel.ResumeLayout(false);
            this.centerPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TextBox infoTextBox;
        private System.Windows.Forms.Button changePasswordButton;
        private System.Windows.Forms.Button toggle2FAButton;
        private System.Windows.Forms.Button logoutButton;
        private System.Windows.Forms.Button adminButton;
    }
}