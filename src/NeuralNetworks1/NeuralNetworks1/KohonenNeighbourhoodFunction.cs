using Encog.Neural.SOM.Training.Neighborhood;
using System;

namespace NeuralNetworks1
{
    public class KohonenNeighbourhoodFunction : INeighborhoodFunction
    {
        /// <summary>
        /// Liczba neuronów w pionie.
        /// </summary>
        private readonly int rows;

        /// <summary>
        /// Liczba neuronów w poziomie.
        /// </summary>
        private readonly int columns;


        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="neighbourHoodRate">Początkowa wartość współczynnika sąsiedztwa.</param>
        /// <param name="rows">Liczba neuronów w pionie.</param>
        /// <param name="columns">Liczba neuronów w poziomie.</param>
        public KohonenNeighbourhoodFunction(double neighbourHoodRate, int rows, int columns)
        {
            Radius = neighbourHoodRate;
            this.rows = rows;
            this.columns = columns;
        }

        private const double EPSILON = 0.0001f;

        public double Function(int currentNeuron, int bestNeuron)
        {
            int currentX = currentNeuron%columns;
            int currentY = currentNeuron/columns;
            int bestX = bestNeuron%columns;
            int bestY = bestNeuron/columns;

            double d = (currentX - bestX)*(currentX - bestX) + (currentY - bestY)*(currentY - bestY);
            if(d == 0.0)
            {
                return 1.0;
            }

            if (Radius < EPSILON) {
                return 0.0;
            }

            return Math.Max(1.0 - d / Radius, 0.0);
        }


        public double Radius { get; set; }
    }
}
