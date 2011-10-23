using System;
using Encog.ML.Train;
using Encog.ML.Train.Strategy;
using Encog.Neural.SOM.Training.Neighborhood;

namespace NeuralNetworks1
{
    /// <summary>
    /// Wprowadza zmianę współczynników nauki i sąsiedztwa podczas działania algorytmu uczącego sieć Kohenena.
    /// </summary>
    public class KohonenTrainStrategy : IStrategy
    {
        /// <summary>
        /// Algorytm uczący.
        /// </summary>
        private BasicTrainSOM basicTrainSOM;

        /// <summary>
        /// Współczynnik zmiany współczynnika nauki.
        /// </summary>
        private double learningChangeRate;

        /// <summary>
        /// Współczynnik zmiany współczynnika sąsiedztwa.
        /// </summary>
        private double neighbourhoodChangeRate;


        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="learningChangeRate">Współczynnik zmiany współczynnika nauki.</param>
        /// <param name="neighbourhoodChangeRate">Współczynnik zmiany współczynnika sąsiedztwa.</param>
        public KohonenTrainStrategy(double learningChangeRate, double neighbourhoodChangeRate)
        {
            this.learningChangeRate = learningChangeRate;
            this.neighbourhoodChangeRate = neighbourhoodChangeRate;
        }


        public void Init(IMLTrain train)
        {
            basicTrainSOM = train as BasicTrainSOM;
            if (basicTrainSOM == null)
            {
                throw new ArgumentException(
                    String.Format("Argument shoud be of {0} type.", typeof(BasicTrainSOM)), "train");
            }
        }

        public void PostIteration()
        {
            basicTrainSOM.LearningRate = basicTrainSOM.LearningRate * learningChangeRate;
            basicTrainSOM.Neighborhood.Radius = basicTrainSOM.Neighborhood.Radius*neighbourhoodChangeRate;
        }

        public void PreIteration()
        {
        }
    }
}
