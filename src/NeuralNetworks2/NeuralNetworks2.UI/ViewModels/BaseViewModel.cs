using System;
using System.ComponentModel;
using System.Linq.Expressions;
using NeuralNetworks2.UI.Tools;

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
            var handler = PropertyChanged;
            if (handler == null)
            {
                return;
            }

            var e = new PropertyChangedEventArgs(propertyName);
            handler(this, e);
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T, object>> e)
        {
            OnPropertyChanged(PropertyHelper.GetPropertyName(e));
        }
    }
}
