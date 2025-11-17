using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace checkersclaude
{
    public class AIVsAIForm : Form
    {
        private Board board;
        private Player redAI;
        private Player blackAI;
        private Timer timer;
        private const int CellSize = 50;
        private Dictionary<string, int> boardHistory = new Dictionary<string, int>();

        public AIVsAIForm(Player red, Player black)
        {
            redAI = red;
            blackAI = black;

            board = new Board();
            this.Size = new Size(420, 450);
            this.Text = "AI vs AI Viewer";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;

            timer = new Timer { Interval = 500 };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Rectangle rect = new Rectangle(c * CellSize, r * CellSize, CellSize, CellSize);
                    Brush brush = ((r + c) % 2 == 0) ? Brushes.Beige : Brushes.Brown;
                    g.FillRectangle(brush, rect);

                    Piece p = board.GetPiece(new Position(r, c));
                    if (p != null)
                    {
                        Brush pieceBrush = p.Color == PieceColor.Red ? Brushes.Red : Brushes.Black;
                        g.FillEllipse(pieceBrush, rect.X + 5, rect.Y + 5, CellSize - 10, CellSize - 10);

                        if (p.Type == PieceType.King)
                        {
                            g.DrawString("K", new Font("Arial", 14, FontStyle.Bold),
                                         Brushes.Gold, rect.X + 15, rect.Y + 15);
                        }
                    }
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Player current = board.GetAllPieces(PieceColor.Red).Count >=
                             board.GetAllPieces(PieceColor.Black).Count ? redAI : blackAI;

            var pieces = board.GetAllPieces(current == redAI ? PieceColor.Red : PieceColor.Black);
            var moves = new List<Move>();
            foreach (var p in pieces)
                moves.AddRange(p.GetValidMoves(board));

            if (moves.Count == 0)
            {
                timer.Stop();
                MessageBox.Show("Game Over!");
                return;
            }

            Move chosen = current.ChooseMove(board, moves, current == redAI ? PieceColor.Red : PieceColor.Black);
            board.ApplyMove(chosen);

            RecordBoardState();
            if (IsDraw())
            {
                timer.Stop();
                MessageBox.Show("Draw detected (threefold repetition)!");
                return;
            }

            this.Invalidate();
        }

        private void RecordBoardState()
        {
            string key = EncodeBoardState();
            if (boardHistory.ContainsKey(key))
                boardHistory[key]++;
            else
                boardHistory[key] = 1;
        }

        private bool IsDraw()
        {
            return boardHistory.Values.Any(count => count >= 3);
        }

        private string EncodeBoardState()
        {
            char[] flat = new char[64];
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Piece p = board.GetPiece(new Position(r, c));
                    if (p == null) flat[r * 8 + c] = '0';
                    else if (p.Color == PieceColor.Red && p.Type == PieceType.Regular) flat[r * 8 + c] = 'r';
                    else if (p.Color == PieceColor.Red) flat[r * 8 + c] = 'R';
                    else if (p.Color == PieceColor.Black && p.Type == PieceType.Regular) flat[r * 8 + c] = 'b';
                    else flat[r * 8 + c] = 'B';
                }
            }
            return new string(flat);
        }
    }
}
