using System.Collections.Generic;
using NeuralNetworks2.API.Model;

namespace NeuralNetworks2.UI.ViewModels
{
    public sealed class TestsResultsWindowViewModel : BaseWindowViewModel
    {
        private static volatile TestsResultsWindowViewModel instance = null;
        private static readonly object instanceLock = new object();

        private IDictionary<Person, double> testsResults;

        /// <summary>
        /// Zwraca instancję viemodelu.
        /// </summary>
        public static TestsResultsWindowViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            instance = new TestsResultsWindowViewModel();
                        }
                    }
                }
                return instance;
            }
        }

        public IDictionary<Person, double> TestsResults
        {
            get
            {
                return testsResults;
            }
            set
            {
                if (value == testsResults)
                {
                    return;
                }
                testsResults = value;
                OnPropertyChanged<TestsResultsWindowViewModel>(x => x.TestsResults);
            }
        }


        private TestsResultsWindowViewModel()
        {
            
        }
    }
}