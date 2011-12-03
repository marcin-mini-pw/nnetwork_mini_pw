using System.Collections.Generic;
using System.Linq;
using Encog.Engine.Network.Activation;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Training.Propagation.Back;

namespace NeuralNetworks2.Logic
{
    /// <summary>
    /// Klasa stanowi wrapper wokol klas odpowiedzialnych za tworzenie perceptronu z Encog.
    /// </summary>
    internal class PerceptronWrapper
    {
        // Siec neuronowa z Encog
        private BasicNetwork basicNetwork;


        public PerceptronWrapper(int inputSize, ICollection<int> hiddenLayers, int outputSize,
            bool useBias, bool unipolar)
        {
            if (inputSize < 1 || outputSize < 1)
            {
                throw new PerceptronWrapperException("Invalid constructor arguments");
            }

            if (hiddenLayers.Any(x => x < 1) && hiddenLayers.Count > 0)
            {
                throw new PerceptronWrapperException("Invalid hidden layer size");
            }

            CreateFeedforwardNetwork(inputSize, hiddenLayers, outputSize, useBias, unipolar);
        }


        /// <summary>
        /// Uczy sieć.
        /// </summary>
        /// <param name="learningRate">Stala uczenia (zwykle w okolicach 0.1)</param>
        /// <param name="epochNumber">Na ilu wybranych losowo przykladach uczyc siec</param>
        /// <param name="momentum">Współczynnik bezwładności.</param>
        /// <param name="inputData">The input into the neural network for training.</param>
        /// <param name="idealData">The idea into the neural network for training.</param>
        public void Train(double learningRate, int epochNumber,
            double momentum, double[][] inputData, double[][] idealData)
        {
            if (learningRate <= 0.0 || epochNumber <= 0)
            {
                throw new PerceptronWrapperException("Invalid arguments");
            }

            if (inputData.Any(x => x.Length != basicNetwork.InputCount)
                || idealData.Any(x => x.Length != basicNetwork.OutputCount)
                || inputData.GetLength(0) <= 0)
            {
                throw new PerceptronWrapperException("Invalid data set size");
            }

            IMLDataSet data = new BasicMLDataSet(inputData, idealData);
            var backprop = new Backpropagation(basicNetwork, data, learningRate, momentum);

            // Uczymy siec za pomoca backpropagation
            backprop.Iteration(epochNumber);
            // :) - Prosciej sie nie da
        }

        /// <summary>
        /// Na podstawie wag sieci zwraca odpowiedzi dla zadanego zbioru testowego.
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public double[][] Compute(double[][] inputData)
        {
            if (inputData.Any(x => x.Length != basicNetwork.InputCount))
            {
                throw new PerceptronWrapperException("Invalid arguments");
            }

            var answers = new List<double[]>();
            foreach (double[] input in inputData)
            {
                var output = new double[basicNetwork.OutputCount];
                basicNetwork.Compute(input, output);
                answers.Add(output);
            }
            return answers.ToArray();
        }


        private static IActivationFunction GetActivationFunction(bool unipolar)
        {
            // Jako funkcje aktywacji uzywamy sigmoida [0,1] lub tanh[-1,1]
            return unipolar
                       ? new ActivationSigmoid() as IActivationFunction
                       : new ActivationTANH();
        }

        private void CreateFeedforwardNetwork(int inputSize, IEnumerable<int> hiddenLayers, int outputSize,
            bool useBias, bool unipolar)
        {
            // Budowa sieci
            basicNetwork = new BasicNetwork();

            basicNetwork.AddLayer(new BasicLayer(GetActivationFunction(unipolar), useBias, inputSize));
            foreach (var size in hiddenLayers)
            {
                basicNetwork.AddLayer(new BasicLayer(GetActivationFunction(unipolar), useBias, size));
            }
            // No bias in output layer
            basicNetwork.AddLayer(new BasicLayer(GetActivationFunction(unipolar), false, outputSize));

            basicNetwork.Structure.FinalizeStructure();
            // Wylosuj wagi sieci z przedzialu [-1; 1]
            basicNetwork.Reset();
        }
    }
}