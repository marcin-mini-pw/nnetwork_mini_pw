using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Engine.Network.Activation;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.Neural.Networks.Training.Propagation.Back;

namespace NeuralNetworks1 {
    /// <summary>
    /// Klasa stanowi wrapper wokol klas odpowiedzialnych
    /// za tworzenie perceptronu z Encog.
    /// </summary>
    class PerceptronWrapper {
        // Siec neuronowa z Encog
        private BasicNetwork _network;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputSize"></param>
        /// <param name="hiddenLayerSize"></param>
        /// <param name="hiddenLayersNumber"></param>
        /// <param name="outputSize"></param>
        public PerceptronWrapper(int inputSize, int hiddenLayerSize, int hiddenLayersNumber, int outputSize, bool useBias, bool unipolar) {
            if (inputSize < 1 || outputSize < 1)
                throw new PerceptronWrapperException("Invalid constructor arguments");
            if (hiddenLayersNumber < 0 || (hiddenLayersNumber > 0 && hiddenLayerSize < 1))
                throw new PerceptronWrapperException("Invalid constructor arguments");

            CreateFeedforwardNetwork(inputSize, hiddenLayerSize, hiddenLayersNumber, outputSize, useBias, unipolar);
        }

        private IActivationFunction GetActivationFunction(bool unipolar) {
            // Jako funkcje aktywacji uzywamy sigmoida [0,1] lub tanh[-1,1]
            if (unipolar)
                return new ActivationSigmoid();
            else
                return new ActivationTANH();
        }

        private void CreateFeedforwardNetwork(int inputSize, int hiddenLayerSize, int hiddenLayersNumber, int outputSize, bool useBias, bool unipolar) {
            // Budowa sieci
            _network = new BasicNetwork();

            _network.AddLayer(new BasicLayer(GetActivationFunction(unipolar), useBias, inputSize));
            for(int i = 0; i < hiddenLayersNumber; i++) {
                _network.AddLayer(new BasicLayer(GetActivationFunction(unipolar), useBias, hiddenLayerSize));
            }
            // No bias in output layer
            _network.AddLayer(new BasicLayer(GetActivationFunction(unipolar), false, outputSize));

            _network.Structure.FinalizeStructure();
            // Wylosuj wagi sieci z przedzialu [-1; 1]
            _network.Reset(); 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="learningRate">Stala uczenia (zwykle w okolicach 0.1)</param>
        /// <param name="epochNumber">Na ilu wybranych losowo przykladach uczyc siec</param>
        /// <param name="dataSet">Dane do nauki sieci</param>
        public void Train(double learningRate, int epochNumber, InputDataSet dataSet) {
            if (learningRate <= 0.0 || epochNumber <= 0)
                throw new PerceptronWrapperException("Invalid arguments");
            if (_network.InputCount != dataSet.InputDataSize ||
                _network.OutputCount != dataSet.OutputDataSize ||
                dataSet.InputDataCount <= 0)
                    throw new PerceptronWrapperException("Invalid data set size");

            IMLDataSet data = new BasicMLDataSet(dataSet.InputSet, dataSet.OutputSet);
            Backpropagation backprop = new Backpropagation(_network, data);
            backprop.LearningRate = learningRate;

            // Uczymy siec za pomoca backpropagation
            backprop.Iteration(epochNumber);
            // :) - Prosciej sie nie da
        }

        /// <summary>
        /// Na podstawie wag sieci zwaraca odpowiedzi dla zbioru testowego
        /// </summary>
        /// <param name="testSet"></param>
        /// <returns></returns>
        public double[][] Compute(InputDataSet testSet) {
            if (testSet.InputDataSize != _network.InputCount)
                throw new PerceptronWrapperException("Invalid arguments");

            List<double[]> answers = new List<double[]>();
            foreach(double[] input in testSet.InputSet) {
                double[] output = new double[_network.OutputCount];
                _network.Compute(input, output);
                answers.Add(output);
            }
            return answers.ToArray();
        }
    }
}
