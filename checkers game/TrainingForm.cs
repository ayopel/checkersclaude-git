using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace checkersclaude
{
    public class TrainingForm : Form
    {
        private TextBox txtPopulation, txtGenerations, txtMutationRate, txtLog;
        private Button btnStart, btnStop, btnBack, btnWatchAI;
        private ProgressBar progressBar;
        private Label lblProgress;
        private bool isTraining = false;
        private Population currentPopulation;

        public TrainingForm()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "AI Training";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Population
            this.Controls.Add(new Label { Text = "Population Size:", Location = new Point(20, 20), Size = new Size(120, 20) });
            txtPopulation = new TextBox { Text = "50", Location = new Point(150, 18), Size = new Size(100, 20) };
            this.Controls.Add(txtPopulation);

            // Generations
            this.Controls.Add(new Label { Text = "Generations:", Location = new Point(20, 50), Size = new Size(120, 20) });
            txtGenerations = new TextBox { Text = "100", Location = new Point(150, 48), Size = new Size(100, 20) };
            this.Controls.Add(txtGenerations);

            // Mutation Rate
            this.Controls.Add(new Label { Text = "Mutation Rate:", Location = new Point(20, 80), Size = new Size(120, 20) });
            txtMutationRate = new TextBox { Text = "0.1", Location = new Point(150, 78), Size = new Size(100, 20) };
            this.Controls.Add(txtMutationRate);

            // Buttons
            btnStart = new Button { Text = "Start Training", Location = new Point(280, 18), Size = new Size(120, 30), BackColor = Color.Green, ForeColor = Color.White };
            btnStart.Click += BtnStart_Click;
            this.Controls.Add(btnStart);

            btnStop = new Button { Text = "Stop", Location = new Point(280, 58), Size = new Size(120, 30), BackColor = Color.Red, ForeColor = Color.White, Enabled = false };
            btnStop.Click += BtnStop_Click;
            this.Controls.Add(btnStop);

            btnBack = new Button { Text = "Back to Menu", Location = new Point(420, 18), Size = new Size(120, 30) };
            btnBack.Click += (s, e) => this.Close();
            this.Controls.Add(btnBack);

            btnWatchAI = new Button { Text = "Watch Best AI", Location = new Point(420, 58), Size = new Size(140, 30), BackColor = Color.DarkBlue, ForeColor = Color.White };
            btnWatchAI.Click += BtnWatchAI_Click;
            this.Controls.Add(btnWatchAI);

            // Progress bar & label
            progressBar = new ProgressBar { Location = new Point(20, 120), Size = new Size(540, 25) };
            this.Controls.Add(progressBar);

            lblProgress = new Label { Text = "Ready to train", Location = new Point(20, 150), Size = new Size(540, 20), Font = new Font("Arial", 10, FontStyle.Bold) };
            this.Controls.Add(lblProgress);

            // Log
            txtLog = new TextBox { Location = new Point(20, 180), Size = new Size(540, 260), Multiline = true, ScrollBars = ScrollBars.Vertical, ReadOnly = true, Font = new Font("Consolas", 9) };
            this.Controls.Add(txtLog);
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtPopulation.Text, out int popSize) || popSize < 10) { MessageBox.Show("Population size must be at least 10"); return; }
            if (!int.TryParse(txtGenerations.Text, out int generations) || generations < 1) { MessageBox.Show("Generations must be at least 1"); return; }
            if (!double.TryParse(txtMutationRate.Text, out double mutationRate) || mutationRate <= 0 || mutationRate > 1) { MessageBox.Show("Mutation rate must be between 0 and 1"); return; }

            isTraining = true;
            btnStart.Enabled = false; btnStop.Enabled = true;
            txtPopulation.Enabled = false; txtGenerations.Enabled = false; txtMutationRate.Enabled = false; btnBack.Enabled = false;

            progressBar.Maximum = generations; progressBar.Value = 0;
            txtLog.Clear();

            await Task.Run(() => TrainAI(popSize, generations, mutationRate));

            btnStart.Enabled = true; btnStop.Enabled = false;
            txtPopulation.Enabled = true; txtGenerations.Enabled = true; txtMutationRate.Enabled = true; btnBack.Enabled = true;
        }

        private void BtnStop_Click(object sender, EventArgs e) => isTraining = false;

        private void BtnWatchAI_Click(object sender, EventArgs e)
        {
            if (currentPopulation == null || currentPopulation.BestPlayer == null)
            {
                MessageBox.Show("No trained AI available yet!", "Error");
                return;
            }
            var redAI = currentPopulation.BestPlayer.Clone();
            var blackAI = currentPopulation.BestPlayer.Clone();
            AIVsAIForm viewer = new AIVsAIForm(redAI, blackAI);
            viewer.Show();
        }

        private void TrainAI(int popSize, int generations, double mutationRate)
        {
            currentPopulation = new Population(popSize, mutationRate);
            AppendLog($"Starting training: Pop={popSize}, Gen={generations}, Mutation={mutationRate:F2}\n");

            for (int gen = 0; gen < generations && isTraining; gen++)
            {
                UpdateProgress($"Generation {gen + 1}/{generations} - Running tournament...", gen);
                currentPopulation.RunTournament(gamesPerPair: 2);
                currentPopulation.Evolve();
                AppendLog($"Gen {gen + 1}: Best Fitness={currentPopulation.BestPlayer.Fitness:F2}");
            }

            if (isTraining)
            {
                MainMenuForm.SaveAI(currentPopulation.BestPlayer);
                AppendLog($"\n✓ Training complete! Best fitness: {currentPopulation.BestPlayer.Fitness:F2}");
                this.Invoke((Action)(() =>
                {
                    MessageBox.Show($"Training complete!\nBest AI Fitness: {currentPopulation.BestPlayer.Fitness:F2}", "Training Complete");
                }));
            }
        }

        private void UpdateProgress(string message, int value)
        {
            if (this.InvokeRequired) { this.Invoke((Action)(() => UpdateProgress(message, value))); return; }
            lblProgress.Text = message; progressBar.Value = Math.Min(value, progressBar.Maximum);
        }

        private void AppendLog(string message)
        {
            if (txtLog.InvokeRequired) { txtLog.Invoke((Action)(() => AppendLog(message))); return; }
            txtLog.AppendText(message + Environment.NewLine);
        }
    }
}
