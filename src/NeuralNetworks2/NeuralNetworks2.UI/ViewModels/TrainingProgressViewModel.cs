using NeuralNetworks2.API.Enums;
using NeuralNetworks2.API.Events;
using NeuralNetworks2.API.Logic;
using NeuralNetworks2.UI.Tools;

namespace NeuralNetworks2.UI.ViewModels
{
    /// <summary>
    /// ViewModel dla widoku prezentującego postępy w nauce sieci.
    /// </summary>
    public class TrainingProgressViewModel: BaseViewModel
    {
        private double value;
        private double minimum;
        private double maximum;
        private TrainingStage trainingStage;


        private static IAlgorithmsLogic AlgorithmsLogic
        {
            get
            {
                return LogicProvider.Instance.AlgorithmsLogic;
            }
        }


        public double Value
        {
            get
            {
                return value;
            }
            set
            {
                if (value == this.value)
                {
                    return;
                }
                this.value = value;
                OnPropertyChanged<TrainingProgressViewModel>(x => x.Value);
            }
        }

        public double Minimum
        {
            get
            {
                return minimum;
            }
            set
            {
                if (value == this.minimum)
                {
                    return;
                }
                this.minimum = value;
                OnPropertyChanged<TrainingProgressViewModel>(x => x.Minimum);
            }
        }

        public double Maximum
        {
            get
            {
                return maximum;
            }
            set
            {
                if (value == this.maximum)
                {
                    return;
                }
                this.maximum = value;
                OnPropertyChanged<TrainingProgressViewModel>(x => x.Maximum);
            }
        }

        public TrainingStage TrainingStage
        {
            get
            {
                return trainingStage;
            }
            set
            {
                if (value == this.trainingStage)
                {
                    return;
                }
                this.trainingStage = value;
                OnPropertyChanged<TrainingProgressViewModel>(x => x.TrainingStage);
            }
        }

        public bool IsReportingProgress { get; private set; }

        
        public void StartReportingProgress()
        {
            Minimum = 0d;
            Maximum = 1d;
            Value = 0d;
            TrainingStage = TrainingStage.Unknown;
            AlgorithmsLogic.TrainingProgressChanged += AlgorithmsLogic_TrainingProgressChanged;
            IsReportingProgress = true;
        }

        public void EndReportingProgress()
        {
            if (IsReportingProgress)
            {
                AlgorithmsLogic.TrainingProgressChanged -= AlgorithmsLogic_TrainingProgressChanged;
                IsReportingProgress = false;
            }
        }


        private void AlgorithmsLogic_TrainingProgressChanged(object sender, TrainingProgressEventArgs e)
        {
            Minimum = e.Minimum;
            Maximum = e.Maximum;
            Value = e.Value;
            TrainingStage = e.Stage;
        }
    }
}
