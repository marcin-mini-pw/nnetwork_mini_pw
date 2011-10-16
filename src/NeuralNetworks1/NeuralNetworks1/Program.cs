using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;
using System.IO;

namespace NeuralNetworks1 {
    class Program {
        static string AddSlashes(string str) {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str) {
                sb.Append(c);
                sb.Append('\\');
            }
            if (sb.Length > 0)
                sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        static char AskQuestion(string question, string answers) {
            answers = answers.ToUpper();
            string answersToShow = AddSlashes(answers);
            char userAnsqwer = '?';

            do {
                Console.Write("{0} [{1}]: ", question, answersToShow);
                string str = Console.ReadLine();
                if (str.Length > 0)
                    userAnsqwer = Char.ToUpper(str[0]);
            } while (!answers.Contains(userAnsqwer));

            return userAnsqwer;
        }

        static int AskForNumber(string question, Predicate<int> goodNumber) {
            int n = 0;
            bool isNumber = false;

            do {
                Console.Write("{0} [Liczba]: ", question);
                string str = Console.ReadLine();
                isNumber = Int32.TryParse(str, out n);
            } while (isNumber != true || goodNumber(n) == false);

            return n;
        }

        static string AskForFileName(string question) {
            string fileName = String.Empty;

            do {
                Console.Write("{0} [Podaj nazwę pliku]: ", question);
                fileName = Console.ReadLine().Trim();
                
            } while (File.Exists(fileName) == false);

            return fileName;
        }

        static void Main(string[] args) {
            try {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

                char answer = AskQuestion("Wybierz typ sieci (P - Perceptron, K - Kohonen)", "PK");
                if (answer == 'K') {
                    BuildKohonenNetwork();
                }
                else {
                    BuildPerceptronNetwork();
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
            Console.ReadLine();
        }

        private static void BuildPerceptronNetwork() {
            bool unipolar = 'U' == AskQuestion("U - Unipolarna, B - Bipolarna funkcja wzmocnienia", "UB");
            bool bias = 'B' == AskQuestion("B - Zastosuj bias, N - Brak biasu", "BN");
            int hiddenLayers = AskForNumber("Podaj liczbe warstw ukrytych", (x) => (x >= 0));
            int inputSize = AskForNumber("Podaj rozmiar WEJSCIA", (x) => (x > 0));
            int outputSize = AskForNumber("Podaj rozmiar WYJSCIA", (x) => (x > 0));

            string trainingFileName = AskForFileName("Podaj nazwę pliku ze zbiorem UCZACYM");
            string dataToProcessFileName = AskForFileName("Podaj nazwe pliku ze zbiorem TESTOWYM");

            InputDataSet trainingSet = new InputDataSet(trainingFileName, inputSize, outputSize);
            InputDataSet testSet = new InputDataSet(dataToProcessFileName, inputSize);

            BuildPerceptronNetwork(unipolar, bias, inputSize, hiddenLayers, outputSize, trainingSet, testSet);
        }

        private static void BuildPerceptronNetwork(bool unipolar, bool bias, int inputSize, int hiddenLayers, int outputSize, InputDataSet trainingSet, InputDataSet testSet) {
            var perceptron = new PerceptronWrapper(inputSize, inputSize, hiddenLayers, outputSize, bias, unipolar);
            perceptron.Train(0.2, 5000, trainingSet);
            double[][] answers = perceptron.Compute(testSet);

            foreach (double[] output in answers) {
                foreach (double o in output) {
                    Console.Write("{0:F3} ", o);
                }
                Console.WriteLine();
            }
        }

        private static void BuildKohonenNetwork() {
            int inputSize = AskForNumber("Podaj rozmiar WEJSCIA", (x) => (x > 0));
            int neuronCount = AskForNumber("Podaj ilosc neuronow", (x) => (x > 0));

            string trainingFileName = AskForFileName("Podaj nazwę pliku ze zbiorem UCZACYM");
            string dataToProcessFileName = AskForFileName("Podaj nazwe pliku ze zbiorem TESTOWYM");

            InputDataSet trainingSet = new InputDataSet(trainingFileName, inputSize);
            InputDataSet testSet = new InputDataSet(dataToProcessFileName, inputSize);

            BuildKohonenNetwork(inputSize, neuronCount, trainingSet, testSet);
        }

        private static void BuildKohonenNetwork(int inputSize, int neuronCount, InputDataSet trainingSet, InputDataSet testSet) {
            var kohonen = new KohenenWrapper(neuronCount, inputSize);
            kohonen.Train(0.05, 1000, 3, trainingSet);
            int[] answers = kohonen.Compute(testSet);

            foreach (var i in answers) {
                Console.WriteLine(i);
            }
        }
    }
}
