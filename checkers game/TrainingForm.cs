using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using checkersclaude;

namespace checkersclaude
{
    public class TrainingForm : Form
    {
        private TextBox txtPopulation;
        private TextBox txtGenerations;
        private TextBox txtMutationRate;
        private Button btnStart;
        private Button btnStop;
        private Button btnBack;
        private ProgressBar progressBar;
        private Label lblProgress;
        private TextBox txtLog;
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

            // Population size
            Label lblPopulation = new Label
            {
                Text = "Population Size:",
                Location = new Point(20, 20),
                Size = new Size(120, 20)
            };
            this.Controls.Add(lblPopulation);

            txtPopulation = new TextBox
            {
                Text = "50",
                Location = new Point(150, 18),
                Size = new Size(100, 20)
            };
            this.Controls.Add(txtPopulation);

            // Generations
            Label lblGenerations = new Label
            {
                Text = "Generations:",
                Location = new Point(20, 50),
                Size = new Size(120, 20)
            };
            this.Controls.Add(lblGenerations);

            txtGenerations = new TextBox
            {
                Text = "100",
                Location = new Point(150, 48),
                Size = new Size(100, 20)
            };
            this.Controls.Add(txtGenerations);

            // Mutation Rate
            Label lblMutation = new Label
            {
                Text = "Mutation Rate:",
                Location = new Point(20, 80),
                Size = new Size(120, 20)
            };
            this.Controls.Add(lblMutation);

            txtMutationRate = new TextBox
            {
                Text = "0.1",
                Location = new Point(150, 78),
                Size = new Size(100, 20)
            };
            this.Controls.Add(txtMutationRate);

            // Start button
            btnStart = new Button
            {
                Text = "Start Training",
                Location = new Point(280, 18),
                Size = new Size(120, 30),
                BackColor = Color.Green,
                ForeColor = Color.White
            };
            btnStart.Click += BtnStart_Click;
            this.Controls.Add(btnStart);

            // Stop button
            btnStop = new Button
            {
                Text = "Stop",
                Location = new Point(280, 58),
                Size = new Size(120, 30),
                BackColor = Color.Red,
                ForeColor = Color.White,
                Enabled = false
            };
            btnStop.Click += BtnStop_Click;
            this.Controls.Add(btnStop);

            // Back button
            btnBack = new Button
            {
                Text = "Back to Menu",
                Location = new Point(420, 18),
                Size = new Size(120, 30)
            };
            btnBack.Click += (s, e) => this.Close();
            this.Controls.Add(btnBack);

            // Progress bar
            progressBar = new ProgressBar
            {
                Location = new Point(20, 120),
                Size = new Size(540, 25)
            };
            this.Controls.Add(progressBar);

            // Progress label
            lblProgress = new Label
            {
                Text = "Ready to train",
                Location = new Point(20, 150),
                Size = new Size(540, 20),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblProgress);

            // Log text box
            txtLog = new TextBox
            {
                Location = new Point(20, 180),
                Size = new Size(540, 260),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new Font("Consolas", 9)
            };
            this.Controls.Add(txtLog);
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtPopulation.Text, out int popSize) || popSize < 10)
            {
                MessageBox.Show("Population size must be at least 10", "Invalid Input");
                return;
            }

            if (!int.TryParse(txtGenerations.Text, out int generations) || generations < 1)
            {
                MessageBox.Show("Generations must be at least 1", "Invalid Input");
                return;
            }

            if (!double.TryParse(txtMutationRate.Text, out double mutationRate) ||
                mutationRate <= 0 || mutationRate > 1)
            {
                MessageBox.Show("Mutation rate must be between 0 and 1", "Invalid Input");
                return;
            }

            isTraining = true;
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            txtPopulation.Enabled = false;
            txtGenerations.Enabled = false;
            txtMutationRate.Enabled = false;
            btnBack.Enabled = false;

            progressBar.Maximum = generations;
            progressBar.Value = 0;
            txtLog.Clear();

            await Task.Run(() => TrainAI(popSize, generations, mutationRate));

            btnStart.Enabled = true;
            btnStop.Enabled = false;
            txtPopulation.Enabled = true;
            txtGenerations.Enabled = true;
            txtMutationRate.Enabled = true;
            btnBack.Enabled = true;
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            isTraining = false;
            AppendLog("Training stopped by user.");
        }

        private void TrainAI(int popSize, int generations, double mutationRate)
        {
            currentPopulation = new Population(popSize, mutationRate);
            AppendLog($"Starting training: Pop={popSize}, Gen={generations}, Mutation={mutationRate:F2}\n");

            for (int gen = 0; gen < generations && isTraining; gen++)
            {
                UpdateProgress($"Generation {gen + 1}/{generations} - Running tournament...", gen);

                currentPopulation.RunTournament(gamesPerPair: 2);

                string stats = currentPopulation.GetGenerationStats();
                AppendLog($"Gen {gen + 1}: {stats}");

                currentPopulation.Evolve();
            }

            if (isTraining)
            {
                UpdateProgress($"Training complete! Saving best AI...", generations);
                MainMenuForm.SaveAI(currentPopulation.BestPlayer);
                AppendLog($"\n✓ Training complete! Best fitness: {currentPopulation.BestPlayer.Fitness:F2}");
                AppendLog("AI saved successfully!");

                this.Invoke((Action)(() =>
                {
                    MessageBox.Show($"Training complete!\nBest AI Fitness: {currentPopulation.BestPlayer.Fitness:F2}\n\n" +
                        $"Wins: {currentPopulation.BestPlayer.Wins}\n" +
                        $"Losses: {currentPopulation.BestPlayer.Losses}\n" +
                        $"AI saved and ready to play!",
                        "Training Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }));
            }
        }

        private void UpdateProgress(string message, int value)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() => UpdateProgress(message, value)));
                return;
            }

            lblProgress.Text = message;
            progressBar.Value = Math.Min(value, progressBar.Maximum);
        }

        private void AppendLog(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke((Action)(() => AppendLog(message)));
                return;
            }

            txtLog.AppendText(message + Environment.NewLine);
        }
    }
}
