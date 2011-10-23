using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace NeuralNetworks1 {
    class Program {
        /// <summary>
        /// Ilosc argumentow wymagana przez program.
        /// </summary>
        private const int ARGUMNENTS_NUMBER = 3; 

        public static void Main(string[] args) {
            try {
                // Zeby .(kropki) wchodzily w Double.Parse
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

                if (args.Length != ARGUMNENTS_NUMBER) {
                    PrintUsage();
                }
                else {
                    GenerateOutput(args[0], args[1], args[2]);
                }

                Encog.EncogFramework.Instance.Shutdown();
            }
            catch (Exception error) {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + error.Message);
                Console.WriteLine("Details:");
                Console.WriteLine(error.ToString());
                Console.ForegroundColor = oldColor;
            }
        }

        private static void GenerateOutput(string configFile, string trainFile, string testFile) {
            var networkConfig = NetworkConfig.ReadFromFile(configFile);
                
            if (networkConfig.Type == NetworkType.Kohonen)
                BuildKohonenNetwork((KohonenNetwokConfig)networkConfig, trainFile, testFile);
            else if (networkConfig.Type == NetworkType.Perceptron)
                BuildPerceptronNetwork((PerceptronNetworkConfig)networkConfig, trainFile, testFile);
        }

        private static void BuildKohonenNetwork(KohonenNetwokConfig config, string trainFile, string testFile) {
            KohonenWrapper kohonen = new KohonenWrapper(config.Height, config.Width, config.InputSize);

            InputDataSet trainData = new InputDataSet(trainFile, config.InputSize);
            kohonen.Train(config.InitialLearningRate, config.LearningRateDecayFactor,
                config.InitialNeighbourhood, config.NeighbourhoodDecayFactor,
                config.Iterations, trainData);

            InputDataSet testData = new InputDataSet(testFile, config.InputSize);
            int[] classes = kohonen.Compute(testData);

            foreach (var i in classes) {
                Console.WriteLine(i);
            }
        }

        private static void BuildPerceptronNetwork(PerceptronNetworkConfig config, string trainFile, string testFile) {
            PerceptronWrapper perceptron = new PerceptronWrapper(
                config.InputSize, config.HiddenLayerSizes, config.OutputSize,
                config.UseBIAS, config.Unipolar
                );

            InputDataSet trainData = new InputDataSet(trainFile, config.InputSize, config.OutputSize);
            perceptron.Train(config.LearningRate, config.Iterations, config.Momentum, trainData);

            InputDataSet testData = new InputDataSet(testFile, config.InputSize);
            double[][] results = perceptron.Compute(testData);

            foreach (var outputs in results) {
                foreach (var output in outputs) {
                    Console.Write("{0:F3} ", output);
                }
                Console.WriteLine();
            }
        }


        private static void PrintUsage() {
            Console.WriteLine("Usage:");
            Console.WriteLine("{0} config_file train_file test_file", Assembly.GetExecutingAssembly().Location);
            Console.WriteLine("Network answers will be printed on standard output");
        }

    }
}
