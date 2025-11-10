using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace checkersclaude
{
    public class MoveValidator
    {
        private Board board;

        public MoveValidator(Board board)
        {
            this.board = board;
        }



        public List<Move> GetValidMoves(Piece piece)
        {
            if (HasAvailableJumps(piece.Color))
            {
                // Return jumps for this piece using the correct function
                return piece.Type == PieceType.King ? GetValidKingJumps(piece) : GetValidJumps(piece);
            }

            List<Move> moves = new List<Move>();

            if (piece.Type == PieceType.King)
            {
                int[] rowDirs = { -1, 1 };
                int[] colDirs = { -1, 1 };
                foreach (int rowDir in rowDirs)
                {
                    foreach (int colDir in colDirs)
                    {
                        int step = 1;
                        while (true)
                        {
                            int newRow = piece.Position.Row + rowDir * step;
                            int newCol = piece.Position.Col + colDir * step;
                            Position newPos = new Position(newRow, newCol);

                            if (!board.IsValidPosition(newPos) || !board.IsPlayableSquare(newPos))
                                break;

                            if (board.GetPiece(newPos) == null)
                                moves.Add(new Move(piece.Position, newPos));
                            else
                                break;

                            step++;
                        }
                    }
                }
            }
            else
            {
                int[] directions = piece.Color == PieceColor.Red ? new[] { -1 } : new[] { 1 };
                foreach (int rowDir in directions)
                {
                    foreach (int colDir in new[] { -1, 1 })
                    {
                        Position newPos = new Position(piece.Position.Row + rowDir, piece.Position.Col + colDir);
                        if (board.IsPlayableSquare(newPos) && board.GetPiece(newPos) == null)
                            moves.Add(new Move(piece.Position, newPos));
                    }
                }
            }

            return moves;
        }

        public List<Move> GetValidJumps(Piece piece, bool allowBackward = false)
        {
            List<Move> jumps = new List<Move>();
            int[] directions;
            if (piece.Type == PieceType.King || allowBackward)
            {
                directions = new[] { -1, 1 };
            }
            else
            {
                directions = piece.Color == PieceColor.Red ? new[] { -1 } : new[] { 1 };
            }

            foreach (int rowDir in directions)
            {
                foreach (int colDir in new[] { -1, 1 })
                {
                    Position jumpedPos = new Position(
                        piece.Position.Row + rowDir,
                        piece.Position.Col + colDir
                    );

                    Position landingPos = new Position(
                        piece.Position.Row + rowDir * 2,
                        piece.Position.Col + colDir * 2
                    );

                    Piece jumpedPiece = board.GetPiece(jumpedPos);

                    if (board.IsValidPosition(landingPos) &&
                        board.IsPlayableSquare(landingPos) &&
                        board.GetPiece(landingPos) == null &&
                        jumpedPiece != null &&
                        jumpedPiece.Color != piece.Color)
                    {
                        jumps.Add(new Move(piece.Position, landingPos, true, jumpedPos));
                    }
                }
            }

            return jumps;
        }

        // King jumps: only land immediately after the captured piece
        public List<Move> GetValidKingJumps(Piece piece)
        {
            List<Move> jumps = new List<Move>();

            int[] rowDirs = { -1, 1 };
            int[] colDirs = { -1, 1 };

            foreach (int rowDir in rowDirs)
            {
                foreach (int colDir in colDirs)
                {
                    int r = piece.Position.Row + rowDir;
                    int c = piece.Position.Col + colDir;
                    bool enemyFound = false;
                    Position enemyPos = null;

                    // Scan along the diagonal
                    while (board.IsValidPosition(new Position(r, c)) &&
                           board.IsPlayableSquare(new Position(r, c)))
                    {
                        Piece p = board.GetPiece(new Position(r, c));

                        if (p == null)
                        {
                            // Empty square, keep scanning
                            r += rowDir;
                            c += colDir;
                            continue;
                        }

                        if (p.Color == piece.Color)
                        {
                            // Our own piece blocks the diagonal
                            break;
                        }

                        // Enemy piece found
                        enemyPos = new Position(r, c);

                        // Landing square is exactly 1 step beyond the enemy
                        int landingRow = r + rowDir;
                        int landingCol = c + colDir;
                        Position landingPos = new Position(landingRow, landingCol);

                        if (board.IsValidPosition(landingPos) &&
                            board.IsPlayableSquare(landingPos) &&
                            board.GetPiece(landingPos) == null)
                        {
                            jumps.Add(new Move(piece.Position, landingPos, true, enemyPos));
                        }

                        // Cannot jump over multiple pieces in same direction
                        break;
                    }
                }
            }

            return jumps;
        }




        public bool HasAvailableJumps(PieceColor color)
        {
            List<Piece> pieces = board.GetAllPieces(color);
            foreach (Piece piece in pieces)
            {
                if (piece.Type == PieceType.King)
                {
                    if (GetValidKingJumps(piece).Count > 0)
                        return true;
                }
                else
                {
                    if (GetValidJumps(piece).Count > 0)
                        return true;
                }
            }
            return false;
        }



   
    }
}