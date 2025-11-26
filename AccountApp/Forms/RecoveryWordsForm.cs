using System;
using System.Windows.Forms;
using AccountApp.Models;
using AccountApp.Services;

namespace AccountApp.Forms
{
    public partial class RecoveryWordsForm : Form
    {
        private readonly Account account;
        private readonly TextBox[] answerBoxes = new TextBox[4];
        private readonly int[] wordPositions = new int[4];

        public RecoveryWordsForm(Account account)
        {
            this.account = account;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Verify Recovery Words";
            this.Size = new System.Drawing.Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;

            var rnd = new Random();
            var usedPositions = new System.Collections.Generic.HashSet<int>();
            
            for (int i = 0; i < 4; i++)
            {
                int pos;
                do
                {
                    pos = rnd.Next(account.RecoveryWords.Count);
                } while (!usedPositions.Add(pos));
                wordPositions[i] = pos;

                var label = new Label
                {
                    Text = $"Word at position {pos + 1}:",
                    Location = new System.Drawing.Point(50, 50 + i * 40),
                    Size = new System.Drawing.Size(120, 20)
                };

                answerBoxes[i] = new TextBox
                {
                    Location = new System.Drawing.Point(180, 50 + i * 40),
                    Size = new System.Drawing.Size(150, 20)
                };

                this.Controls.AddRange(new Control[] { label, answerBoxes[i] });
            }

            var verifyButton = new Button
            {
                Text = "Verify",
                Location = new System.Drawing.Point(100, 210),
                Size = new System.Drawing.Size(100, 30)
            };

            var copyButton = new Button
            {
                Text = "Copy Words",
                Location = new System.Drawing.Point(210, 210),
                Size = new System.Drawing.Size(100, 30)
            };

            var timer = new System.Windows.Forms.Timer { Interval = 10000 };
            var startTime = DateTime.Now;

            verifyButton.Click += (s, e) =>
            {
                var providedWords = account.RecoveryWords.ToArray();
                for (int i = 0; i < 4; i++)
                {
                    providedWords[wordPositions[i]] = answerBoxes[i].Text;
                }

                if (CryptoService.HashRecoveryWords(providedWords) != account.HashedRecoveryWords)
                {
                    MessageBox.Show("Wrong recovery words! Please wait 10 seconds...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    verifyButton.Enabled = false;
                    foreach (var box in answerBoxes) box.Enabled = false;
                    timer.Start();
                    return;
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            timer.Tick += (s, e) =>
            {
                timer.Stop();
                verifyButton.Enabled = true;
                copyButton.Enabled = true;
                foreach (var box in answerBoxes) box.Enabled = true;
                foreach (var box in answerBoxes) box.Text = "";
                answerBoxes[0].Focus();
            };

            copyButton.Click += (s, e) =>
            {
                try
                {
                    var wordsText = string.Join(", ", account.RecoveryWords);
                    System.Windows.Forms.Clipboard.SetText(wordsText);
                    MessageBox.Show("Recovery words copied to clipboard!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to copy: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            this.Controls.Add(verifyButton);
            this.Controls.Add(copyButton);
        }
    }
}