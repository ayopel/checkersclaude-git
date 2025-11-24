// ============================================================================
// UPDATED VERSION - FIXES THE CLEARCACHE ERROR
// ============================================================================
// This is the FINAL version - use this one!
// Replace your AI/AIPlayer.cs with this entire file
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using checkersclaude.AI;

namespace checkersclaude
{
    public class AIPlayer
    {
        public DeepNeuralNetwork Brain { get; private set; }
        public PlayerStats Stats { get; private set; }

        private const int InputSize = 64;
        private static readonly int[] HiddenSizes = { 128, 64, 32 };
        private const int OutputSize = 1;

        public AIPlayer(Random random = null)
        {
            Brain = new DeepNeuralNetwork(InputSize, HiddenSizes, OutputSize, random);
            Stats = new PlayerStats();
        }

        public AIPlayer(DeepNeuralNetwork brain)
        {
            Brain = brain;
            Stats = new PlayerStats();
        }

        public Move ChooseMove(Board board, List<Move> validMoves, PieceColor color)
        {
            if (validMoves == null || validMoves.Count == 0)
                return null;

            if (validMoves.Count == 1)
                return validMoves[0];

            double bestScore = double.MinValue;
            Move bestMove = validMoves[0];

            foreach (Move move in validMoves)
            {
                double score = EvaluateMove(board, move, color);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            Stats.TotalMoves++;
            return bestMove;
        }

        private double EvaluateMove(Board board, Move move, PieceColor color)
        {
            double[] boardState = GetEnhancedBoardState(board, move, color);
            double[] output = Brain.FeedForward(boardState);

            double strategicValue = CalculateEnhancedStrategicValue(board, move, color);
            double positionalValue = CalculateTacticalAdvantage(board, move, color);

            return output[0] + strategicValue * 0.15 + positionalValue * 0.1;
        }

        private double[] GetEnhancedBoardState(Board board, Move move, PieceColor color)
        {
            double[] state = new double[InputSize];
            Board simBoard = SimulateMove(board, move);

            int index = 0;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Position pos = new Position(row, col);
                    Piece piece = simBoard.GetPiece(pos);

                    if (piece == null)
                    {
                        state[index] = 0.0;
                    }
                    else if (piece.Color == color)
                    {
                        double value = piece.Type == PieceType.King ? 3.0 : 1.0;
                        value += GetEnhancedPositionValue(pos, piece, color);

                        if (CanPieceMove(simBoard, piece))
                            value += 0.2;

                        if (IsPieceProtected(simBoard, pos, color))
                            value += 0.3;

                        state[index] = value;
                    }
                    else
                    {
                        double value = piece.Type == PieceType.King ? -3.0 : -1.0;
                        value -= GetEnhancedPositionValue(pos, piece, piece.Color);

                        if (CanPieceMove(simBoard, piece))
                            value -= 0.2;

                        if (IsPieceProtected(simBoard, pos, piece.Color))
                            value -= 0.3;

                        state[index] = value;
                    }

                    index++;
                }
            }

            return state;
        }

        private double GetEnhancedPositionValue(Position pos, Piece piece, PieceColor color)
        {
            double value = 0.0;

            // Center control
            double centerValue = CalculateCenterControl(pos);
            value += centerValue * 0.4;

            // Advancement bonus
            double advancementValue = CalculateAdvancement(pos, piece, color);
            value += advancementValue * 0.3;

            // King row proximity
            if (piece.Type != PieceType.King)
            {
                double promotionProximity = CalculatePromotionProximity(pos, color);
                value += promotionProximity * 0.5;
            }

            // Edge penalty
            if (pos.Col == 0 || pos.Col == 7)
                value -= 0.15;

            // Back row defense
            if ((color == PieceColor.Red && pos.Row == 7) ||
                (color == PieceColor.Black && pos.Row == 0))
            {
                value += 0.2;
            }

            // Diagonal control
            value += CalculateDiagonalControl(pos) * 0.2;

            return value;
        }

        private double CalculateCenterControl(Position pos)
        {
            double distanceFromCenter = Math.Sqrt(
                Math.Pow(pos.Row - 3.5, 2) + Math.Pow(pos.Col - 3.5, 2));
            return Math.Max(0, 1.0 - (distanceFromCenter / 5.0));
        }

        private double CalculateAdvancement(Position pos, Piece piece, PieceColor color)
        {
            if (piece.Type == PieceType.King)
                return 0.0;

            if (color == PieceColor.Red)
                return (7 - pos.Row) / 7.0;
            else
                return pos.Row / 7.0;
        }

        private double CalculatePromotionProximity(Position pos, PieceColor color)
        {
            int rowsToKing;
            if (color == PieceColor.Red)
                rowsToKing = pos.Row;
            else
                rowsToKing = 7 - pos.Row;

            return Math.Pow((7.0 - rowsToKing) / 7.0, 2);
        }

        private double CalculateDiagonalControl(Position pos)
        {
            double value = 0.0;

            if (pos.Row == pos.Col)
                value += 0.3;

            if (pos.Row + pos.Col == 7)
                value += 0.3;

            return value;
        }

        private double CalculateEnhancedStrategicValue(Board board, Move move, PieceColor color)
        {
            double value = 0.0;

            // Capture value
            if (move.IsJump)
            {
                double captureValue = 0.0;

                foreach (Position jumpedPos in move.JumpedPositions)
                {
                    Piece capturedPiece = board.GetPiece(jumpedPos);
                    if (capturedPiece != null)
                    {
                        captureValue += capturedPiece.Type == PieceType.King ? 5.0 : 2.0;
                    }
                }

                if (move.JumpedPositions.Count > 1)
                    captureValue += move.JumpedPositions.Count * 1.5;

                value += captureValue;
            }

            // Promotion value
            Piece movingPiece = board.GetPiece(move.From);
            if (movingPiece != null && movingPiece.Type != PieceType.King)
            {
                if ((color == PieceColor.Red && move.To.Row == 0) ||
                    (color == PieceColor.Black && move.To.Row == 7))
                {
                    value += 3.0;
                }
            }

            // Material balance
            Board afterMove = SimulateMove(board, move);
            value += EvaluateMaterialBalance(afterMove, color) * 0.5;

            // Mobility value
            value += EvaluateMobility(afterMove, color) * 0.3;

            // King safety
            if (movingPiece?.Type == PieceType.King)
            {
                if (IsPositionExposed(afterMove, move.To, color))
                    value -= 0.8;
            }

            // Tempo
            if (move.IsJump)
                value += 0.5;

            // Endgame adjustments
            int totalPieces = board.GetAllPieces(color).Count +
                            board.GetAllPieces(color == PieceColor.Red ? PieceColor.Black : PieceColor.Red).Count;

            if (totalPieces <= 6)
            {
                value += CalculateCenterControl(move.To) * 0.8;
            }

            return value;
        }

        private double EvaluateMaterialBalance(Board board, PieceColor color)
        {
            var ourPieces = board.GetAllPieces(color);
            var opponentColor = color == PieceColor.Red ? PieceColor.Black : PieceColor.Red;
            var theirPieces = board.GetAllPieces(opponentColor);

            double ourMaterial = 0;
            double theirMaterial = 0;

            foreach (var piece in ourPieces)
                ourMaterial += piece.Type == PieceType.King ? 3.0 : 1.0;

            foreach (var piece in theirPieces)
                theirMaterial += piece.Type == PieceType.King ? 3.0 : 1.0;

            return ourMaterial - theirMaterial;
        }

        private double EvaluateMobility(Board board, PieceColor color)
        {
            MoveValidator validator = new MoveValidator(board);
            var ourPieces = board.GetAllPieces(color);
            var opponentPieces = board.GetAllPieces(
                color == PieceColor.Red ? PieceColor.Black : PieceColor.Red);

            int ourMoves = 0;
            int theirMoves = 0;

            foreach (var piece in ourPieces)
                ourMoves += validator.GetValidMoves(piece).Count;

            foreach (var piece in opponentPieces)
                theirMoves += validator.GetValidMoves(piece).Count;

            return (ourMoves - theirMoves) * 0.1;
        }

        private bool CanPieceMove(Board board, Piece piece)
        {
            MoveValidator validator = new MoveValidator(board);
            return validator.GetValidMoves(piece).Count > 0;
        }

        private bool IsPieceProtected(Board board, Position pos, PieceColor color)
        {
            int[] directions = { -1, 1 };

            foreach (int rowDir in directions)
            {
                foreach (int colDir in directions)
                {
                    Position checkPos = new Position(pos.Row + rowDir, pos.Col + colDir);
                    if (board.IsValidPosition(checkPos))
                    {
                        Piece neighbor = board.GetPiece(checkPos);
                        if (neighbor != null && neighbor.Color == color)
                            return true;
                    }
                }
            }

            return false;
        }

        private bool IsPositionExposed(Board board, Position pos, PieceColor color)
        {
            PieceColor opponentColor = color == PieceColor.Red ? PieceColor.Black : PieceColor.Red;
            var opponentPieces = board.GetAllPieces(opponentColor);
            MoveValidator validator = new MoveValidator(board);

            foreach (var opponentPiece in opponentPieces)
            {
                var jumps = validator.GetValidJumps(opponentPiece);
                if (jumps.Any(jump => jump.JumpedPositions.Contains(pos)))
                    return true;
            }

            return false;
        }

        private double CalculateTacticalAdvantage(Board board, Move move, PieceColor color)
        {
            double value = 0.0;
            Board afterMove = SimulateMove(board, move);
            PieceColor opponentColor = color == PieceColor.Red ? PieceColor.Black : PieceColor.Red;

            var ourPieces = afterMove.GetAllPieces(color);
            MoveValidator validator = new MoveValidator(afterMove);

            foreach (var piece in ourPieces)
            {
                var jumps = validator.GetValidJumps(piece);
                value += jumps.Count * 0.5;
            }

            var theirPieces = afterMove.GetAllPieces(opponentColor);
            foreach (var piece in theirPieces)
            {
                var jumps = validator.GetValidJumps(piece);
                value -= jumps.Count * 0.3;
            }

            return value;
        }

        private Board SimulateMove(Board board, Move move)
        {
            Board simBoard = board.Clone();
            Piece movingPiece = simBoard.GetPiece(move.From);

            if (movingPiece != null)
            {
                simBoard.RemovePiece(move.From);

                if (move.IsJump && move.JumpedPositions != null)
                {
                    foreach (Position jumped in move.JumpedPositions)
                        simBoard.RemovePiece(jumped);
                }

                simBoard.SetPiece(move.To, movingPiece);

                if (movingPiece.Type != PieceType.King)
                {
                    if ((movingPiece.Color == PieceColor.Red && move.To.Row == 0) ||
                        (movingPiece.Color == PieceColor.Black && move.To.Row == 7))
                    {
                        movingPiece.PromoteToKing();
                    }
                }
            }

            return simBoard;
        }

        public void UpdateGameResult(GameResult result, int piecesRemaining, int opponentPiecesRemaining)
        {
            Stats.GamesPlayed++;

            switch (result)
            {
                case GameResult.Win:
                    Stats.Wins++;
                    Stats.PiecesCaptured += 12 - opponentPiecesRemaining;
                    break;
                case GameResult.Loss:
                    Stats.Losses++;
                    Stats.PiecesLost += 12 - piecesRemaining;
                    break;
                case GameResult.Draw:
                    Stats.Draws++;
                    break;
            }
        }

        public void CalculateFitness()
        {
            double fitness = 0.0;

            fitness += Stats.Wins * 100.0;
            fitness -= Stats.Losses * 50.0;
            fitness += Stats.Draws * 25.0;

            double captureRatio = Stats.TotalMoves > 0
                ? (double)Stats.PiecesCaptured / Stats.TotalMoves
                : 0.0;
            fitness += captureRatio * 50.0;

            double survivalRate = Stats.GamesPlayed > 0
                ? 1.0 - ((double)Stats.PiecesLost / (Stats.GamesPlayed * 12))
                : 0.0;
            fitness += survivalRate * 30.0;

            fitness += Stats.KingsMade * 15.0;
            fitness += Stats.KingsCaptured * 20.0;
            fitness -= Stats.KingsLost * 25.0;

            Brain.Fitness = Math.Max(0, fitness);
        }

        public AIPlayer Clone()
        {
            return new AIPlayer(Brain.Clone());
        }

        public void Mutate(double mutationRate)
        {
            Brain.Mutate(mutationRate);
        }

        public AIPlayer Crossover(AIPlayer partner, Random random)
        {
            DeepNeuralNetwork childBrain = Brain.Crossover(partner.Brain);
            return new AIPlayer(childBrain);
        }

        /// <summary>
        /// Backward compatibility method - not needed in improved version
        /// The improved AI doesn't use caching for better move variety
        /// </summary>
 
    }

    public class PlayerStats
    {
        public int GamesPlayed { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int TotalMoves { get; set; }
        public int PiecesCaptured { get; set; }
        public int PiecesLost { get; set; }
        public int KingsMade { get; set; }
        public int KingsCaptured { get; set; }
        public int KingsLost { get; set; }

        public double WinRate => GamesPlayed > 0 ? (double)Wins / GamesPlayed : 0.0;
    }
}