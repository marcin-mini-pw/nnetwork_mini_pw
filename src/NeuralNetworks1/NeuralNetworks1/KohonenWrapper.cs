using System.Collections.Generic;
using System.Linq;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.Train.Strategy;
using Encog.Neural.SOM.Training.Neighborhood;
using Kohonen = Encog.Neural.SOM.SOMNetwork;

namespace NeuralNetworks1
{
    class KohonenWrapper
    {
        public Kohonen network;

        /// <summary>
        /// Liczba neuronów w pionie.
        /// </summary>
        private readonly int rows;

        /// <summary>
        /// Liczba neuronów w poziomie.
        /// </summary>
        private readonly int columns;

        /// <summary>
        /// Tworzy nowa siec Kohonena.
        /// </summary>
        /// <param name="rows">Liczba neuronów w pionie.</param>
        /// <param name="columns">Liczba neuronów w poziomie.</param>
        /// <param name="inputSize">Liczba neuronów w poziomie.</param>
        public KohonenWrapper(int rows, int columns, int inputSize)
        {
            if (rows < 1 || columns < 1 || inputSize < 1)
                throw new KohonenWrapperException("Invalid parameters");

            this.rows = rows;
            this.columns = columns;
            BuildKohonenNetwork(rows*columns, inputSize);
        }

        /// <summary>
        /// Uczy siec Kohonena z podanymi parametrami.
        /// </summary>
        /// <param name="learningRate">Początkowy współczynnik nauki.</param>
        /// <param name="learningChangeRate">Współczynnik zmiany współczynnika nauki</param>
        /// <param name="neighbourhoodRate">Początkowy współczynnik sąsiedztwa.</param>
        /// <param name="neighbourhoodChangeRate">Współczynnik zmiany współczynnika sąsiedztwa.</param>
        /// <param name="trainIterations">Na ilu przykladach przebiega nauka.</param>
        /// <param name="learningSet">Zbiór danych uczących.</param>
        public void Train(double learningRate, double learningChangeRate,
            double neighbourhoodRate, double neighbourhoodChangeRate,
            int trainIterations, InputDataSet learningSet)
        {
            var basicMlDataSet = new BasicMLDataSet(learningSet.InputSet, null);
            INeighborhoodFunction neighborhoodFunc = 
                new KohonenNeighbourhoodFunction(neighbourhoodRate, rows, columns);
            var train = new BasicTrainSOM(network, learningRate, basicMlDataSet, neighborhoodFunc);
            IStrategy strategy = new KohonenTrainStrategy(learningChangeRate, neighbourhoodChangeRate);
            strategy.Init(train);
            train.Strategies.Add(strategy);

            train.Iteration(trainIterations);
            train.FinishTraining();
        }

        /// <summary>
        /// Zwaraca odpowiedzi sieci dla
        /// danego zbioru testowego
        /// </summary>
        /// <param name="testSet"></param>
        /// <returns></returns>
        public int[] Compute(InputDataSet testSet)
        {
            if (testSet.InputDataSize != network.InputCount)
                throw new PerceptronWrapperException("Invalid arguments");

            return testSet.InputSet
                        .Select(input => new BasicMLData(input))
                        .Select(d => network.Winner(d))
                        .ToArray();
        }


        private void BuildKohonenNetwork(int neuronsCount, int inputSize)
        {
            network = new Kohonen(inputSize, neuronsCount);
            network.Reset();
        }
    }
}