using System.Windows.Input;
using NeuralNetworks2.UI.Tools;

namespace NeuralNetworks2.UI.ViewModels
{
    public class AlgorithmInitWindowViewModel : BaseWindowViewModel
    {
        private static volatile AlgorithmInitWindowViewModel instance = null;
        private static readonly object instanceLock = new object();

        private readonly PeopleViewModel peopleViewModel;
        private readonly AlgorithmParamsViewModel algorithmParamsViewModel;

        private RelayCommand startLearningCommand;

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


        /// <summary>
        /// Konstruktor prywatny.
        /// </summary>
        private AlgorithmInitWindowViewModel()
        {
            peopleViewModel = new PeopleViewModel();
            algorithmParamsViewModel = new AlgorithmParamsViewModel();
        }


        private void StartLearning()
        {
            //TODO (stworzyć sieci, nauczyć je, wyświetlić rezultaty na danych testowych i zamknąć okno)

            OnRequestClose();
        }

        private bool CanStartLearning()
        {
            return true;
        }
    }
}