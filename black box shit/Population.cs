using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace checkersclaude
{
    public class Population
    {
        public List<Player> Players { get; private set; }
        public Player BestPlayer { get; private set; }
        private Random random;
        private double MutationRate;

        public Population(int size, double mutationRate, Random random = null)
        {
            this.random = random ?? new Random();
            MutationRate = mutationRate;
            Players = new List<Player>();

            for (int i = 0; i < size; i++)
                Players.Add(new Player(this.random));
        }

        public void RunTournament(int gamesPerPair = 2)
        {
            int n = Players.Count;

            Parallel.For(0, n, i =>
            {
                for (int j = i + 1; j < n; j++)
                {
                    for (int g = 0; g < gamesPerPair; g++)
                    {
                        PlayGame(Players[i], Players[j]);
                    }
                }
            });

            foreach (var p in Players)
            {
                p.CalculateFitness();
            }

            BestPlayer = Players.OrderByDescending(p => p.Fitness).First();
        }

        private void PlayGame(Player p1, Player p2)
        {
            Board board = new Board();
            PieceColor turn = PieceColor.Red;

            int maxMoves = 200;
            int moveCount = 0;

            while (moveCount < maxMoves)
            {
                List<Piece> pieces = board.GetAllPieces(turn);
                List<Move> validMoves = new List<Move>();

                foreach (var piece in pieces)
                    validMoves.AddRange(piece.GetValidMoves(board));

                if (validMoves.Count == 0)
                {
                    // Current player loses
                    if (turn == PieceColor.Red) { p1.Losses++; p2.Wins++; }
                    else { p2.Losses++; p1.Wins++; }
                    return;
                }

                Move move = turn == PieceColor.Red ? p1.ChooseMove(board, validMoves, turn)
                                                   : p2.ChooseMove(board, validMoves, turn);

                board.ApplyMove(move);

                moveCount++;
                turn = turn == PieceColor.Red ? PieceColor.Black : PieceColor.Red;
            }

            // Draw
            p1.Draws++;
            p2.Draws++;
        }

        public void Evolve()
        {
            // Select top 50% players
            int survivorCount = Players.Count / 2;
            var sorted = Players.OrderByDescending(p => p.Fitness).ToList();
            var survivors = sorted.Take(survivorCount).ToList();

            List<Player> nextGen = new List<Player>();

            // Keep best player
            nextGen.Add(survivors[0].Clone());

            while (nextGen.Count < Players.Count)
            {
                var parent1 = survivors[random.Next(survivors.Count)];
                var parent2 = survivors[random.Next(survivors.Count)];
                var child = parent1.Crossover(parent2, random);
                child.Mutate(MutationRate);
                nextGen.Add(child);
            }

            Players = nextGen;
        }

        public string GetGenerationStats()
        {
            double avgFitness = Players.Average(p => p.Fitness);
            double bestFitness = Players.Max(p => p.Fitness);
            return $"AvgFitness={avgFitness:F2}, BestFitness={bestFitness:F2}";
        }
    }
}
