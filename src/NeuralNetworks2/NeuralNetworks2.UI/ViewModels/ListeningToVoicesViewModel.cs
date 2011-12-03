using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using NeuralNetworks2.API.Logic;
using NeuralNetworks2.API.Model;
using NeuralNetworks2.UI.Tools;

namespace NeuralNetworks2.UI.ViewModels
{
    /// <summary>s
    /// ViewModel dla widoku nasłuchującego wypowiedzi "na żywo" oraz wyświetlającego wyniki rozpoznawania
    /// autorów takich wypowiedzi.
    /// </summary>
    public class ListeningToVoicesViewModel: BaseViewModel
    {
        /// <summary>
        /// Liczba ludzi, których głos był najbliższy nagranemu przez mikrofon i których wyniki chcemy pokazać.
        /// </summary>
        private const int RecognizedPeopleToShowCount = 5;

        private bool isListening;
        private IList<Tuple<Person, double>> results;

        private RelayCommand startListeningCommand;
        private RelayCommand stopListeningCommand;

        private IAlgorithmsLogic AlgorithmsLogic
        {
            get
            {
                return LogicProvider.Instance.AlgorithmsLogic;
            }
        }


        /// <summary>
        /// Rezultaty ostatniego rozpoznawania osoby po głosie.
        /// </summary>
        public IList<Tuple<Person, double>> Results
        {
            get
            {
                return results;
            }
            set
            {
                if (results == value)
                {
                    return;
                }

                results = value;
                OnPropertyChanged<ListeningToVoicesViewModel>(x => x.Results);
            }
        }

        /// <summary>
        /// Czy trwa nagrywanie głosu z mikrofonu.
        /// </summary>
        public bool IsListening
        {
            get
            {
                return isListening;
            }
            private set
            {
                if(value == isListening)
                {
                    return;
                }

                isListening = value;
                OnPropertyChanged<ListeningToVoicesViewModel>(x => x.IsListening);
            }
        }

        /// <summary>
        /// Rozpoczyna nagrywanie głosu z mikrofonu.
        /// </summary>
        public ICommand StartListeningCommand
        {
            get
            {
                return startListeningCommand ??
                       (startListeningCommand = new RelayCommand(param => StartListening()));
            }
        }

        /// <summary>
        /// Kończy nagrywanie głosu z mikrofonu i próbuje rozpoznać osobę.
        /// </summary>
        public ICommand StopListeningCommand
        {
            get
            {
                return stopListeningCommand ??
                       (stopListeningCommand = new RelayCommand(param => StopListening(), param => CanStopListening()));
            }
        }


        private void StopListening()
        {
            IsListening = false;
            var res = AlgorithmsLogic.StopRecordingAndGetResults();
            Results = res.Take(Math.Min(res.Count, RecognizedPeopleToShowCount))
                         .ToList();
        }

        private bool CanStopListening()
        {
            return IsListening;
        }

        private void StartListening()
        {
            Results = null;
            AlgorithmsLogic.StartRecordingVoice();
            IsListening = true;
        }
    }
}
