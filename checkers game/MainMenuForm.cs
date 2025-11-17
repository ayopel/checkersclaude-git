using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using checkersclaude;

namespace checkersclaude
{
    public class MainMenuForm : Form
    {
        private Button btnPlayHuman;
        private Button btnPlayAI;
        private Button btnTrainAI;
        private Label lblTitle;
        private Label lblStatus;
        private const string AI_FILE = "best_ai.dat";

        public MainMenuForm()
        {
            InitializeUI();
        }


        private void InitializeUI()
        {
            this.Text = "Checkers Game - Main Menu";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Title
            lblTitle = new Label
            {
                Text = "CHECKERS",
                Font = new Font("Arial", 32, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 0, 0),
                Location = new Point(0, 30),
                Size = new Size(500, 60),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            // Play vs Human button
            btnPlayHuman = new Button
            {
                Text = "Play vs Human",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Size = new Size(300, 50),
                Location = new Point(100, 120),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPlayHuman.FlatAppearance.BorderSize = 0;
            btnPlayHuman.Click += BtnPlayHuman_Click;
            this.Controls.Add(btnPlayHuman);

            // Play vs AI button
            btnPlayAI = new Button
            {
                Text = "Play vs AI",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Size = new Size(300, 50),
                Location = new Point(100, 190),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPlayAI.FlatAppearance.BorderSize = 0;
            btnPlayAI.Click += BtnPlayAI_Click;
            this.Controls.Add(btnPlayAI);

            // Train AI button
            btnTrainAI = new Button
            {
                Text = "Train AI",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Size = new Size(300, 50),
                Location = new Point(100, 260),
                BackColor = Color.FromArgb(255, 140, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnTrainAI.FlatAppearance.BorderSize = 0;
            btnTrainAI.Click += BtnTrainAI_Click;
            this.Controls.Add(btnTrainAI);

            // Status label
            lblStatus = new Label
            {
                Text = File.Exists(AI_FILE) ? "✓ Trained AI Available" : "No trained AI yet",
                Font = new Font("Arial", 10, FontStyle.Italic),
                ForeColor = File.Exists(AI_FILE) ? Color.Green : Color.Gray,
                Location = new Point(0, 330),
                Size = new Size(500, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblStatus);
        }

        private void BtnPlayHuman_Click(object sender, EventArgs e)
        {
            CheckersForm gameForm = new CheckersForm(GameMode.HumanVsHuman, null);
            gameForm.FormClosed += GameForm_FormClosed;
            gameForm.Show();
            this.Hide();
        }

        private void BtnPlayAI_Click(object sender, EventArgs e)
        {
            if (!File.Exists(AI_FILE))
            {
                MessageBox.Show("No trained AI found! Please train an AI first.",
                    "No AI Available", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Player aiPlayer = LoadAI();
            if (aiPlayer == null)
            {
                MessageBox.Show("Failed to load AI. Please train a new AI.",
                    "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            CheckersForm gameForm = new CheckersForm(GameMode.HumanVsAI, aiPlayer);
            gameForm.FormClosed += GameForm_FormClosed;
            gameForm.Show();
            this.Hide();
        }

        private void BtnTrainAI_Click(object sender, EventArgs e)
        {
            TrainingForm trainingForm = new TrainingForm();
            trainingForm.FormClosed += (s, args) =>
            {
                lblStatus.Text = File.Exists(AI_FILE) ? "✓ Trained AI Available" : "No trained AI yet";
                lblStatus.ForeColor = File.Exists(AI_FILE) ? Color.Green : Color.Gray;
                this.Show();
            };
            trainingForm.Show();
            this.Hide();
        }

        private void GameForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Show();
        }

        public static void SaveAI(Player player)
        {
            try
            {
                player.Brain.SaveToFile(AI_FILE);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save AI: {ex.Message}", "Save Error");
            }
        }

        public static Player LoadAI()
        {
            try
            {
                NeuralNetwork brain = NeuralNetwork.LoadFromFile(AI_FILE);
                return new Player(brain);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load AI: {ex.Message}", "Load Error");
                return null;
            }
        }
    }
}
