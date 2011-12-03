using System;
using System.Windows.Input;
using NeuralNetworks2.UI.Tools;

namespace NeuralNetworks2.UI.ViewModels
{
    /// <summary>
    /// Bazowy ViewModel dla viewmodeli, z których korzystają okienka aplikacji.
    /// </summary>
    public class BaseWindowViewModel: BaseViewModel
    {
        /// <summary>
        /// Wywoływane, gdy widok powinien zostać zamknięty.
        /// </summary>
        public event EventHandler RequestClose;

        private RelayCommand closeCommand;


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
        /// Wywołuje zdarzenie <see cref="RequestClose"/>.
        /// </summary>
        protected void OnRequestClose()
        {
            var handler = RequestClose;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
