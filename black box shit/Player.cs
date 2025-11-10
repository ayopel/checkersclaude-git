using checkersclaude;
using System;
using System.Collections.Generic;
using System.Linq;

namespace checkersclaude
{
    public class Player
    {
        public NeuralNetwork Brain { get; private set; }
        public double Fitness { get; set; }

        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int TotalMoves { get; set; }
        public int PiecesCaptured { get; set; }
        public int kingscaptured {  get; set; }
        public int kingslost { get; set; }
        public int kingsmade { get; set; }
        public int PiecesLost { get; set; }
        public PieceColor Black { get; }


        private const int InputSize = 32;  // 32 playable squares (each can be: empty, red, black, red king, black king)
        private const int HiddenSize = 40;
        private const int OutputSize = 1;  // Single output for move evaluation

        public Player(Random random = null)
        {
            Brain = new NeuralNetwork(InputSize, HiddenSize, OutputSize, random);
            ResetStats();
        }
        

        public Player(NeuralNetwork brain)
        {
            Brain = brain;
            ResetStats();
        }
       

        public Player(NeuralNetwork brain, PieceColor black) : this(brain)
        {
            Black = black;
        }

        public void ResetStats()
        {
            Fitness = 0;
            Wins = 0;
            Losses = 0;
            Draws = 0;
            TotalMoves = 0;
            PiecesCaptured = 0;
            PiecesLost = 0;
            kingscaptured = 0;
            kingslost = 0;
            kingsmade = 0;
        }

        public Move ChooseMove(Board board, List<Move> validMoves, PieceColor color)
        {
            if (validMoves.Count == 0)
                return null;

            if (validMoves.Count == 1)
                return validMoves[0];

            // Evaluate each move
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

            TotalMoves++;
            if (bestMove.IsJump)
                PiecesCaptured++;

            return bestMove;
        }

        private double EvaluateMove(Board board, Move move, PieceColor color)
        {
            // Create a copy of the board state after the move
            double[] boardState = GetBoardStateAfterMove(board, move, color);

            // Get neural network evaluation
            double[] output = Brain.FeedForward(boardState);
            return output[0];
        }

        private double[] GetBoardStateAfterMove(Board board, Move move, PieceColor color)
        {
            double[] state = new double[InputSize];
            int index = 0;

            // Simulate the move
            Piece movingPiece = board.GetPiece(move.From);

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if ((row + col) % 2 == 0) continue; // Skip light squares

                    Position pos = new Position(row, col);
                    Piece piece = board.GetPiece(pos);

                    // Apply the hypothetical move
                    if (pos.Equals(move.From))
                        piece = null;
                    else if (pos.Equals(move.To))
                        piece = movingPiece;
                    else if (move.IsJump && move.JumpedPositions.Contains(pos))


                        // Encode piece state
                        if (piece == null)
                        state[index] = 0;
                    else if (piece.Color == color)
                        state[index] = piece.Type == PieceType.King ? 2.0 : 1.0;
                    else
                        state[index] = piece.Type == PieceType.King ? -2.0 : -1.0;

                    index++;
                }
            }

            return state;
        }

        public void CalculateFitness()
        {
            // Multi-factor fitness calculation
            double winBonus = Wins * 150;
            double lossePenalty = Losses * 50;
            double captureBonus = PiecesCaptured * 10;
            double capturekingbonus = kingscaptured * 30;
            double kingslostpelanlty = kingslost * 30;
            double kingsmadebonus = kingsmade * 20;
            double lossPenalty = PiecesLost * 10;

            double moveEfficiency = TotalMoves > 0 ? (PiecesCaptured * 100.0 / TotalMoves) : 0;

            Fitness = winBonus - lossePenalty + captureBonus - lossPenalty + moveEfficiency;
            Fitness = Math.Max(0, Fitness); // Ensure non-negative
        }

        public Player Clone()
        {
            Player clone = new Player(Brain.Clone());
            return clone;
        }

        public void Mutate(double mutationRate)
        {
            Brain.Mutate(mutationRate);
        }

        public Player Crossover(Player partner, Random random)
        {
            NeuralNetwork childBrain = Brain.Crossover(partner.Brain);
            return new Player(childBrain);
        }
    }
}