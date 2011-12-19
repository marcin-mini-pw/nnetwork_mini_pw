using System;
using System.Windows.Input;
using Microsoft.Win32;
using NeuralNetworks2.API.Logic;
using NeuralNetworks2.UI.Tools;
using NeuralNetworks2.UI.Windows;

namespace NeuralNetworks2.UI.ViewModels
{
    /// <summary>
    /// ViewModel dla głównego okna aplikacji.
    /// </summary>
    public sealed class MainWindowViewModel : BaseWindowViewModel
    {
        private static volatile MainWindowViewModel instance = null;
        private static readonly object instanceLock = new object();

        private readonly ListeningToVoicesViewModel listeningToVoicesViewModel;

        private RelayCommand newAlgorithmCommand;
        private RelayCommand saveAlgorithmLogicCommand;


        /// <summary>
        /// Zwraca instancję viemodelu.
        /// </summary>
        public static MainWindowViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            instance = new MainWindowViewModel();
                        }
                    }
                }
                return instance;
            }
        }

        public ListeningToVoicesViewModel ListeningToVoicesViewModel
        {
            get
            {
                return listeningToVoicesViewModel;
            }
        }

        public AlgorithmInitWindowViewModel AlgorithmInitWindowViewModel
        {
            get
            {
                return AlgorithmInitWindowViewModel.Instance;
            }
        }

        private static IFileIOLogic FileIOLogic
        {
            get
            {
                return LogicProvider.Instance.FileIOLogic;
            }
        }

        public ICommand NewAlgorithmCommand
        {
            get
            {
                return newAlgorithmCommand ??
                       (newAlgorithmCommand = new RelayCommand(param => NewAlgorithm(), param => CanNewAlgorithm()));
            }
        }


        public ICommand SaveAlgorithmLogicCommand
        {
            get
            {
                return saveAlgorithmLogicCommand ??
                       (saveAlgorithmLogicCommand =
                        new RelayCommand(param => SaveAlgorithmLogic(), param => CanSaveAlgorithmLogic()));
            }
        }



        /// <summary>
        /// Konstruktor prywatny.
        /// </summary>
        private MainWindowViewModel()
        {
            listeningToVoicesViewModel = new ListeningToVoicesViewModel();
        }


        public bool Init()
        {
            return InitializeAppForNewPeopleGroup(true);
        }


        /// <summary>
        /// Inicjalizuje działanie programu na nowo (tzn. dla innego zestawu ludzi lub np. dla innych
        /// parametrów sieci neuronowych).
        /// </summary>
        /// <param name="closeIfCancel">Ustawienie na true spowoduje, że jeśli użytkownik anuluje inicjalizację
        /// nowego algorytmu, aplikacja zostanie zamknięta.</param>
        private bool InitializeAppForNewPeopleGroup(bool closeIfCancel)
        {
            var algorithmInitWindow = new AlgorithmInitWindow();
            bool? result = algorithmInitWindow.ShowDialog();
            if (!result.HasValue || !result.Value)
            {
                if (closeIfCancel)
                {
                    OnRequestClose();
                }
                return false;
            }
            ListeningToVoicesViewModel.Results = null;

            return true;
        }

        private void NewAlgorithm()
        {
            InitializeAppForNewPeopleGroup(false);
        }

        private bool CanNewAlgorithm()
        {
            return !ListeningToVoicesViewModel.IsListening;
        }

        private static void SaveAlgorithmLogic()
        {
            var saveFileDialog = new SaveFileDialog();
            SetGetLogicFileDialogExtensionAndFilter(saveFileDialog);
            bool? result = saveFileDialog.ShowDialog();
            if (!result.HasValue || !result.Value)
            {
                return;
            }

            var algLogic = LogicProvider.Instance.AlgorithmsLogic;
            FileIOLogic.SaveAlgorithmsLogic(algLogic, saveFileDialog.FileName);
        }

        private bool CanSaveAlgorithmLogic()
        {
            return !ListeningToVoicesViewModel.IsListening;
        }

        private static void SetGetLogicFileDialogExtensionAndFilter(FileDialog fileDialog)
        {
            fileDialog.DefaultExt = FileIOLogic.DefaultAlgorithmsLogicFileExtension;

            fileDialog.Filter =
                String.Format("NeuralNetworks2 Logic File (*{0})|*{0}|All files (*.*)|*.*",
                FileIOLogic.DefaultAlgorithmsLogicFileExtension);
        }
    }
}