using System;

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
