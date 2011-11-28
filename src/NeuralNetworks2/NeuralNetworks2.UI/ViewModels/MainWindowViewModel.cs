using System;
using System.Windows.Input;
using NeuralNetworks2.UI.Tools;

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

        private RelayCommand closeCommand;


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


        /// <summary>
        /// Powoduje wywołanie zdarzenia <see cref="RequestClose"/>.
        /// </summary>
        public ICommand CloseCommand
        {
            get
            {
                return closeCommand ??
                       (closeCommand = new RelayCommand(param => OnRequestClose()));
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
            if(!result.HasValue || !result.Value)
            {
                if(closeIfCancel)
                {
                    OnRequestClose();
                }
                return false;
            }

            //TODO
            return true;
        }
    }
}