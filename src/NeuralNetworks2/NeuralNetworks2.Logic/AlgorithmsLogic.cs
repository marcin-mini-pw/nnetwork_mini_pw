using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using NeuralNetworks2.API.Enums;
using NeuralNetworks2.API.Events;
using NeuralNetworks2.API.Exceptions;
using NeuralNetworks2.API.Logic;
using NeuralNetworks2.API.Model;

namespace NeuralNetworks2.Logic {
    [Serializable]
    public class AlgorithmsLogic : IAlgorithmsLogic, IDeserializationCallback {
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
        private const double TestSetMaxError = 0.01d;

        /// <summary>
        /// Maksymalna liczba iteracji uczenia pojedynczej sieci.
        /// </summary>
        private const int MaxIterationCounts = 160000;

        /// <summary>
        /// Prog powyzej ktorego uznajemy odpowiedz sieci za znaczaca.
        /// </summary>
        private const double THRESHOLD = 0.65; // 0.65

        /// <summary>
        /// Ile razy mniej (w stosunku do liczby wejść) ma być w kolejnych warstwach sieci neuronowych?
        /// </summary> // 4 8 domyslny
        private static readonly double[] HiddenLayerNeuronsCountCoefs = new double[] { 4d, 8d };

        /// <summary>
        /// Służy do nagrywania głosów z mikrofonu.
        /// </summary>
        [NonSerialized]
        private AudioRecorder audioRecorder;

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


        [field: NonSerialized]
        public event EventHandler<TrainingProgressEventArgs> TrainingProgressChanged;


        public AlgorithmsLogic() {
            audioRecorder = new AudioRecorder();
        }

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

            Dictionary<Person, List<double[]>[]> trainingMfccs = GetAllPeopleTrainingDataMfccs(0);
            Dictionary<Person, List<double[]>[]> testMfccs = GetAllPeopleTestDataMfccs(people.Count);

            double[] input = new double[GetNetworksInputSize()];
            double[][] answer = new double[][] { new double[1] };

            int i = 0;
            CallTrainingProgressChanged(TrainingStage.TrainingNetworks, people.Count, i);
            foreach (Person person in people) {
                Debug.WriteLine("Train person: {0}", person.FirstName, null);

                PerceptronWrapper network = neuralNetworks[person];
                TrainPersonNeuralNetwork(trainingMfccs, testMfccs, input, answer, person, network);

                result.Add(person, TestPersonNeuralNetwork(person, testMfccs, input));

                CallTrainingProgressChanged(TrainingStage.TrainingNetworks, people.Count, ++i);
            }

            WasTrained = true;
            PrintTotalError(result);
            return result;
        }

        private void CallTrainingProgressChanged(TrainingStage stage, double max, double value)
        {
            var temp = TrainingProgressChanged;
            if (temp == null)
            {
                return;
            }

            var args = new TrainingProgressEventArgs(stage, value, max, 0d);
            temp(this, args);
        }

        private void PrintTotalError(Dictionary<Person, double> result) {
            double totalError = result.Sum(x => x.Value);
            Debug.WriteLine("Total error: {0}", totalError / result.Count);
        }

        void IDeserializationCallback.OnDeserialization(object sender) {
            audioRecorder = new AudioRecorder();
        }

        private void TrainPersonNeuralNetwork(
            Dictionary<Person, List<double[]>[]> trainingMfccs,
            Dictionary<Person, List<double[]>[]> testMfccs,
            double[] input, double[][] answer, Person person,
            PerceptronWrapper network) {

            GnuPlot plot = new GnuPlot(
                person.FullName,
                @"D:\GIT\nnetwork_mini_pw\charts\plot_" + person.FullName.Replace(' ', '_') + ".png"
                );
            plot.BeginPlot();

            List<double[]>[] personMfccs = null;
            int iterations = 0;

            while (iterations < MaxIterationCounts) {
                double expectedAnswer;

                if (Random.NextDouble() < algorithmParams.TCoef) {
                    expectedAnswer      = 0.0d;
                    Person otherPerson  = SelectOtherPerson(person);
                    personMfccs         = trainingMfccs[otherPerson];
                }
                else {
                    expectedAnswer      = 1.0d;
                    personMfccs         = trainingMfccs[person];
                }

                DrawInputData(input, personMfccs);

                double scale            = Math.Sqrt(1.0 + iterations);
                double newLearningRate  = algorithmParams.LearningRate / scale;
                double newMomentum      = algorithmParams.Momentum / scale;

                answer[0][0] = expectedAnswer;
                network.Train(newLearningRate, 1, newMomentum,
                    new[] { input },
                    answer);

                iterations++;

                if (iterations % 1000 == 0) {
                    double testResult   = TestPersonNeuralNetwork(person, testMfccs, input);
                    double learnResult  = TestPersonNeuralNetwork(person, trainingMfccs, input);
                    AddPlotPoint(plot, iterations, learnResult, testResult);

                    if (testResult <= TestSetMaxError)
                    {
                        break;
                    }
                }
            }

            plot.AddAlgorithmParamsToTitle(algorithmParams);
            plot.End2Plot();
        }

        private static void AddPlotPoint(GnuPlot plot, int iterations, double learnResult, double testResult) {
            plot.AddDataPoint(iterations, learnResult, testResult);

            string line = String.Format(CultureInfo.InvariantCulture, "{0}\t{1}", iterations, testResult);
            Debug.WriteLine(line);
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

        /// <summary>
        /// Losowo wybiera osobe rozna od przekazanej jako parametr.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        private Person SelectOtherPerson(Person person) {
            int     otherPersonIndex    = Random.Next(people.Count);
            Person  otherPerson         = people[otherPersonIndex];

            if (otherPerson == person) {
                int otherPersonsCount   = people.Count - 1;
                Debug.Assert(otherPersonsCount >= 1);

                int newOtherPersonIndex = otherPersonIndex + Random.Next(otherPersonsCount) + 1;
                otherPerson = people[newOtherPersonIndex % people.Count];
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

            var stream   = audioRecorder.StopRecording();
            var mfccs    = GetMfccsFromStream(stream);

            var results = RecogniseSpeaker(mfccs);

            results.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            WasAudioRecordingStarted = false;

            return results;
        }

        private double NormalizeResult(double result) {
            const double NORM_POWER = 0.5;

            double certanity = 0.0;
            if (result >= THRESHOLD) {
                certanity = result;
                return result * Math.Pow((result - THRESHOLD) / (1.0 - THRESHOLD), NORM_POWER);
            }
            else {
                return result * Math.Pow(((1.0 - result) - THRESHOLD) / (1.0 - THRESHOLD), 1.0 / NORM_POWER);
            }
        }

        private List<Tuple<Person, double>> RecogniseSpeaker(List<double[]> mfccs) {
            var results     = new List<Tuple<Person, double>>();
            var mfccArray   = mfccs.ToArray();

            double[]    input           = new double[GetNetworksInputSize()];
            int         totalSamples    = 0;

            Dictionary<Person, double>  resultsPerNetwork   = new Dictionary<Person, double>();
            Dictionary<Person, int>     samplesPerNetwork   = new Dictionary<Person, int>();
            Dictionary<Person, GnuPlot> plots               = new Dictionary<Person, GnuPlot>();

            foreach (var person in people) {
                resultsPerNetwork[person] = 0.0f;
                samplesPerNetwork[person] = 0;
                plots[person] = new GnuPlot(person.FullName, GetRecognitionPlotFileName(person));
                plots[person].BeginPlot();
            }

            for (int start = 0; start < mfccArray.Length - algorithmParams.SignalFramesCount; start++) {
                CopyInputData(input, mfccArray, start);

                var result = from person in people
                              let network = neuralNetworks[person]
                              select new KeyValuePair<Person, double>(
                                  person, 
                                  network.Compute(new[] { input })[0][0]
                                  );

                foreach (var r in result) {
                    plots[r.Key].AddDataPoint(start, r.Value);

                    if (r.Value > THRESHOLD || r.Value < (1.0 - THRESHOLD)) {
                        //samplesPerNetwork[r.Key]++;
                        resultsPerNetwork[r.Key] += (r.Value > THRESHOLD) ? r.Value : (r.Value - 1.0);// NormalizeResult(r.Value);

                        if(r.Value > THRESHOLD)
                            totalSamples++;
                    }
                }
            }

            foreach (var p in people) {
                plots[p].SetYRange(0, 1);
                plots[p].EndPlot();
            }

            var finalResults = from person in people
                               let res = Math.Max(0, resultsPerNetwork[person])
                               let norm = res / (double)totalSamples
                               select new Tuple<Person, double>(person, norm);
            //var finalResults = from person in people
            //                   let samples  = samplesPerNetwork[person]
            //                   let res      = resultsPerNetwork[person]
            //                   let norm     = (samples == 0) ? 0 : (res / (double)samples)
            //                   select new Tuple<Person, double>(person, norm);

            return finalResults.ToList();
        }

        private static string GetRecognitionPlotFileName(Person person) {
            return @"D:\GIT\nnetwork_mini_pw\charts\rplot_" + person.FullName.Replace(' ', '_') + ".png";
        }


        private void CreateNeuralNetworks() {
            neuralNetworks = new Dictionary<Person, PerceptronWrapper>();

            int                 inputSize       = GetNetworksInputSize();
            ICollection<int>    hiddenLayers    = GetLayersCount(inputSize);

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
            var layers = new int[HiddenLayerNeuronsCountCoefs.Length];

            for (int i = 0; i < layers.Length; ++i) {
                layers[i] = Convert.ToInt32( inputSize / HiddenLayerNeuronsCountCoefs[i] );
            }

            return layers;
        }

        private Dictionary<Person, List<double[]>[]> GetAllPeopleTrainingDataMfccs(int countingProgressInitCount)
        {
            return GetAllPeopleMfccsHelper(p => p.TrainWavesPaths, countingProgressInitCount);
        }

        private Dictionary<Person, List<double[]>[]> GetAllPeopleTestDataMfccs(int countingProgressInitCount)
        {
            return GetAllPeopleMfccsHelper(p => p.TestWavesPaths, countingProgressInitCount);
        }

        private Dictionary<Person, List<double[]>[]> GetAllPeopleMfccsHelper(
            Func<Person, ObservableCollection<string>> getWavesPaths, int countingProgressInitCount) {

            CallTrainingProgressChanged(TrainingStage.ComputingMFCC, people.Count << 1, countingProgressInitCount);

            var mfccs = new Dictionary<Person, List<double[]>[]>();
            foreach (Person person in people) {
                var wavesPaths  = getWavesPaths(person);
                var personMfccs = new List<double[]>[wavesPaths.Count];

                for (int i = 0; i < wavesPaths.Count; ++i) {
                    personMfccs[i] = GetMfccsFromFile(wavesPaths[i]);
                }

                mfccs.Add(person, personMfccs);
                CallTrainingProgressChanged(TrainingStage.ComputingMFCC, people.Count << 1, ++countingProgressInitCount);
            }

            return mfccs;
        }

        private List<double[]> GetMfccsFromFile(string path)
        {
            LightWaveFileReader wave = null;
            try
            {
                wave = new LightWaveFileReader(path);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new WaveFileException(path, ex.Message, ex);
            }

            return GetMfccsHelper(wave);
        }

        private List<double[]> GetMfccsFromStream(Stream stream) {
            var wave = new LightWaveFileReader(stream);
            return GetMfccsHelper(wave);
        }

        #region MFCC Stuff

        /// <summary>
        /// Czestosc probkowania, u nas stala.
        /// </summary>
        private const int FREQUENCY = 44100;
        
        /// <summary>
        /// Rozmiar okna 1024, co daje nam 23ms
        /// </summary>
        private const int WINDOW_SIZE = 512;

        /// <summary>
        /// Jak okna sie nakladaja.
        /// </summary>
        private const int OVERLAP = WINDOW_SIZE / 2;

        /// <summary>
        /// Ilosc filtrow trojkatnych.
        /// </summary>
        private const int FILTERS_NUMBER = 24;

        /// <summary>
        /// Tworzymy bank filtrow (jego tworzenie jest kosztowne i powinno obdyc sie
        /// tylko raz.
        /// </summary>
        private static double[][] filters =
            TriFilterBank.CreateFiltersBank(FILTERS_NUMBER, WINDOW_SIZE, FREQUENCY, 0, 8000);

        /// <summary>
        /// Kiedy uznajemy ze na wejsciu jest sygnal mowy
        /// </summary>
        const double SPEAK_POWER_THRESHOLD = 0.03; //0.05; // Ustalone empirycznie

        /// <summary>
        /// Zwaraca liste wsp. mfcc dla kolejnych ramek sgnalu mowy
        /// </summary>
        /// <param name="wave"></param>
        /// <returns></returns>
        private List<double[]> GetMfccsHelper(LightWaveFileReader wave) {
            if (wave.Frequency != FREQUENCY)
                throw new ArgumentException("Only 44100Hz waves are supported");

            const int WIN_DELTA = WINDOW_SIZE - OVERLAP;

            var results     = new List<double[]>();
            var windowData  = new double[WINDOW_SIZE];

            for (int windowBegining = 0; windowBegining < wave.SamplesCount - WINDOW_SIZE; windowBegining += WIN_DELTA) {
                CopyWindowData(wave, windowData, windowBegining);

                double power = windowData.Sum(x => Math.Abs(x)) / WINDOW_SIZE;
                if (power > SPEAK_POWER_THRESHOLD) {
                    var mfcc = MFCCCoefficients.GetMFCC(FREQUENCY, windowData, filters, algorithmParams.MfccCount);
                    results.Add(mfcc);
                }
            }

            if (results.Count < algorithmParams.SignalFramesCount) {
                throw new ArgumentException("Too little usable voice samples found in wave file");
            }

            Debug.WriteLine("Z pliku wyodrebiono {0} ramek", results.Count);
            return results;
        }

        private void CopyWindowData(LightWaveFileReader wave, double[] windowData, int windowBegining) {
            Debug.Assert(windowData.Length == WINDOW_SIZE);

            for (int i = 0; i < WINDOW_SIZE; i++) {
                windowData[i] = wave.SoundSamples[windowBegining + i];
            }
        }

        #endregion

        private double TestPersonNeuralNetwork(Person testedPerson, Dictionary<Person, List<double[]>[]> testSet, double[] input) {
            PerceptronWrapper   network     = neuralNetworks[testedPerson];
            double              error       = 0.0;
            int                 testCases   = 0;

            foreach (Person person in people) {
                double  answer          = (person == testedPerson) ? 1.0 : 0.0;
                var     personTestSet   = testSet[person];

                foreach (var testFile in personTestSet) {
                    var testArray = testFile.ToArray();

                    for (int inputBegining = 0; inputBegining < testArray.Length - algorithmParams.SignalFramesCount; inputBegining++) {

                        CopyInputData(input, testArray, inputBegining);
                        double networkAnswer = network.Compute(new[] { input })[0][0];

                        if (answer > THRESHOLD || answer < (1.0f - THRESHOLD)) {
                            error += Math.Pow(answer - networkAnswer, 2.0f);
                            testCases++;
                        }
                    }
                }
            }

            return error / testCases;
        }

        private void CopyInputData(double[] destination, double[][] source, int start) {
            for (int j = 0; j < algorithmParams.SignalFramesCount; j++)
                Array.Copy(source[start + j], 0, destination, j * algorithmParams.MfccCount, algorithmParams.MfccCount);
        }
    }
}
