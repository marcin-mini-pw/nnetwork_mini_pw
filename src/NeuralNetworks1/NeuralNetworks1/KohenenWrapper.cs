using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Kohonen = Encog.Neural.SOM.SOMNetwork;
using Encog.Neural.SOM.Training.Neighborhood;
using Encog.ML.Data.Basic;
using Encog.ML.Data;

namespace NeuralNetworks1 {
    class KohenenWrapper {
        Kohonen _network;

        /// <summary>
        /// Tworzy nowa siec Kohonena.
        /// </summary>
        /// <param name="neuronsCount"></param>
        /// <param name="inputSize"></param>
        public KohenenWrapper(int neuronsCount, int inputSize) {
            if (neuronsCount < 1 || inputSize < 1)
                throw new KohenenWrapperException("Invalid parameters");

            BuildKohonenNetwork(neuronsCount, inputSize);
        }

        private void BuildKohonenNetwork(int neuronsCount, int inputSize) {
            _network = new Kohonen(inputSize, neuronsCount);
            _network.Reset();
        }

        /// <summary>
        /// Uczy siec Kohonena z podanymi parametrami.
        /// </summary>
        /// <param name="learningRate">Stala uczenia</param>
        /// <param name="epochNumber">Na ilu przykladach przebiega nauka</param>
        /// <param name="neighborhoodRadius">Zasieg sasiedstwa</param>
        /// <param name="learningSet">Zbior danych uczacych</param>
        public void Train(double learningRate, int epochNumber, int neighborhoodRadius, InputDataSet learningSet) {
            BasicMLDataSet data = new BasicMLDataSet(learningSet.InputSet, null);
            INeighborhoodFunction func = new NeighborhoodBubble(neighborhoodRadius);
            BasicTrainSOM train = new BasicTrainSOM(_network, learningRate, data, func);
            train.Iteration(epochNumber);
        }

        /// <summary>
        /// Zwaraca odpowiedzi sieci dla
        /// danego zbioru testowego
        /// </summary>
        /// <param name="testSet"></param>
        /// <returns></returns>
        public int[] Compute(InputDataSet testSet) {
            if (testSet.InputDataSize != _network.InputCount)
                throw new PerceptronWrapperException("Invalid arguments");

            List<int> answers = new List<int>();
            foreach (double[] input in testSet.InputSet) {
                IMLData d = new BasicMLData(input);
                answers.Add(_network.Winner(d)); // Zwaca indeks neuronu zwyciescy
            }
            return answers.ToArray();
        }
    }
}
