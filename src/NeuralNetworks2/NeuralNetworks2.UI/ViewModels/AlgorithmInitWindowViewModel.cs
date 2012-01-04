using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using NeuralNetworks2.API.Logic;
using NeuralNetworks2.UI.Tools;
using NeuralNetworks2.UI.Windows;

namespace NeuralNetworks2.UI.ViewModels
{
    public sealed class AlgorithmInitWindowViewModel : BaseWindowViewModel
    {
        private static volatile AlgorithmInitWindowViewModel instance = null;
        private static readonly object instanceLock = new object();

        private readonly PeopleViewModel peopleViewModel;
        private readonly AlgorithmParamsViewModel algorithmParamsViewModel;
        private readonly TrainingProgressViewModel trainingProgressViewModel;

        private RelayCommand startLearningCommand;
        private RelayCommand getLogicFromFileCommand;

        private Thread trainingThread;


        private static IAlgorithmsLogic AlgorithmsLogic
        {
            get
            {
                return LogicProvider.Instance.AlgorithmsLogic;
            }
        }

        /// <summary>
        /// Zwraca instancję viemodelu.
        /// </summary>
        public static AlgorithmInitWindowViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            instance = new AlgorithmInitWindowViewModel();
                        }
                    }
                }
                return instance;
            }
        }


        public PeopleViewModel PeopleViewModel
        {
            get
            {
                return peopleViewModel;
            }
        }

        public AlgorithmParamsViewModel AlgorithmParamsViewModel
        {
            get
            {
                return algorithmParamsViewModel;
            }
        }

        public TrainingProgressViewModel TrainingProgressViewModel
        {
            get
            {
                return trainingProgressViewModel;
            }
        }

        public bool IsTrainingInProgress
        {
            get
            {
                return trainingThread != null;
            }
        }


        private static IFileIOLogic FileIOLogic
        {
            get
            {
                return LogicProvider.Instance.FileIOLogic;
            }
        }

        /// <summary>
        /// Rozpoczyna uczenie sieci.
        /// </summary>
        public ICommand StartLearningCommand
        {
            get
            {
                return startLearningCommand ??
                       (startLearningCommand = new RelayCommand(param => StartLearning(), param => CanStartLearning()));
            }
        }

        public ICommand GetLogicFromFileCommand
        {
            get
            {
                return getLogicFromFileCommand ??
                       (getLogicFromFileCommand = new RelayCommand(param => GetLogicFromFile()));
            }
        }
        

        public void CancelTraining()
        {
            if(trainingThread == null)
            {
                return;
            }

            if (TrainingProgressViewModel.IsReportingProgress)
            {
                TrainingProgressViewModel.EndReportingProgress();
            }

            trainingThread.Abort();
            trainingThread = null;
        }

        /// <summary>
        /// Konstruktor prywatny.
        /// </summary>
        private AlgorithmInitWindowViewModel()
        {
            peopleViewModel = new PeopleViewModel();
            algorithmParamsViewModel = new AlgorithmParamsViewModel();
            trainingProgressViewModel = new TrainingProgressViewModel();
        }


        private void StartLearning()
        {
            AlgorithmsLogic.Reset();
            AlgorithmsLogic.Init(PeopleViewModel.PeopleList, AlgorithmParamsViewModel.AlgorithmParams);
            trainingProgressViewModel.StartReportingProgress();
            trainingThread = new Thread(TrainInSeparateThread);
            trainingThread.IsBackground = true;
            trainingThread.Start();
        }

        private void TrainInSeparateThread()
        {
            var testsResults = AlgorithmsLogic.Train();
            TestsResultsWindowViewModel.Instance.TestsResults = testsResults;
            Application.Current.Dispatcher.Invoke(
                new Action(() =>
                                {
                                    var testsResultsWindow =
                                        new TestsResultsWindow();
                                    testsResultsWindow.ShowDialog();
                                    trainingProgressViewModel.EndReportingProgress();
                                    trainingThread = null;
                                    OnRequestClose();
                                }),
                DispatcherPriority.Normal);
        }

        private bool CanStartLearning()
        {
            //TODO
            return true;
        }

        private void GetLogicFromFile()
        {
            var openFileDialog = new OpenFileDialog();
            SetGetLogicFileDialogExtensionAndFilter(openFileDialog);
            bool? result = openFileDialog.ShowDialog();
            if (!result.HasValue || !result.Value)
            {
                return;
            }

            IAlgorithmsLogic readLogic;
            try
            {
                readLogic = FileIOLogic.ReadAlgorithmsLogic(openFileDialog.FileName);
            }
            catch (Exception)
            {
                MessageBox.Show("Błąd wczytywania danych z pliku!", "Błąd");
                return;
            }

            LogicProvider.Instance.AlgorithmsLogic = readLogic;
            OnRequestClose();
        }

        private void SetGetLogicFileDialogExtensionAndFilter(FileDialog fileDialog)
        {
            fileDialog.DefaultExt = FileIOLogic.DefaultAlgorithmsLogicFileExtension;

            fileDialog.Filter =
                String.Format("NeuralNetworks2 Logic File (*{0})|*{0}|All files (*.*)|*.*",
                FileIOLogic.DefaultAlgorithmsLogicFileExtension);
        }
    }
}