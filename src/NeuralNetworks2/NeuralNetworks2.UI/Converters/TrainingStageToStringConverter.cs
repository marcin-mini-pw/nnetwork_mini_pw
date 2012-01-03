using System;
using System.Globalization;
using System.Windows.Data;
using NeuralNetworks2.API.Enums;

namespace NeuralNetworks2.UI.Converters
{
    [ValueConversion(typeof(TrainingStage), typeof(string))]
    public class TrainingStageToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stage = (TrainingStage) value;
            switch (stage)
            {
                case TrainingStage.ComputingMFCC:
                    return "Obliczanie współczynników MFCC...";
                case TrainingStage.TrainingNetworks:
                    return "Uczenie sieci neuronowych...";
                default:
                    return String.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}