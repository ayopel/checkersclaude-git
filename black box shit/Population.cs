using checkersclaude;
using System;
using System.Collections.Generic;
using System.Linq;

namespace checkersclaude
{
    public class Population
    {
        public List<Player> Players { get; private set; }
        public int Generation { get; private set; }
        public int PopulationSize { get; private set; }
        public double MutationRate { get; set; }
        public Player BestPlayer { get; private set; }
        public double BestFitness { get; private set; }

        private Random random;

        public Population(int populationSize, double mutationRate = 0.1, int? seed = null)
        {
            PopulationSize = populationSize;
            MutationRate = mutationRate;
            Generation = 1;
            random = seed.HasValue ? new Random(seed.Value) : new Random();

            Players = new List<Player>();
            for (int i = 0; i < populationSize; i++)
            {
                Players.Add(new Player(random));
            }
        }

        public void EvaluatePopulation()
        {
            // Calculate fitness for each player
            foreach (Player player in Players)
            {
                player.CalculateFitness();
            }

            // Find best player
            BestPlayer = Players.OrderByDescending(p => p.Fitness).First();
            BestFitness = BestPlayer.Fitness;
        }

        public void RunTournament(int gamesPerPair = 2)
        {
            // Reset all player stats
            foreach (Player player in Players)
            {
                player.ResetStats();
            }

            // Round-robin tournament with each pair playing multiple games
            for (int i = 0; i < Players.Count; i++)
            {
                for (int j = i + 1; j < Players.Count; j++)
                {
                    for (int game = 0; game < gamesPerPair; game++)
                    {
                        // Alternate colors
                        Player red = game % 2 == 0 ? Players[i] : Players[j];
                        Player black = game % 2 == 0 ? Players[j] : Players[i];

                        PlayGame(red, black);
                    }
                }
            }

            EvaluatePopulation();
        }

        private void PlayGame(Player redPlayer, Player blackPlayer, int maxMoves = 200)
        {
            GameEngine game = new GameEngine();
            int moveCount = 0;

            while (game.State != GameState.RedWins &&
                   game.State != GameState.BlackWins &&
                   moveCount < maxMoves)
            {
                Player currentPlayer = game.State == GameState.RedTurn ? redPlayer : blackPlayer;
                PieceColor currentColor = game.State == GameState.RedTurn ? PieceColor.Red : PieceColor.Black;

                // Get all valid moves for current player
                List<Move> allValidMoves = GetAllValidMoves(game, currentColor);

                if (allValidMoves.Count == 0)
                    break;

                // Let AI choose a move
                Move chosenMove = currentPlayer.ChooseMove(game.Board, allValidMoves, currentColor);

                if (chosenMove == null)
                    break;

                // Execute the move
                game.SelectPiece(chosenMove.From);
                game.MovePiece(chosenMove.To);

                moveCount++;
            }

            // Update player stats
            if (game.State == GameState.RedWins)
            {
                redPlayer.Wins++;
                blackPlayer.Losses++;
                blackPlayer.PiecesLost = 12 - game.Board.GetAllPieces(PieceColor.Black).Count;
            }
            else if (game.State == GameState.BlackWins)
            {
                blackPlayer.Wins++;
                redPlayer.Losses++;
                redPlayer.PiecesLost = 12 - game.Board.GetAllPieces(PieceColor.Red).Count;
            }
            else
            {
                // Draw
                redPlayer.Draws++;
                blackPlayer.Draws++;
            }
        }

        private List<Move> GetAllValidMoves(GameEngine game, PieceColor color)
        {
            List<Move> allMoves = new List<Move>();
            List<Piece> pieces = game.Board.GetAllPieces(color);
            MoveValidator validator = new MoveValidator(game.Board);

            foreach (Piece piece in pieces)
            {
                List<Move> pieceMoves = validator.GetValidMoves(piece);
                allMoves.AddRange(pieceMoves);
            }

            return allMoves;
        }

        public void Evolve()
        {
            List<Player> newGeneration = new List<Player>();

            // Elitism: Keep top 10% of players
            int eliteCount = Math.Max(1, PopulationSize / 10);
            List<Player> sortedPlayers = Players.OrderByDescending(p => p.Fitness).ToList();

            for (int i = 0; i < eliteCount; i++)
            {
                newGeneration.Add(sortedPlayers[i].Clone());
            }

            // Fill rest with offspring
            while (newGeneration.Count < PopulationSize)
            {
                Player parent1 = SelectParent();
                Player parent2 = SelectParent();

                Player child = parent1.Crossover(parent2, random);
                child.Mutate(MutationRate);

                newGeneration.Add(child);
            }

            Players = newGeneration;
            Generation++;
        }

        private Player SelectParent()
        {
            // Tournament selection
            int tournamentSize = 5;
            Player best = Players[random.Next(Players.Count)];

            for (int i = 1; i < tournamentSize; i++)
            {
                Player contestant = Players[random.Next(Players.Count)];
                if (contestant.Fitness > best.Fitness)
                    best = contestant;
            }

            return best;
        }

        public string GetGenerationStats()
        {
            double avgFitness = Players.Average(p => p.Fitness);
            double avgWins = Players.Average(p => p.Wins);
            double avgMoves = Players.Average(p => p.TotalMoves);

            return $"Generation {Generation} | Best Fitness: {BestFitness:F2} | " +
                   $"Avg Fitness: {avgFitness:F2} | Avg Wins: {avgWins:F2} | Avg Moves: {avgMoves:F2}";
        }
    }
}