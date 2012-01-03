using System;
using System.ComponentModel;
using System.Linq.Expressions;
using NeuralNetworks2.API.Tools;

namespace NeuralNetworks2.UI.ViewModels
{
    /// <summary>
    /// Bazowy ViewModel.
    /// </summary>
    public abstract class BaseViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedHelper(propertyName, (handler, sender, args) => handler(sender, args));
        }

        protected virtual void OnPropertyChangedDispatcher(string propertyName)
        {
            PropertyChangedHelper(propertyName, 
                (handler, sender, args) => handler(sender, args));
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T, object>> e)
        {
            OnPropertyChanged(PropertyHelper.GetPropertyName(e));
        }


        private void PropertyChangedHelper(string propertyName,
            Action<PropertyChangedEventHandler, object, PropertyChangedEventArgs> callHandler)
        {
            var handler = PropertyChanged;
            if (handler == null)
            {
                return;
            }

            var e = new PropertyChangedEventArgs(propertyName);
            callHandler(handler, this, e);
        }
    }
}
