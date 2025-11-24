// ============================================================================
// REPLACE YOUR ENTIRE AI/TrainingSystem.cs FILE WITH THIS CODE
// ============================================================================
// Instructions:
// 1. Open your AI/TrainingSystem.cs file
// 2. Select ALL content (Ctrl+A)
// 3. Delete it
// 4. Paste this entire file
// 5. Save
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace checkersclaude
{
    public class TrainingSystem
    {
        public List<AIPlayer> Population { get; private set; }
        public int Generation { get; private set; }
        public AIPlayer BestPlayer { get; private set; }
        public TrainingStats CurrentStats { get; private set; }

        private readonly int populationSize;
        private readonly double mutationRate;
        private readonly double elitePercentage;
        private readonly Random random;
        private readonly TrainingConfig config;

        private double historicalBestFitness = 0;
        private int generationsWithoutImprovement = 0;
        private const int DiversityResetThreshold = 20;

        public TrainingSystem(TrainingConfig config)
        {
            this.config = config;
            this.populationSize = config.PopulationSize;
            this.mutationRate = config.MutationRate;
            this.elitePercentage = config.ElitePercentage;
            this.random = config.Seed.HasValue ? new Random(config.Seed.Value) : new Random();

            Generation = 0;
            CurrentStats = new TrainingStats();
            InitializePopulation();
        }

        private void InitializePopulation()
        {
            Population = new List<AIPlayer>();
            for (int i = 0; i < populationSize; i++)
            {
                Population.Add(new AIPlayer(random));
            }
        }

        public void RunGeneration()
        {
            Generation++;

            foreach (var player in Population)
            {
                player.Stats.GamesPlayed = 0;
                player.Stats.Wins = 0;
                player.Stats.Losses = 0;
                player.Stats.Draws = 0;
            }

            RunImprovedTournament();

            foreach (var player in Population)
            {
                player.CalculateFitness();
            }

            BestPlayer = Population.OrderByDescending(p => p.Brain.Fitness).First();

            if (BestPlayer.Brain.Fitness > historicalBestFitness)
            {
                historicalBestFitness = BestPlayer.Brain.Fitness;
                generationsWithoutImprovement = 0;
            }
            else
            {
                generationsWithoutImprovement++;
            }

            UpdateStats();
            EvolvePopulation();

            if (generationsWithoutImprovement >= DiversityResetThreshold)
            {
                InjectDiversity();
                generationsWithoutImprovement = 0;
            }
        }

        private void RunImprovedTournament()
        {
            int gamesPerPair = config.GamesPerPair;
            var matchups = GenerateImprovedMatchups();

            if (config.UseParallelProcessing)
            {
                Parallel.ForEach(matchups, matchup =>
                {
                    for (int game = 0; game < gamesPerPair; game++)
                    {
                        AIPlayer red = game % 2 == 0 ? matchup.Item1 : matchup.Item2;
                        AIPlayer black = game % 2 == 0 ? matchup.Item2 : matchup.Item1;
                        PlayGame(red, black);
                    }
                });
            }
            else
            {
                foreach (var matchup in matchups)
                {
                    for (int game = 0; game < gamesPerPair; game++)
                    {
                        AIPlayer red = game % 2 == 0 ? matchup.Item1 : matchup.Item2;
                        AIPlayer black = game % 2 == 0 ? matchup.Item2 : matchup.Item1;
                        PlayGame(red, black);
                    }
                }
            }
        }

        private List<Tuple<AIPlayer, AIPlayer>> GenerateImprovedMatchups()
        {
            var matchups = new List<Tuple<AIPlayer, AIPlayer>>();

            var sortedPopulation = Generation > 1
                ? Population.OrderByDescending(p => p.Brain.Fitness).ToList()
                : Population.ToList();

            for (int i = 0; i < Population.Count; i++)
            {
                int opponentsPerPlayer = Math.Min(config.OpponentsPerPlayer, Population.Count - 1);

                for (int j = 0; j < opponentsPerPlayer; j++)
                {
                    int opponentIndex;

                    if (random.NextDouble() < 0.6 && Generation > 1)
                    {
                        int minRank = Math.Max(0, i - 3);
                        int maxRank = Math.Min(sortedPopulation.Count - 1, i + 3);
                        opponentIndex = random.Next(minRank, maxRank + 1);
                    }
                    else
                    {
                        opponentIndex = random.Next(Population.Count);
                    }

                    if (opponentIndex != i)
                    {
                        matchups.Add(new Tuple<AIPlayer, AIPlayer>(
                            Population[i],
                            sortedPopulation[opponentIndex]));
                    }
                }
            }

            return matchups;
        }

        private void PlayGame(AIPlayer redPlayer, AIPlayer blackPlayer)
        {
            GameEngine game = new GameEngine();
            int moveCount = 0;
            int maxMoves = config.MaxMovesPerGame;

            Dictionary<string, int> stateHistory = new Dictionary<string, int>();

            while (game.State != GameState.RedWins &&
                   game.State != GameState.BlackWins &&
                   moveCount < maxMoves)
            {
                AIPlayer currentPlayer = game.State == GameState.RedTurn ? redPlayer : blackPlayer;
                PieceColor currentColor = game.State == GameState.RedTurn ? PieceColor.Red : PieceColor.Black;

                List<Move> validMoves = GetAllValidMoves(game, currentColor);

                if (validMoves.Count == 0)
                    break;

                Move chosenMove = currentPlayer.ChooseMove(game.Board, validMoves, currentColor);

                if (chosenMove == null)
                    break;

                if (chosenMove.IsJump)
                {
                    currentPlayer.Stats.PiecesCaptured += chosenMove.JumpedPositions.Count;

                    foreach (var jumpedPos in chosenMove.JumpedPositions)
                    {
                        Piece captured = game.Board.GetPiece(jumpedPos);
                        if (captured?.Type == PieceType.King)
                        {
                            currentPlayer.Stats.KingsCaptured++;
                            AIPlayer opponent = currentPlayer == redPlayer ? blackPlayer : redPlayer;
                            opponent.Stats.KingsLost++;
                        }
                    }
                }

                game.SelectPiece(chosenMove.From);
                game.MovePiece(chosenMove.To);

                Piece movedPiece = game.Board.GetPiece(chosenMove.To);
                if (movedPiece != null && movedPiece.Type == PieceType.King)
                {
                    if (chosenMove.From.Row != chosenMove.To.Row)
                    {
                        int kingRow = currentColor == PieceColor.Red ? 0 : 7;
                        if (chosenMove.To.Row == kingRow)
                        {
                            currentPlayer.Stats.KingsMade++;
                        }
                    }
                }

                string boardState = game.Board.GetStateString();
                if (!stateHistory.ContainsKey(boardState))
                    stateHistory[boardState] = 0;
                stateHistory[boardState]++;

                if (stateHistory[boardState] >= 3)
                {
                    redPlayer.UpdateGameResult(GameResult.Draw,
                        game.Board.GetAllPieces(PieceColor.Red).Count,
                        game.Board.GetAllPieces(PieceColor.Black).Count);
                    blackPlayer.UpdateGameResult(GameResult.Draw,
                        game.Board.GetAllPieces(PieceColor.Black).Count,
                        game.Board.GetAllPieces(PieceColor.Red).Count);
                    return;
                }

                moveCount++;
            }

            int redPieces = game.Board.GetAllPieces(PieceColor.Red).Count;
            int blackPieces = game.Board.GetAllPieces(PieceColor.Black).Count;

            if (game.State == GameState.RedWins)
            {
                redPlayer.UpdateGameResult(GameResult.Win, redPieces, blackPieces);
                blackPlayer.UpdateGameResult(GameResult.Loss, blackPieces, redPieces);
            }
            else if (game.State == GameState.BlackWins)
            {
                blackPlayer.UpdateGameResult(GameResult.Win, blackPieces, redPieces);
                redPlayer.UpdateGameResult(GameResult.Loss, redPieces, blackPieces);
            }
            else
            {
                redPlayer.UpdateGameResult(GameResult.Draw, redPieces, blackPieces);
                blackPlayer.UpdateGameResult(GameResult.Draw, blackPieces, redPieces);
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

        private void EvolvePopulation()
        {
            List<AIPlayer> newGeneration = new List<AIPlayer>();

            var sortedPopulation = Population.OrderByDescending(p => p.Brain.Fitness).ToList();

            int eliteCount = Math.Max(2, (int)(populationSize * elitePercentage));
            for (int i = 0; i < eliteCount; i++)
            {
                newGeneration.Add(sortedPopulation[i].Clone());
            }

            while (newGeneration.Count < populationSize)
            {
                AIPlayer parent1 = TournamentSelection(sortedPopulation, 5);
                AIPlayer parent2 = TournamentSelection(sortedPopulation, 5);

                AIPlayer child = parent1.Crossover(parent2, random);

                double adaptiveMutationRate = CalculateAdaptiveMutationRate(parent1, parent2);
                child.Mutate(adaptiveMutationRate);

                newGeneration.Add(child);
            }

            Population = newGeneration;
        }

        private double CalculateAdaptiveMutationRate(AIPlayer parent1, AIPlayer parent2)
        {
            double rate = mutationRate;

            double avgParentFitness = (parent1.Brain.Fitness + parent2.Brain.Fitness) / 2.0;
            double bestFitness = BestPlayer?.Brain.Fitness ?? 1.0;

            if (bestFitness > 0)
            {
                double relativeWeakness = 1.0 - (avgParentFitness / bestFitness);
                rate += relativeWeakness * 0.15;
            }

            if (generationsWithoutImprovement > 5)
            {
                rate += 0.05 * (generationsWithoutImprovement / 5.0);
            }

            return Math.Min(rate, 0.5);
        }

        private AIPlayer TournamentSelection(List<AIPlayer> sortedPopulation, int tournamentSize)
        {
            tournamentSize = Math.Min(tournamentSize, sortedPopulation.Count);
            AIPlayer best = sortedPopulation[random.Next(sortedPopulation.Count)];

            for (int i = 1; i < tournamentSize; i++)
            {
                AIPlayer contestant = sortedPopulation[random.Next(sortedPopulation.Count)];
                if (contestant.Brain.Fitness > best.Brain.Fitness)
                    best = contestant;
            }

            return best;
        }

        private void InjectDiversity()
        {
            int replaceCount = (int)(populationSize * 0.2);
            var sortedPopulation = Population.OrderBy(p => p.Brain.Fitness).ToList();

            for (int i = 0; i < replaceCount; i++)
            {
                sortedPopulation[i] = new AIPlayer(random);
            }

            int mutateStart = replaceCount;
            int mutateEnd = mutateStart + replaceCount;

            for (int i = mutateStart; i < mutateEnd && i < sortedPopulation.Count; i++)
            {
                sortedPopulation[i].Mutate(0.3);
            }
        }

        private void UpdateStats()
        {
            CurrentStats.Generation = Generation;
            CurrentStats.BestFitness = BestPlayer.Brain.Fitness;
            CurrentStats.AverageFitness = Population.Average(p => p.Brain.Fitness);
            CurrentStats.BestWinRate = BestPlayer.Stats.WinRate;
            CurrentStats.AverageWinRate = Population.Average(p => p.Stats.WinRate);
            CurrentStats.BestGamesPlayed = BestPlayer.Stats.GamesPlayed;
        }

        public string GetGenerationReport()
        {
            string stagnationWarning = generationsWithoutImprovement > 10
                ? $" ⚠ Stagnation: {generationsWithoutImprovement} gen"
                : "";

            return $"Generation {CurrentStats.Generation} | " +
                   $"Best Fitness: {CurrentStats.BestFitness:F2} | " +
                   $"Avg Fitness: {CurrentStats.AverageFitness:F2} | " +
                   $"Best Win Rate: {CurrentStats.BestWinRate:P1} | " +
                   $"Avg Win Rate: {CurrentStats.AverageWinRate:P1}" +
                   stagnationWarning;
        }
    }

    public class TrainingConfig
    {
        public int PopulationSize { get; set; } = 50;
        public double MutationRate { get; set; } = 0.1;
        public double ElitePercentage { get; set; } = 0.1;
        public int GamesPerPair { get; set; } = 2;
        public int OpponentsPerPlayer { get; set; } = 5;
        public int MaxMovesPerGame { get; set; } = 200;
        public bool UseParallelProcessing { get; set; } = true;
        public int? Seed { get; set; } = null;
    }

    public class TrainingStats
    {
        public int Generation { get; set; }
        public double BestFitness { get; set; }
        public double AverageFitness { get; set; }
        public double BestWinRate { get; set; }
        public double AverageWinRate { get; set; }
        public int BestGamesPlayed { get; set; }
    }
}