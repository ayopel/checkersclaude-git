using System;
using System.Collections.Generic;

namespace checkersclaude
{
    public class Board
    {
        private const int BoardSize = 8;
        private Piece[,] squares;

        public Board()
        {
            squares = new Piece[BoardSize, BoardSize];
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            // Place black pieces (top 3 rows)
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if ((row + col) % 2 == 1)
                        squares[row, col] = new Piece(PieceColor.Black, new Position(row, col));
                }
            }

            // Place red pieces (bottom 3 rows)
            for (int row = 5; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if ((row + col) % 2 == 1)
                        squares[row, col] = new Piece(PieceColor.Red, new Position(row, col));
                }
            }
        }

        public Piece GetPiece(Position pos)
        {
            if (IsValidPosition(pos))
                return squares[pos.Row, pos.Col];
            return null;
        }

        public void SetPiece(Position pos, Piece piece)
        {
            if (IsValidPosition(pos))
            {
                squares[pos.Row, pos.Col] = piece;
                if (piece != null)
                    piece.Position = pos;
            }
        }

        public void RemovePiece(Position pos)
        {
            if (IsValidPosition(pos))
                squares[pos.Row, pos.Col] = null;
        }

        public bool IsValidPosition(Position pos)
        {
            return pos.Row >= 0 && pos.Row < BoardSize &&
                   pos.Col >= 0 && pos.Col < BoardSize;
        }

        public bool IsPlayableSquare(Position pos)
        {
            return IsValidPosition(pos) && (pos.Row + pos.Col) % 2 == 1;
        }

        public List<Piece> GetAllPieces(PieceColor color)
        {
            List<Piece> pieces = new List<Piece>();
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    Piece piece = squares[row, col];
                    if (piece != null && piece.Color == color)
                        pieces.Add(piece);
                }
            }
            return pieces;
        }

        public int GetBoardSize()
        {
            return BoardSize;
        }

        // =======================
        // Apply a move (normal or multi-jump)
        // =======================
        public void ApplyMove(Move move)
        {
            Piece piece = GetPiece(move.From);
            if (piece == null) return;

            // Remove the piece from the original position
            RemovePiece(move.From);

            // Remove all jumped pieces (works for multi-jumps)
            if (move.IsJump && move.JumpedPositions != null)
            {
                foreach (Position jumped in move.JumpedPositions)
                {
                    RemovePiece(jumped);
                }
            }

            // Place the piece at the destination
            SetPiece(move.To, piece);

            // Update piece position
            piece.Position = move.To;

            // Optional: Promote to king if it reaches the last row
            if (piece.Type != PieceType.King)
            {
                if ((piece.Color == PieceColor.Red && move.To.Row == 0) ||
                    (piece.Color == PieceColor.Black && move.To.Row == BoardSize - 1))
                {
                    piece.Type = PieceType.King;
                }
            }
        }
    }
}
