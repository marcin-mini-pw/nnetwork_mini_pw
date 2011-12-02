using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NeuralNetworks2.API.Exceptions;
using NeuralNetworks2.API.Logic;
using NeuralNetworks2.API.Model;

namespace NeuralNetworks2.Logic
{
    public class AlgorithmsLogic : IAlgorithmsLogic
    {
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
        private const double TestSetMaxError = 0.3d;

        /// <summary>
        /// Maksymalna liczba iteracji uczenia pojedynczej sieci.
        /// </summary>
        private const int MaxIterationCounts = 10000;

        /// <summary>
        /// Ile razy mniej (w stosunku do liczby wejść) ma być w kolejnych warstwach sieci neuronowych?
        /// </summary>
        private static readonly double[] HiddenLayerNeuronsCountCoefs = new double[] { 8d, 16d };

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
        private bool WasInitialized
        {
            get
            {
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
        public void Init(IList<Person> peopleToBeRecognized, AlgorithmParams algParams)
        {
            if (peopleToBeRecognized == null)
            {
                throw new ArgumentNullException("peopleToBeRecognized");
            }

            if (!peopleToBeRecognized.Any())
            {
                throw new ArgumentOutOfRangeException("peopleToBeRecognized", "You should assign at least one person.");
            }

            if (algParams == null)
            {
                throw new ArgumentNullException("algParams");
            }

            if (WasInitialized)
            {
                throw new InvalidOperationException("AlgorithmsLogic was already initialized!");
            }

            people = peopleToBeRecognized;
            algorithmParams = algParams;
            CreateNeuralNetworks();
        }

        /// <summary>
        /// Resetule logikę (usuwa zbudowane sieci neuronowe itp.). Potem jest potrzebna nowa inicjalizacja.
        /// </summary>
        public void Reset()
        {
            if (WasAudioRecordingStarted)
            {
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
        public IDictionary<Person, double> Train()
        {
            if (!WasInitialized)
            {
                throw new InvalidOperationException("You have to initialize logic first!");
            }

            var result = new Dictionary<Person, double>();

            Dictionary<Person, double[][]> trainingMfccs = GetAllPeopleTrainingDataMfccs();
            Dictionary<Person, double[][]> testMfccs = GetAllPeopleTestDataMfccs();

            foreach (Person person in people)
            {
                PerceptronWrapper network = neuralNetworks[person];
                int iterations = 0;
                double error = 1.0d;
                while (error > TestSetMaxError && iterations < MaxIterationCounts)
                {
                    double r = Random.NextDouble(); //losowa liczba z zakresu [0.0; 1.0]
                    double expectedAnswer = 1.0d;
                    double[][] personMfccs = trainingMfccs[person];
                    if (r >= algorithmParams.TCoef)
                    {
                        expectedAnswer = 0.0d;
                        int otherPersonInd = Random.Next(neuralNetworks.Count);
                        Person otherPerson = people[otherPersonInd];
                        if (otherPerson == person)
                        {
                            otherPerson = people[(otherPersonInd + 1) % neuralNetworks.Count];
                        }
                        personMfccs = trainingMfccs[otherPerson];
                    }

                    var expectedAnswers = new double[personMfccs.Length];
                    for (int i = 0; i < expectedAnswers.Length; ++i)
                    {
                        expectedAnswers[i] = expectedAnswer;
                    }
                    network.Train(algorithmParams.LearningRate, 1, algorithmParams.Momentum, personMfccs,
                                  new[] { expectedAnswers });
                    error = Test(person, testMfccs);
                    iterations++;
                }

                result.Add(person, error);
            }

            WasTrained = true;
            return result;
        }

        /// <summary>
        /// Rozpoczyna nagrywanie dźwięku z mikrofonu.
        /// </summary>
        public void StartRecordingVoice()
        {
            if (!WasInitialized)
            {
                throw new InvalidOperationException("You have to initialize logic first!");
            }

            if (!WasTrained)
            {
                throw new InvalidOperationException("You have to train neural networks first!");
            }

            audioRecorder.StartRecording();
            WasAudioRecordingStarted = true;
        }

        /// <summary>
        /// Kończy nagrywanie dźwięku z mikrofonu.
        /// </summary>
        /// <returns>Rezultaty zwrócone przez sieci odpowiadające poszczególnym osobom.</returns>
        public IDictionary<Person, double> StopRecordingAndGetResults()
        {
            if (!WasAudioRecordingStarted)
            {
                throw new InvalidOperationException("You have to start recording first!");
            }

            var stream = audioRecorder.StopRecording();
            double[] mfccs = GetMfccsFromStream(stream);
            var results = new Dictionary<Person, double>();

            foreach (Person person in people)
            {
                PerceptronWrapper network = neuralNetworks[person];
                double networkRes = network.Compute(new[] { mfccs })[0][0];
                results.Add(person, networkRes);
            }

            WasAudioRecordingStarted = false;
            return results;
        }


        private void CreateNeuralNetworks()
        {
            neuralNetworks = new Dictionary<Person, PerceptronWrapper>();

            int inputSize = GetNetworksInputSize();
            ICollection<int> hiddenLayers = GetLayersCount(inputSize);
            foreach (var person in people)
            {
                var perceptron = new PerceptronWrapper(inputSize, hiddenLayers, NeuralNetworkOutputSize, UseBias,
                                                       UseUniPolarActivationFunction);
                neuralNetworks.Add(person, perceptron);
            }
        }

        private int GetNetworksInputSize()
        {
            return algorithmParams.MfccCount * algorithmParams.SignalFramesCount;
        }

        private int[] GetLayersCount(int inputSize)
        {
            var inputSizeD = (double)inputSize;
            var layers = new int[HiddenLayerNeuronsCountCoefs.Length];

            for (int i = 0; i < layers.Length; ++i)
            {
                layers[i] = Convert.ToInt32(inputSizeD / HiddenLayerNeuronsCountCoefs[i]);
            }

            return layers;
        }

        private Dictionary<Person, double[][]> GetAllPeopleTrainingDataMfccs()
        {
            return GetAllPeopleMfccsHelper(p => p.TrainWavesPaths);
        }

        private Dictionary<Person, double[][]> GetAllPeopleTestDataMfccs()
        {
            return GetAllPeopleMfccsHelper(p => p.TestWavesPaths);
        }

        private Dictionary<Person, double[][]> GetAllPeopleMfccsHelper(
            Func<Person, ObservableCollection<string>> getWavesPaths)
        {
            var mfccs = new Dictionary<Person, double[][]>();
            foreach (Person person in people)
            {
                ObservableCollection<string> wavesPaths = getWavesPaths(person);
                var personMfccs = new double[wavesPaths.Count][];
                for (int i = 0; i < wavesPaths.Count; ++i)
                {
                    personMfccs[i] = GetMfccsFromFile(wavesPaths[i]);
                }

                mfccs.Add(person, personMfccs);
            }

            return mfccs;
        }

        private double[] GetMfccsFromFile(string path)
        {
            LightWaveFileReader wave = null;
            try
            {
                wave = new LightWaveFileReader(path);
            }
            catch (Exception ex)
            {
                throw new WaveFileException(path, ex.Message, ex);
            }

            return GetMfccsHelper(wave);
        }

        private double[] GetMfccsFromStream(Stream stream)
        {
            var wave = new LightWaveFileReader(stream);
            return GetMfccsHelper(wave);
        }

        private double[] GetMfccsHelper(LightWaveFileReader wave)
        {
            const int windowSize = 1024;
            const int overlap = 512;
            const int windowsDelta = windowSize - overlap;

            var result = new double[GetNetworksInputSize()];

            double[][] filters = TriFilterBank.CreateFiltersBank(20, 1024, wave.Frequency, 0, 4800);
            //TODO: te stałe wyrzucić gdzieś wyżej

            var windowData = new double[windowSize];

            int s = 0;
            for (int i = 0; i < wave.SamplesCount - windowSize; i += windowsDelta)
            {
                if (s + 1 >= algorithmParams.SignalFramesCount)
                {
                    break;
                }

                for (int j = 0; j < windowSize; j++)
                {
                    windowData[j] = wave.SoundSamples[i + j];
                }

                var mfcc = MFCCCoefficients.GetMFCC(windowData, filters, algorithmParams.MfccCount);
                for (int j = s * algorithmParams.MfccCount, k = 0; j < s * (algorithmParams.MfccCount + 1); ++j, ++k)
                {
                    result[j] = mfcc[k];
                }
                ++s;
            }

            if (s <= algorithmParams.SignalFramesCount)
            {
                for (int i = s * algorithmParams.MfccCount; i < result.Length; ++i)
                {
                    result[i] = 0.0; //TODO: czy może powinniśmy dopełniać tutaj czymś innym niż zerami?
                    //może rzucić po prostu jakimś wyjątkiem jak sygnał był za krótki?
                }
            }

            return result;
        }

        /// <summary>
        /// Wykonuje testy dla danej osoby na zbiorach testowych.
        /// </summary>
        /// <param name="person">Osoba, której sieć chcemy przetestować.</param>
        /// <param name="testSetMfccs">Współczynniki MFCC dla wszystkich próbek testowych dla wszystkich osób.</param>
        /// <returns>Błąd sieci odpowiadającej zadanej osobie na zbiorze testowym.</returns>
        private double Test(Person person, Dictionary<Person, double[][]> testSetMfccs)
        {
            Debug.Assert(NeuralNetworkOutputSize == 1);

            PerceptronWrapper network = neuralNetworks[person];
            double error = 0.0d;
            int testsCount = 0;
            foreach (Person p in people)
            {
                var mfccs = testSetMfccs[p];
                var result = network.Compute(mfccs)[0];

                if (p == person)
                {
                    for (int i = 0; i < result.Length; ++i)
                    {
                        result[i] = 1.0d - result[i];
                    }
                }

                error += result.Sum();
                testsCount += result.Length;
            }

            return error / testsCount; //TODO: przemyśleć, czy zwracanie tutaj średniej arytmetycznej jest dobrym pomysłem
        }
    }
}