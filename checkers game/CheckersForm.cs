using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Windows.Forms;

namespace checkersclaude
{

    public partial class CheckersForm : Form
    {
        private GameEngine game;
        private Button[,] boardButtons;
        private const int SquareSize = 70;
        private Label statusLabel;
        private Button resetButton;
        private GameMode mode;
        private Player aiPlayer;

        public GameMode HumanVsHuman { get; }
        public object Value { get; }

        public CheckersForm()
        {
            InitializeComponent();
            game = new GameEngine();
            CreateBoard();
            UpdateBoard();
        }

        public CheckersForm(GameMode mode, object value)
        {
            InitializeComponent();
            game = new GameEngine();
            CreateBoard();
            UpdateBoard();
            this.mode = mode;
            this.aiPlayer = value as Player;
        }
         private void MakeAIMove()
        {
            if (aiPlayer == null) return;

            // Find all valid moves for AI's pieces
            var pieces = game.Board.GetAllPieces(PieceColor.Black);
            foreach (var piece in pieces)
            {
                var moves = new MoveValidator(game.Board).GetValidMoves(piece);
                if (moves.Count > 0)
                {
                    Move bestMove = aiPlayer.ChooseMove(game.Board, moves, piece.Color);


                    // Select and move
                    game.SelectPiece(piece.Position);
                    game.MovePiece(bestMove.To);
                    UpdateBoard();
                    break;
                }
            }
        }



        private void InitializeComponent()
        {
            this.Text = "Checkers Game";
            this.ClientSize = new Size(SquareSize * 8, SquareSize * 8 + 80);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Status label
            statusLabel = new Label
            {
                Location = new Point(10, SquareSize * 8 + 10),
                Size = new Size(300, 30),
                Font = new Font("Arial", 14, FontStyle.Bold),
                Text = "Red's Turn"
            };
            this.Controls.Add(statusLabel);

            // Reset button
            resetButton = new Button
            {
                Location = new Point(SquareSize * 8 - 110, SquareSize * 8 + 10),
                Size = new Size(100, 30),
                Text = "New Game",
                Font = new Font("Arial", 10)
            };
            resetButton.Click += ResetButton_Click;
            this.Controls.Add(resetButton);
        }

        private void CreateBoard()
        {
            boardButtons = new Button[8, 8];

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Button btn = new Button
                    {
                        Size = new Size(SquareSize, SquareSize),
                        Location = new Point(col * SquareSize, row * SquareSize),
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Arial", 24, FontStyle.Bold),
                        Tag = new Position(row, col)
                    };

                    // Set square color
                    if ((row + col) % 2 == 0)
                        btn.BackColor = Color.FromArgb(240, 217, 181); // Light square
                    else
                        btn.BackColor = Color.FromArgb(181, 136, 99); // Dark square

                    btn.Click += Square_Click;
                    boardButtons[row, col] = btn;
                    this.Controls.Add(btn);
                }
            }
        }

        private void UpdateBoard()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Position pos = new Position(row, col);
                    Piece piece = game.Board.GetPiece(pos);
                    Button btn = boardButtons[row, col];

                    // Reset button appearance
                    btn.Text = "";
                    btn.ForeColor = Color.Black;
                    btn.FlatAppearance.BorderSize = 0;

                    if ((row + col) % 2 == 0)
                        btn.BackColor = Color.FromArgb(240, 217, 181);
                    else
                        btn.BackColor = Color.FromArgb(181, 136, 99);

                    // Draw piece
                    if (piece != null)
                    {
                        btn.Text = piece.Type == PieceType.King ? "♔" : "●";
                        btn.ForeColor = piece.Color == PieceColor.Red ?
                            Color.FromArgb(200, 0, 0) : Color.FromArgb(50, 50, 50);
                    }

                    // Highlight selected piece
                    if (game.GetSelectedPiece() != null &&
                        game.GetSelectedPiece().Position.Equals(pos))
                    {
                        btn.FlatAppearance.BorderSize = 3;
                        btn.FlatAppearance.BorderColor = Color.Yellow;
                    }

                    // Highlight valid moves
                    var validMoves = game.GetValidMovePositions();
                    foreach (Position validPos in validMoves)
                    {
                        if (validPos.Equals(pos))
                        {
                            boardButtons[row, col].BackColor = Color.FromArgb(144, 238, 144);
                        }
                    }
                }
            }

            // Update status
            switch (game.State)
            {
                case GameState.RedTurn:
                    statusLabel.Text = "Red's Turn";
                    statusLabel.ForeColor = Color.FromArgb(200, 0, 0);
                    break;
                case GameState.BlackTurn:
                    statusLabel.Text = "Black's Turn";
                    statusLabel.ForeColor = Color.FromArgb(50, 50, 50);
                    break;
                case GameState.RedWins:
                    statusLabel.Text = "Red Wins!";
                    statusLabel.ForeColor = Color.FromArgb(200, 0, 0);
                    MessageBox.Show("Red Wins!", "Game Over", MessageBoxButtons.OK);
                    break;
                case GameState.BlackWins:
                    statusLabel.Text = "Black Wins!";
                    statusLabel.ForeColor = Color.FromArgb(50, 50, 50);
                    MessageBox.Show("Black Wins!", "Game Over", MessageBoxButtons.OK);
                    break;
            }
        }

        private void Square_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            Position pos = (Position)btn.Tag;

            if (game.GetSelectedPiece() == null)
            {
                // Try to select a piece
                if (game.SelectPiece(pos))
                {
                    UpdateBoard();
                }
            }
            else
            {
                // Try to move the selected piece
                if (game.MovePiece(pos))
                {
                    UpdateBoard();
                    // If playing vs AI and it's now the AI's turn, let AI move
                    if (mode == GameMode.HumanVsAI && game.State == GameState.BlackTurn)
                    {
                        MakeAIMove();
                    }
                }
                else
                {
                    // Deselect and try to select another piece
                    game.DeselectPiece();
                    if (game.SelectPiece(pos))
                    {
                        UpdateBoard();
                    }
                }
            }
        }


        private void ResetButton_Click(object sender, EventArgs e)
        {
            game.ResetGame();
            UpdateBoard();
        }

    }
}

