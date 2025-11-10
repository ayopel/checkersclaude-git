using checkersclaude;
using System.Collections.Generic;

namespace checkersclaude
{
    public class GameEngine
    {
        public Board Board { get; private set; }
        public GameState State { get; private set; }
        private MoveValidator validator;
        private Piece selectedPiece;
        private bool mustContinueJumping;

        public GameEngine()
        {
            Board = new Board();
            validator = new MoveValidator(Board);
            State = GameState.RedTurn;
            selectedPiece = null;
            mustContinueJumping = false;
        }

        public GameEngine(Board board)
        {
            Board = board;
            validator = new MoveValidator(Board);
            State = GameState.RedTurn;
            selectedPiece = null;
            mustContinueJumping = false;
        }

        public bool SelectPiece(Position pos)
        {
            if (mustContinueJumping && selectedPiece != null)
                return selectedPiece.Position.Equals(pos);

            Piece piece = Board.GetPiece(pos);
            if (piece == null)
                return false;

            PieceColor currentColor = State == GameState.RedTurn ? PieceColor.Red : PieceColor.Black;
            if (piece.Color != currentColor)
                return false;

            selectedPiece = piece;
            return true;
        }

        public bool MovePiece(Position to)
        {
            if (selectedPiece == null)
                return false;

            List<Move> validMoves = validator.GetValidMoves(selectedPiece);
            Move selectedMove = validMoves.Find(m => m.To.Equals(to));

            if (selectedMove == null)
                return false;

            // Execute move
            Board.RemovePiece(selectedPiece.Position);
            Board.SetPiece(to, selectedPiece);

            // Handle jump
            if (selectedMove.IsJump)
            {
                foreach (var jumpedPosition in selectedMove.JumpedPositions)
                {
                    Board.RemovePiece(jumpedPosition);
                }

                // Check for additional jumps
                List<Move> additionalJumps = validator.GetValidJumps(selectedPiece);
                if (additionalJumps.Count > 0)
                {
                    mustContinueJumping = true;
                    return true;
                }
            }

            // Check for king promotion
            if (selectedPiece.Type == PieceType.Regular)
            {
                if ((selectedPiece.Color == PieceColor.Red && to.Row == 0) ||
                    (selectedPiece.Color == PieceColor.Black && to.Row == Board.GetBoardSize() - 1))
                {
                    selectedPiece.PromoteToKing();

                    
                }
            }

            // End turn
            mustContinueJumping = false;
            selectedPiece = null;
            SwitchTurn();
            CheckWinCondition();

            return true;
        }

        public List<Position> GetValidMovePositions()
        {
            if (selectedPiece == null)
                return new List<Position>();

            List<Move> moves = validator.GetValidMoves(selectedPiece);
            List<Position> positions = new List<Position>();
            foreach (Move move in moves)
                positions.Add(move.To);

            return positions;
        }

        private void SwitchTurn()
        {
            State = State == GameState.RedTurn ? GameState.BlackTurn : GameState.RedTurn;
        }

        private void CheckWinCondition()
        {
            PieceColor currentColor = State == GameState.RedTurn ? PieceColor.Red : PieceColor.Black;
            List<Piece> pieces = Board.GetAllPieces(currentColor);

            if (pieces.Count == 0)
            {
                State = currentColor == PieceColor.Red ? GameState.BlackWins : GameState.RedWins;
                return;
            }

            // Check if current player has any valid moves
            bool hasValidMoves = false;
            foreach (Piece piece in pieces)
            {
                if (validator.GetValidMoves(piece).Count > 0)
                {
                    hasValidMoves = true;
                    break;
                }
            }

            if (!hasValidMoves)
            {
                State = currentColor == PieceColor.Red ? GameState.BlackWins : GameState.RedWins;
            }
        }

        public Piece GetSelectedPiece()
        {
            return selectedPiece;
        }

        public void DeselectPiece()
        {
            if (!mustContinueJumping)
                selectedPiece = null;
        }

        public void ResetGame()
        {
            Board = new Board();
            validator = new MoveValidator(Board);
            State = GameState.RedTurn;
            selectedPiece = null;
            mustContinueJumping = false;
        }
    }
}