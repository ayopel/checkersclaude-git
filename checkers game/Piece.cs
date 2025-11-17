using System;
using System.Collections.Generic;

namespace checkersclaude
{
    public class Piece
    {
        public PieceColor Color { get; set; }
        public PieceType Type { get; set; } = PieceType.Regular;
        public Position Position { get; set; }

        public Piece(PieceColor color, Position pos)
        {
            Color = color;
            Position = pos;
        }

        // Returns all valid moves for this piece
        public List<Move> GetValidMoves(Board board)
        {
            List<Move> moves = new List<Move>();
            int dir = Color == PieceColor.Red ? -1 : 1; // Red moves up, Black moves down

            int[] colOffsets = { -1, 1 };

            // Normal moves
            foreach (int c in colOffsets)
            {
                Position target = new Position(Position.Row + dir, Position.Col + c);
                if (board.IsPlayableSquare(target) && board.GetPiece(target) == null)
                {
                    moves.Add(new Move(Position, target));
                }
            }

            // King moves (can go both directions)
            if (Type == PieceType.King)
            {
                foreach (int rDir in new int[] { -1, 1 })
                {
                    foreach (int cDir in new int[] { -1, 1 })
                    {
                        Position target = new Position(Position.Row + rDir, Position.Col + cDir);
                        if (board.IsPlayableSquare(target) && board.GetPiece(target) == null)
                        {
                            moves.Add(new Move(Position, target));
                        }
                    }
                }
            }

            // Jump moves
            moves.AddRange(GetJumpMoves(board, this.Position, new List<Position>()));

            return moves;
        }

        private List<Move> GetJumpMoves(Board board, Position from, List<Position> jumped)
        {
            List<Move> jumpMoves = new List<Move>();
            int[] rowDirs = Type == PieceType.King ? new int[] { -1, 1 } : new int[] { Color == PieceColor.Red ? -1 : 1 };
            int[] colDirs = { -1, 1 };

            foreach (int rDir in rowDirs)
            {
                foreach (int cDir in colDirs)
                {
                    Position over = new Position(from.Row + rDir, from.Col + cDir);
                    Position landing = new Position(from.Row + 2 * rDir, from.Col + 2 * cDir);

                    if (!board.IsPlayableSquare(landing) || board.GetPiece(landing) != null)
                        continue;

                    Piece overPiece = board.GetPiece(over);
                    if (overPiece != null && overPiece.Color != this.Color && !jumped.Contains(over))
                    {
                        List<Position> newJumped = new List<Position>(jumped) { over };
                        Move move = new Move(Position, landing) { IsJump = true, JumpedPositions = newJumped };
                        jumpMoves.Add(move);

                        // Multi-jumps
                        Board boardCopy = CloneBoard(board);
                        boardCopy.RemovePiece(over);
                        boardCopy.SetPiece(from, null);
                        boardCopy.SetPiece(landing, this);

                        var furtherJumps = GetJumpMoves(boardCopy, landing, newJumped);
                        jumpMoves.AddRange(furtherJumps);
                    }
                }
            }

            return jumpMoves;
        }

        private Board CloneBoard(Board board)
        {
            Board clone = new Board();
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Piece p = board.GetPiece(new Position(r, c));
                    if (p != null)
                        clone.SetPiece(new Position(r, c), new Piece(p.Color, p.Position) { Type = p.Type });
                    else
                        clone.RemovePiece(new Position(r, c));
                }
            }
            return clone;
        }
    }

    public enum PieceColor { Red, Black }
    public enum PieceType { Regular, King }
}
