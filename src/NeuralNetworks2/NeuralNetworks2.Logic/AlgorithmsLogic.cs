using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NeuralNetworks2.API.Exceptions;
using NeuralNetworks2.API.Logic;
using NeuralNetworks2.API.Model;
using System.Globalization;

namespace NeuralNetworks2.Logic {
    public class AlgorithmsLogic : IAlgorithmsLogic {
        /// <summary>
        /// Liczba wyjść pojedynczej sieci neuronowej.
        /// </summary>
        private const int NeuralNetworkOutputSize = 1;

        /// <summary>
        /// Czy sieci neuronowe powinny używać biasu.
        /// </summary>
        private const bool UseBias = true;

        /// <summary>
        /// Czy należy używać unipolarnej funkcji aktywacji?
        /// True oznacza, że tak; false spowoduje użycie funkcji bipolarnej.
        /// </summary>
        private const bool UseUniPolarActivationFunction = true;

        /// <summary>
        /// Maksymalny błąd dopuszczalny dla zbioru testowego.
        /// </summary>
        private const double TestSetMaxError = 0.1d;

        /// <summary>
        /// Maksymalna liczba iteracji uczenia pojedynczej sieci.
        /// </summary>
        private const int MaxIterationCounts = 100000;

        /// <summary>
        /// Ile razy mniej (w stosunku do liczby wejść) ma być w kolejnych warstwach sieci neuronowych?
        /// </summary> // 8 16 domyslny
        private static readonly double[] HiddenLayerNeuronsCountCoefs = new double[] { 4d, 8d };

        /// <summary>
        /// Służy do nagrywania głosów z mikrofonu.
        /// </summary>
        private readonly AudioRecorder audioRecorder = new AudioRecorder();

        /// <summary>
        /// Dla generowania wszelkich potrzebnych losowych wartości.
        /// </summary>
        private static readonly Random Random = new Random();


        /// <summary>
        /// Parametry algorytmu przekazane przy inicjalizacji.
        /// </summary>
        private AlgorithmParams algorithmParams;

        /// <summary>
        /// Sieci neuronowe zbudowane dla poszczególnych rozpoznawanych osób.
        /// </summary>
        private Dictionary<Person, PerceptronWrapper> neuralNetworks;

        /// <summary>
        /// Wszystkie rozpoznawane przez sieci osoby.
        /// </summary>
        private IList<Person> people;


        /// <summary>
        /// Czy logika została zainicjowana? W praktyce oznacza to, czy zostały już utworzone sieci neuronowe.
        /// </summary>
        private bool WasInitialized {
            get {
                return neuralNetworks != null;
            }
        }

        /// <summary>
        /// Czy sieci były już uczone.
        /// </summary>
        private bool WasTrained { get; set; }

        /// <summary>
        /// Czy trwa nagrywanie dźwięków z mikrofonu.
        /// </summary>
        private bool WasAudioRecordingStarted { get; set; }


        /// <summary>
        /// Inicjalizuje logikę (w praktyce: buduje sieci neuronowe).
        /// </summary>
        /// <param name="peopleToBeRecognized">Osoby, których rozpoznawania będziemy uczyć sieć.</param>
        /// <param name="algParams">Parametry algorytmu.</param>
        public void Init(IList<Person> peopleToBeRecognized, AlgorithmParams algParams) {
            if (peopleToBeRecognized == null) {
                throw new ArgumentNullException("peopleToBeRecognized");
            }

            if (!peopleToBeRecognized.Any()) {
                throw new ArgumentOutOfRangeException("peopleToBeRecognized", "You should assign at least one person.");
            }

            if (algParams == null) {
                throw new ArgumentNullException("algParams");
            }

            if (WasInitialized) {
                throw new InvalidOperationException("AlgorithmsLogic was already initialized!");
            }

            people = peopleToBeRecognized;
            algorithmParams = algParams;
            CreateNeuralNetworks();
        }

        /// <summary>
        /// Resetule logikę (usuwa zbudowane sieci neuronowe itp.). Potem jest potrzebna nowa inicjalizacja.
        /// </summary>
        public void Reset() {
            if (WasAudioRecordingStarted) {
                audioRecorder.StopRecording();
                WasAudioRecordingStarted = false;
            }

            WasTrained = false;

            neuralNetworks = null;
            people = null;
            algorithmParams = null;
        }

        /// <summary>
        /// Uczy wszystkie zbudowane sieci rozpoznawania przypisanych do nich osób.
        /// </summary>
        /// <returns>Słownik [Osoba, błąd sieci odpowiadającej tej osobie na zbiorze testowym].</returns>
        public IDictionary<Person, double> Train() {
            if (!WasInitialized) {
                throw new InvalidOperationException("You have to initialize logic first!");
            }

            var result = new Dictionary<Person, double>();

            Dictionary<Person, List<double[]>[]> trainingMfccs = GetAllPeopleTrainingDataMfccs();
            Dictionary<Person, List<double[]>[]> testMfccs = GetAllPeopleTestDataMfccs();

            double[] input = new double[GetNetworksInputSize()];
            double[][] answer = new double[][] { new double[1] };

            foreach (Person person in people) {
                Debug.WriteLine("Train person: {0}", person.FirstName);

                PerceptronWrapper network = neuralNetworks[person];
                TrainPesonNetwork2(trainingMfccs, testMfccs, input, answer, person, network);

                // Przetestuj nauczona siec
                result.Add(person, Test(person, testMfccs, input));
            }

            WasTrained = true;
            return result;
        }

        private void TrainPesonNetwork2(
            Dictionary<Person, List<double[]>[]> trainingMfccs,
            Dictionary<Person, List<double[]>[]> testMfccs,
            double[] input, double[][] answer, Person person,
            PerceptronWrapper network) {

            string plotFileName = @"d:/tmp/person_" + person.FirstName + "_" + person.SurName;
           // sw = new StreamWriter(plotFileName + ".txt");
            // dla kazdego pliku uczacego lista mfcc dla kolejnych ramek glosu
            List<double[]>[] personMfccs = trainingMfccs[person];

            int iterations = 0;

            while (iterations < MaxIterationCounts) {
                double expectedAnswer = 1.0d;

                if (Random.NextDouble() < algorithmParams.TCoef) {
                    expectedAnswer = 0.0d;
                    Person otherPerson = SelectOtherPerson(person);
                    personMfccs = trainingMfccs[otherPerson];
                }
                else {
                    personMfccs = trainingMfccs[person];
                }

                // Losujemy ramki na ktorych bedziemy uczyc siec, ramki musza wystepowac po sobie
                // bez przerws
                DrawInputData(input, personMfccs);

               double newLearningRate = algorithmParams.LearningRate / Math.Log10(10.0 + iterations);
               double newMomentum = algorithmParams.LearningRate / Math.Log10(10.0 + iterations);

                answer[0][0] = expectedAnswer;
                network.Train(newLearningRate, 1, newMomentum,
                    new[] { input },
                    answer);

                iterations++;

                if (iterations % 1000 == 0) {
                    var line = String.Format(CultureInfo.InvariantCulture, "{0}\t{1}", iterations, HardTest(person, testMfccs, input));
                //    sw.WriteLine(line);
                    Debug.WriteLine(line);
                }
            }

            //sw.Close();

           // ProcessStartInfo psi = new ProcessStartInfo(@"D:\TOOLS\gnuplot\binary\gnuplot.exe", String.Format(@"-e ""set terminal png;set output '{0}.png';plot '{0}.txt' with lines""", plotFileName));
           // psi.UseShellExecute = true;
           // Process.Start(psi);
        }

        private void TrainPesonNetwork(
            Dictionary<Person, List<double[]>[]> trainingMfccs, 
            double[] input, double[][] answer, Person person, 
            PerceptronWrapper network) {

            // dla kazdego pliku uczacego lista mfcc dla kolejnych ramek glosu
            List<double[]>[] personMfccs = trainingMfccs[person];

            int iterations  = 0;

            while (iterations < MaxIterationCounts) {
                double expectedAnswer = 1.0d;
                
                if (Random.NextDouble() < algorithmParams.TCoef) {
                    expectedAnswer = 0.0d;
                    Person otherPerson = SelectOtherPerson(person);
                    personMfccs = trainingMfccs[otherPerson];
                }
                else {
                    personMfccs = trainingMfccs[person];
                }

                // Losujemy ramki na ktorych bedziemy uczyc siec, ramki musza wystepowac po sobie
                // bez przerws
                DrawInputData(input, personMfccs);

                answer[0][0] = expectedAnswer;
                network.Train(algorithmParams.LearningRate, 1, algorithmParams.Momentum,
                    new[] { input },
                    answer);

                iterations++;
            }
        }

        private void DrawInputData(double[] input, List<double[]>[] personMfccs) {
            // Wylosuj numer pliku z ktorego pobierzemy dane
            int file = Random.Next(personMfccs.Length);
            // Wylosuj poczatek ciagu ramek
            int start = Random.Next(personMfccs[file].Count - algorithmParams.SignalFramesCount);
            // Kopiuj sygnal do tablicy input
            for (int k = 0; k < algorithmParams.SignalFramesCount; k++) {
                double[] frameMfcc = personMfccs[file][start + k];
                for (int l = 0; l < algorithmParams.MfccCount; l++) {
                    input[l + k * algorithmParams.MfccCount] = frameMfcc[l];
                }
            }
        }

        private Person SelectOtherPerson(Person person) {
            int otherPersonInd = Random.Next(neuralNetworks.Count);
            Person otherPerson = people[otherPersonInd];
            if (otherPerson == person) {
                otherPerson = people[(otherPersonInd + 1) % neuralNetworks.Count];
            }
            return otherPerson;
        }

        /// <summary>
        /// Rozpoczyna nagrywanie dźwięku z mikrofonu.
        /// </summary>
        public void StartRecordingVoice() {
            if (!WasInitialized) {
                throw new InvalidOperationException("You have to initialize logic first!");
            }

            if (!WasTrained) {
                throw new InvalidOperationException("You have to train neural networks first!");
            }

            audioRecorder.StartRecording();
            WasAudioRecordingStarted = true;
        }

        /// <summary>
        /// Kończy nagrywanie dźwięku z mikrofonu.
        /// </summary>
        /// <returns>Rezultaty zwrócone przez sieci odpowiadające poszczególnym osobom.
        /// Posortowane od najlepszych do najsłabszych wyników.</returns>
        public IList<Tuple<Person, double>> StopRecordingAndGetResults() {
            if (!WasAudioRecordingStarted) {
                throw new InvalidOperationException("You have to start recording first!");
            }

            var stream = audioRecorder.StopRecording();
            List<double[]> mfccs = GetMfccsFromStream(stream);
            var results = GetResults(mfccs);// GetResults_AllTheresholdStrategy(mfccs);

            results.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            WasAudioRecordingStarted = false;
            return results;
        }

        private List<Tuple<Person, double>> GetResults(List<double[]> mfccs) {
            var results = new List<Tuple<Person, double>>();

            const int DRAW_NUMBER = 10;
            double[] input = new double[GetNetworksInputSize()];

            foreach (Person person in people) {
                PerceptronWrapper network = neuralNetworks[person];

                double networkResults = 0.0f;
                foreach (int i in Enumerable.Range(0, DRAW_NUMBER)) {
                    DrawInputData(input, new[] { mfccs });
                    networkResults += network.Compute(new[] { input })[0][0];
                }
                // Obliczamy srednio z DRAW_NUMBER prob rozpoznania glosu
                networkResults /= DRAW_NUMBER;
                results.Add(new Tuple<Person, double>(person, networkResults));
            }
            return results;
        }

        const double THRESEHOLD = 0.65;
        private List<Tuple<Person, double>> GetResults_AllTheresholdStrategy(List<double[]> mfccs) {
            
            var results = new List<Tuple<Person, double>>();
            var mfccArray = mfccs.ToArray();

            double[] input = new double[GetNetworksInputSize()];

            Dictionary<Person, double> resultsPerNetwork = new Dictionary<Person, double>();
            foreach (var person in people)
                resultsPerNetwork[person] = 0.0f;

            for (int start = 0; start < mfccArray.Length - algorithmParams.SignalFramesCount; start++) {
                for (int i = 0; i < algorithmParams.SignalFramesCount; i++) {
                    Array.Copy(mfccArray[start + i], 0, input, algorithmParams.MfccCount * i, algorithmParams.MfccCount);
                }

                var result = from p in people
                             let network = neuralNetworks[p]
                             select new KeyValuePair<Person, double>(p, network.Compute(new[] { input })[0][0]);

                foreach (var r in result) {
                    if (r.Value > THRESEHOLD) {
                        resultsPerNetwork[r.Key] += r.Value;
                    }
                }
            }

            double sum = resultsPerNetwork.Sum(x => x.Value);
            if (Math.Abs(sum) < 0.001)
                return results; // Empty list

            var normResults = from r in resultsPerNetwork
                              select new Tuple<Person, double>(r.Key, r.Value / sum);

            return normResults.ToList();
        }

        private List<Tuple<Person, double>> GetResults_AllMaxStrategy(List<double[]> mfccs) {
            var results = new List<Tuple<Person, double>>();
            var mfccArray = mfccs.ToArray();

            double[] input = new double[GetNetworksInputSize()];

            Dictionary<Person, double> resultsPerNetwork = new Dictionary<Person, double>();
            foreach (var person in people)
                resultsPerNetwork[person] = 0.0f;

            for (int start = 0; start < mfccArray.Length - algorithmParams.SignalFramesCount; start++) {
                for (int i = 0; i < algorithmParams.SignalFramesCount; i++) {
                    Array.Copy(mfccArray[start + i], 0, input, algorithmParams.MfccCount * i, algorithmParams.MfccCount);
                }

                var result = from p in people
                             let network = neuralNetworks[p]
                             select new KeyValuePair<Person, double>(p, network.Compute(new[] { input })[0][0]);

                var max = result.OrderByDescending(x => x.Value).First();
                resultsPerNetwork[max.Key] += max.Value;
            }

            double sum = resultsPerNetwork.Sum(x => x.Value);
            if (Math.Abs(sum) < 0.001)
                return results; // Empty list

            var normResults = from r in resultsPerNetwork
                              select new Tuple<Person, double>(r.Key, r.Value / sum);

            return normResults.ToList();
        }


        private void CreateNeuralNetworks() {
            neuralNetworks = new Dictionary<Person, PerceptronWrapper>();

            int inputSize = GetNetworksInputSize();
            ICollection<int> hiddenLayers = GetLayersCount(inputSize);
            foreach (var person in people) {
                var perceptron = new PerceptronWrapper(inputSize, hiddenLayers, NeuralNetworkOutputSize, UseBias,
                                                       UseUniPolarActivationFunction);
                neuralNetworks.Add(person, perceptron);
            }
        }

        private int GetNetworksInputSize() {
            return algorithmParams.MfccCount * algorithmParams.SignalFramesCount;
        }

        private int[] GetLayersCount(int inputSize) {
            var inputSizeD = (double)inputSize;
            var layers = new int[HiddenLayerNeuronsCountCoefs.Length];

            for (int i = 0; i < layers.Length; ++i) {
                layers[i] = Convert.ToInt32(inputSizeD / HiddenLayerNeuronsCountCoefs[i]);
            }

            return layers;
        }

        private Dictionary<Person, List<double[]>[]> GetAllPeopleTrainingDataMfccs() {
            return GetAllPeopleMfccsHelper(p => p.TrainWavesPaths);
        }

        private Dictionary<Person, List<double[]>[]> GetAllPeopleTestDataMfccs() {
            return GetAllPeopleMfccsHelper(p => p.TestWavesPaths);
        }

        private Dictionary<Person, List<double[]>[]> GetAllPeopleMfccsHelper(
            Func<Person, ObservableCollection<string>> getWavesPaths) {
            var mfccs = new Dictionary<Person, List<double[]>[]>();
            foreach (Person person in people) {
                ObservableCollection<string> wavesPaths = getWavesPaths(person);
                var personMfccs = new List<double[]>[wavesPaths.Count];
                for (int i = 0; i < wavesPaths.Count; ++i) {
                    personMfccs[i] = GetMfccsFromFile(wavesPaths[i]);
                }

                

                mfccs.Add(person, personMfccs);
            }

            return mfccs;
        }

        private List<double[]> GetMfccsFromFile(string path) {
            LightWaveFileReader wave = null;
            try {
                wave = new LightWaveFileReader(path);
            }
            catch (Exception ex) {
                throw new WaveFileException(path, ex.Message, ex);
            }

            return GetMfccsHelper(wave);
        }

        private List<double[]> GetMfccsFromStream(Stream stream) {
            var wave = new LightWaveFileReader(stream);
            return GetMfccsHelper(wave);
        }

        #region MFCC Stuff


        private const int FREQUENCY = 44100;
        const int WINDOW_SIZE = 1024;
        const int OVERLAP = 512;
        const int FILTERS_NUMBER = 27;
        private static double[][] filters =
            TriFilterBank.CreateFiltersBank(FILTERS_NUMBER, WINDOW_SIZE, FREQUENCY, 0, 8000);

        const double SPEAK_POWER_THRESHOLD = 0.05;//0.022; // Ustalone empirycznie

        /// <summary>
        /// Zwaraca lite wsp. mfcc dla kolejnych ramek sgnalu mowy
        /// </summary>
        /// <param name="wave"></param>
        /// <returns></returns>
        private List<double[]> GetMfccsHelper(LightWaveFileReader wave) {
            if (wave.Frequency != FREQUENCY)
                throw new ArgumentException("Only 44100Hz waves are supported");

            const int WIN_DELTA = WINDOW_SIZE - OVERLAP;

            var results = new List<double[]>();
            var windowData = new double[WINDOW_SIZE];
            double[] tmp = new double[algorithmParams.MfccCount];

            double aproxSpeakThreshold = SPEAK_POWER_THRESHOLD;

            //FileStream fs = new FileStream("d:/tmp/tracks.txt", FileMode.Append);
            //StreamWriter sw = new StreamWriter(fs);

            //wave.NormalizeWaveSamples();
           
            for (int i = 0; i < wave.SamplesCount - WINDOW_SIZE; i += WIN_DELTA) {
                double power = 0.0f;
                for (int j = 0; j < WINDOW_SIZE; j++) {
                    windowData[j] = wave.SoundSamples[i + j];
                    power += Math.Abs(windowData[j]);
                }
                power /= WINDOW_SIZE;

                if (power > aproxSpeakThreshold) {
                    //Debug.WriteLine("power: {0}", power);
                    double signalPower;
                    var mfcc = MFCCCoefficients.GetMFCC(FREQUENCY, windowData, filters, algorithmParams.MfccCount, out signalPower);
                
                    //sw.Write('X');
                    double sum = 0.0f;
                    for (int k = 0; k < algorithmParams.MfccCount; ++k) {
                        tmp[k] = mfcc[k];
                        sum += tmp[k] * tmp[k];
                    }
                    // Normalize data
                    //if (sum > 0.0001) {
                    //    sum = Math.Sqrt(sum);
                    //    for (int k = 0; k < algorithmParams.MfccCount; ++k) {
                    //        tmp[k] /= sum;
                    //    }
                    //}

                    results.Add(tmp);
                    tmp = new double[algorithmParams.MfccCount];
                }
                //else sw.Write('_');
            }

            //sw.WriteLine();
            //sw.Close();

            if (results.Count < algorithmParams.SignalFramesCount) {
                throw new ArgumentException("Too little usable voice samples found in wave file");
            }

            Debug.WriteLine("Z pliku wyodrebiono {0} ramek", results.Count);
            return results;
        }

        #endregion

        /// <summary>
        /// Wykonuje testy dla danej osoby na zbiorach testowych.
        /// </summary>
        /// <param name="person">Osoba, której sieć chcemy przetestować.</param>
        /// <param name="testSetMfccs">Współczynniki MFCC dla wszystkich próbek testowych dla wszystkich osób.</param>
        /// <returns>Błąd sieci odpowiadającej zadanej osobie na zbiorze testowym.</returns>
        private double Test(Person testedPerson, Dictionary<Person, List<double[]>[]> testSet, double[] input) {
            Debug.Assert(NeuralNetworkOutputSize == 1);

            const int TEST_COUNT = 50; // Ile razy losujemy probki do testow
            PerceptronWrapper network = neuralNetworks[testedPerson];
            double error = 0.0d;

            foreach (Person person in people) {
                double answer = (person == testedPerson) ? 1.0 : 0.0;

                foreach (int tmp in Enumerable.Range(0, TEST_COUNT)) {
                    DrawInputData(input, testSet[person]);
                    double networkAnswer = network.Compute(new[] { input })[0][0];

                    error += Math.Abs(answer - networkAnswer);
                }
            }

            return error / (TEST_COUNT * people.Count);
        }

        private double HardTest_Threshold(Person testedPerson, Dictionary<Person, List<double[]>[]> testSet, double[] input) {
            Debug.Assert(NeuralNetworkOutputSize == 1);

            PerceptronWrapper network = neuralNetworks[testedPerson];
            double error = 0.0d;
            int testCases = 0;

            foreach (Person person in people) {
                double answer = (person == testedPerson) ? 1.0 : 0.0;
                var personTestSet = testSet[person];

                foreach (var testFile in personTestSet) {
                    var testArray = testFile.ToArray();

                    for (int i = 0; i < testArray.Length - algorithmParams.SignalFramesCount; i++) {
                        for (int j = 0; j < algorithmParams.SignalFramesCount; j++ )
                            Array.Copy(testArray[i+j], 0, input, j * algorithmParams.MfccCount, algorithmParams.MfccCount);
                        double networkAnswer = network.Compute(new[] { input })[0][0];

                        if (answer > THRESEHOLD || answer < (1.0f - THRESEHOLD)) {
                            error += Math.Pow(answer - networkAnswer, 2.0f);
                            testCases++;
                        }
                    }
                }
            }

            return error / testCases;
        }

        private double HardTest(Person testedPerson, Dictionary<Person, List<double[]>[]> testSet, double[] input) {
            Debug.Assert(NeuralNetworkOutputSize == 1);


            PerceptronWrapper network = neuralNetworks[testedPerson];
            double error = 0.0d;
            int testCases = 0;

            foreach (Person person in people) {
                double answer = (person == testedPerson) ? 1.0 : 0.0;
                var personTestSet = testSet[person];

                foreach (var testFile in personTestSet) {
                    var testArray = testFile.ToArray();

                    for (int i = 0; i < testArray.Length - algorithmParams.SignalFramesCount; i++) {
                        for (int j = 0; j < algorithmParams.SignalFramesCount; j++)
                            Array.Copy(testArray[i + j], 0, input, j * algorithmParams.MfccCount, algorithmParams.MfccCount);
                        double networkAnswer = network.Compute(new[] { input })[0][0];

                        error += Math.Pow(answer - networkAnswer, 2.0f);
                        testCases++;
                    }
                }
            }

            return error / testCases;
        }
    }
}