using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.IO;

namespace NeuralNetworks1 {
    /// <summary>
    /// Abstrakcyjna klasa bazowa opisujaca konfiguracje sieci.
    /// </summary>
    abstract class NetworkConfig {
        /// <summary>
        /// Typ sieci.
        /// </summary>
        public abstract NetworkType Type { get; }

        /// <summary>
        /// Metoda odczytuje konfiguracje sieci z pliku konfiguracji.
        /// </summary>
        /// <param name="configFilePath"></param>
        /// <returns></returns>
        public static NetworkConfig ReadFromFile(string configFilePath) {
            try {
                using (TextReader reader = new StreamReader(configFilePath)) {
                    string firstLine = reader.ReadLine()
                        .ToLower()
                        .Trim();

                    switch (firstLine) {
                        case "kohonen":
                            return ReadKohonenConfiguration(reader);

                        case "feedforward":
                            return ReadPerceptronConfiguration(reader);

                        default:
                            throw new NetworkConfigException("Invalid network type in config file.");
                    }
                }
            }
            catch (IOException error) {
                throw new NetworkConfigException("IO Error: " + error.Message);
            }
        }

        private static PerceptronNetworkConfig ReadPerceptronConfiguration(TextReader reader) {
            string secondLine = reader.ReadLine().Trim();
            bool useBias = secondLine.StartsWith("bias");
            bool unipolar = secondLine.EndsWith("unipolar");

            List<int> netSizes = null;
            try {
                var netSizesQuery = from s in reader
                                   .ReadLine()
                                   .Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                               select
                                    Int32.Parse(s);
                netSizes = netSizesQuery.ToList();

                if(netSizes.Count < 2)
                    throw new NetworkConfigException("Not enough network size parameters");
            }
            catch (Exception) {
                throw new NetworkConfigException("Invalid network sizes");
            }

            string[] lastLine = reader
                .ReadLine()
                .Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            try {
                int iterations = Int32.Parse(lastLine[0]);
                double learningRate = Double.Parse(lastLine[1]);
                double bias = Double.Parse(lastLine[2]);

                return new PerceptronNetworkConfig(
                    netSizes[0], netSizes[netSizes.Count - 1], netSizes.GetRange(1, netSizes.Count - 2),
                    iterations, learningRate, useBias, bias, unipolar
                    );
            }
            catch (Exception) {
                throw new NetworkConfigException("Invalid network parametrers");
            }
        }

        private static KohonenNetwokConfig ReadKohonenConfiguration(TextReader reader) {
            try {
                string[] firstLine = reader
                            .ReadLine()
                            .Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                int neurons = Int32.Parse(firstLine[0]);
                int widht = Int32.Parse(firstLine[1]);
                int height = Int32.Parse(firstLine[2]);

                string[] secondLine = reader
                            .ReadLine()
                            .Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                int iterations = Int32.Parse(secondLine[0]);
                double initNgh = Double.Parse(secondLine[1]);
                double nghDecay = Double.Parse(secondLine[2]);
                double initLrn = Double.Parse(secondLine[3]);
                double lrnDecay = Double.Parse(secondLine[4]);

                return new KohonenNetwokConfig(
                    neurons, widht, height, iterations,
                    initLrn, lrnDecay, initNgh, nghDecay
                    );
            }
            catch (Exception) {
                throw new NetworkConfigException("Invalid network parametrers");
            }
        }

        
    }

    /// <summary>
    /// Konfiguracja dla sieci Perceptron.
    /// </summary>
    class PerceptronNetworkConfig : NetworkConfig {
        private readonly List<int> _hiddenLayers; 

        public override NetworkType Type { 
            get { return NetworkType.Perceptron; } 
        }

        /// <summary>
        /// Liczba neuronow w warstwie wejsciowej.
        /// </summary>
        public int InputSize { get; private set; }

        /// <summary>
        /// Liczba neuonow w warstwie wyjsciowej.
        /// </summary>
        public int OutputSize { get; private set; }

        /// <summary>
        /// Rozmiary warstw ukrytych.
        /// </summary>
        public List<int> HiddenLayerSizes {
            get { return new List<int>(_hiddenLayers); }
        }

        /// <summary>
        /// Liczba iteracji.
        /// </summary>
        public int Iterations { get; private set; }

        /// <summary>
        /// Wspolczynnik nauki.
        /// </summary>
        public double LearningRate { get; private set; }

        /// <summary>
        /// Czy w sieci uzywac BIASu.
        /// </summary>
        public bool UseBIAS { get; private set; }

        /// <summary>
        /// Wartosc momentu.
        /// </summary>
        public double Momentum { get; private set; }

        /// <summary>
        /// Czy funkcja aktywacji ma byc unipolarna [-1; 0] czy
        /// bipolarna [-1; 1].
        /// </summary>
        public bool Unipolar { get; private set; }

        public PerceptronNetworkConfig(
            int inputSize, int outputSize, List<int> hiddenLayers,
            int iterations, double learningRate,
            bool useBias, double bias, bool unipolar) 
        {
            this.InputSize = inputSize;
            this.OutputSize = outputSize;
            this._hiddenLayers = new List<int>(hiddenLayers);

            this.Iterations = iterations;
            this.LearningRate = learningRate;

            this.UseBIAS = useBias;
            this.Momentum = bias;

            this.Unipolar = unipolar;
        }
    }

    /// <summary>
    /// Konfiguracja dla sieci Kohonena
    /// </summary>
    class KohonenNetwokConfig : NetworkConfig {
        public override NetworkType Type { 
            get { return NetworkType.Kohonen; } 
        }

        /// <summary>
        /// Ilosc wejsc dla sieci kohonena.
        /// </summary>
        public int InputSize { get; private set; }
        
        /// <summary>
        /// Ilosc neuronow w poziomie.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Ilosc neuronow w pionie.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Ilosc iteracji.
        /// </summary>
        public int Iterations { get; private set; }

        /// <summary>
        /// Poczatkowy rozmiar sasiedztwa.
        /// </summary>
        public double InitialNeighbourhood { get; private set; }

        /// <summary>
        /// Wspolczynnik zmiany rozmiaru sasiedztwa w czasie.
        /// </summary>
        public double NeighbourhoodDecayFactor { get; private set; }

        /// <summary>
        /// Poczatkowa wartosc wspolczynnika nauki.
        /// </summary>
        public double InitialLearningRate { get; private set; }

        /// <summary>
        /// Wspolczynnik zmiany wartosci wspolczynnika nauki.
        /// </summary>
        public double LearningRateDecayFactor { get; private set; }

        public KohonenNetwokConfig(
            int inputSize, int width, int height,
            int iterations, double learingRate, double learningRateDecay,
            double initailNeighbourhood, double neighbourhoodDecay) 
        {
            this.InputSize = inputSize;
            this.Width = width;
            this.Height = height;
            this.Iterations = iterations;

            this.InitialLearningRate = learingRate;
            this.LearningRateDecayFactor = learningRateDecay;

            this.InitialNeighbourhood = initailNeighbourhood;
            this.NeighbourhoodDecayFactor = neighbourhoodDecay;
        }
    }

    /// <summary>
    /// Typ sieci neuronowej
    /// </summary>
    enum NetworkType {
        Kohonen,
        Perceptron
    }
}
