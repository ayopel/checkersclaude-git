using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace checkersclaude
{
    public class NeuralNetwork
    {
        private double[][] weights1; // Input to hidden layer
        private double[][] weights2; // Hidden to output layer
        private double[] biases1;
        private double[] biases2;

        private int inputSize;
        private int hiddenSize;
        private int outputSize;

        private Random random;

        public NeuralNetwork(int inputSize, int hiddenSize, int outputSize, Random random = null)
        {
            this.inputSize = inputSize;
            this.hiddenSize = hiddenSize;
            this.outputSize = outputSize;
            this.random = random ?? new Random();

            InitializeWeights();
        }

        private void InitializeWeights()
        {
            weights1 = new double[inputSize][];
            for (int i = 0; i < inputSize; i++)
            {
                weights1[i] = new double[hiddenSize];
                for (int j = 0; j < hiddenSize; j++)
                    weights1[i][j] = (random.NextDouble() * 2 - 1) * 0.5;
            }

            weights2 = new double[hiddenSize][];
            for (int i = 0; i < hiddenSize; i++)
            {
                weights2[i] = new double[outputSize];
                for (int j = 0; j < outputSize; j++)
                    weights2[i][j] = (random.NextDouble() * 2 - 1) * 0.5;
            }

            biases1 = new double[hiddenSize];
            biases2 = new double[outputSize];
            for (int i = 0; i < hiddenSize; i++)
                biases1[i] = (random.NextDouble() * 2 - 1) * 0.1;
            for (int i = 0; i < outputSize; i++)
                biases2[i] = (random.NextDouble() * 2 - 1) * 0.1;
        }

        public double[] FeedForward(double[] inputs)
        {
            double[] hidden = new double[hiddenSize];
            for (int i = 0; i < hiddenSize; i++)
            {
                hidden[i] = biases1[i];
                for (int j = 0; j < inputSize; j++)
                    hidden[i] += inputs[j] * weights1[j][i];
                hidden[i] = ReLU(hidden[i]);
            }

            double[] outputs = new double[outputSize];
            for (int i = 0; i < outputSize; i++)
            {
                outputs[i] = biases2[i];
                for (int j = 0; j < hiddenSize; j++)
                    outputs[i] += hidden[j] * weights2[j][i];
            }

            return outputs;
        }

        private double ReLU(double x) => Math.Max(0, x);

        public NeuralNetwork Clone()
        {
            NeuralNetwork clone = new NeuralNetwork(inputSize, hiddenSize, outputSize, random);

            for (int i = 0; i < inputSize; i++)
                for (int j = 0; j < hiddenSize; j++)
                    clone.weights1[i][j] = weights1[i][j];

            for (int i = 0; i < hiddenSize; i++)
                for (int j = 0; j < outputSize; j++)
                    clone.weights2[i][j] = weights2[i][j];

            for (int i = 0; i < hiddenSize; i++)
                clone.biases1[i] = biases1[i];
            for (int i = 0; i < outputSize; i++)
                clone.biases2[i] = biases2[i];

            return clone;
        }

        public void Mutate(double mutationRate)
        {
            for (int i = 0; i < inputSize; i++)
                for (int j = 0; j < hiddenSize; j++)
                    if (random.NextDouble() < mutationRate)
                        weights1[i][j] += (random.NextDouble() * 2 - 1) * 0.3;

            for (int i = 0; i < hiddenSize; i++)
                for (int j = 0; j < outputSize; j++)
                    if (random.NextDouble() < mutationRate)
                        weights2[i][j] += (random.NextDouble() * 2 - 1) * 0.3;

            for (int i = 0; i < hiddenSize; i++)
                if (random.NextDouble() < mutationRate)
                    biases1[i] += (random.NextDouble() * 2 - 1) * 0.1;

            for (int i = 0; i < outputSize; i++)
                if (random.NextDouble() < mutationRate)
                    biases2[i] += (random.NextDouble() * 2 - 1) * 0.1;
        }

        public NeuralNetwork Crossover(NeuralNetwork partner)
        {
            NeuralNetwork child = new NeuralNetwork(inputSize, hiddenSize, outputSize, random);

            for (int i = 0; i < inputSize; i++)
                for (int j = 0; j < hiddenSize; j++)
                    child.weights1[i][j] = random.NextDouble() < 0.5 ? weights1[i][j] : partner.weights1[i][j];

            for (int i = 0; i < hiddenSize; i++)
                for (int j = 0; j < outputSize; j++)
                    child.weights2[i][j] = random.NextDouble() < 0.5 ? weights2[i][j] : partner.weights2[i][j];

            for (int i = 0; i < hiddenSize; i++)
                child.biases1[i] = random.NextDouble() < 0.5 ? biases1[i] : partner.biases1[i];

            for (int i = 0; i < outputSize; i++)
                child.biases2[i] = random.NextDouble() < 0.5 ? biases2[i] : partner.biases2[i];

            return child;
        }
        // Add this property to track fitness (score)
        public double Fitness { get; set; }

        // Add this method to reward for various achievements
        public void AddReward(double reward)
        {
            Fitness += reward;
        }

        // Optionally, reset fitness before each training cycle
        public void ResetFitness()
        {
            Fitness = 0;
        }


        // ===========================
        // SAVE / LOAD FUNCTIONALITY
        // ===========================
        public void SaveToFile(string filepath)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(filepath, FileMode.Create)))
            {
                writer.Write(inputSize);
                writer.Write(hiddenSize);
                writer.Write(outputSize);

                // Write weights1
                for (int i = 0; i < inputSize; i++)
                    for (int j = 0; j < hiddenSize; j++)
                        writer.Write(weights1[i][j]);

                // Write weights2
                for (int i = 0; i < hiddenSize; i++)
                    for (int j = 0; j < outputSize; j++)
                        writer.Write(weights2[i][j]);

                // Write biases
                for (int i = 0; i < hiddenSize; i++)
                    writer.Write(biases1[i]);
                for (int i = 0; i < outputSize; i++)
                    writer.Write(biases2[i]);
            }
        }

        public static NeuralNetwork LoadFromFile(string filepath)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(filepath, FileMode.Open)))
            {
                int inputSize = reader.ReadInt32();
                int hiddenSize = reader.ReadInt32();
                int outputSize = reader.ReadInt32();

                NeuralNetwork network = new NeuralNetwork(inputSize, hiddenSize, outputSize);

                // Read weights1
                for (int i = 0; i < inputSize; i++)
                    for (int j = 0; j < hiddenSize; j++)
                        network.weights1[i][j] = reader.ReadDouble();

                // Read weights2
                for (int i = 0; i < hiddenSize; i++)
                    for (int j = 0; j < outputSize; j++)
                        network.weights2[i][j] = reader.ReadDouble();

                // Read biases
                for (int i = 0; i < hiddenSize; i++)
                    network.biases1[i] = reader.ReadDouble();
                for (int i = 0; i < outputSize; i++)
                    network.biases2[i] = reader.ReadDouble();

                return network;
            }
        }
    }
}
