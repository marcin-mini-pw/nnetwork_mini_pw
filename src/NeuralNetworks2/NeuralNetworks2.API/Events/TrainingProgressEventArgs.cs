using System;
using NeuralNetworks2.API.Enums;

namespace NeuralNetworks2.API.Events
{
    public class TrainingProgressEventArgs: EventArgs
    {
        private readonly double value;
        private readonly double minimum;
        private readonly double maximum;
        private TrainingStage stage;


        public double Value
        {
            get
            {
                return value;
            }
        }

        public double Minimum
        {
            get
            {
                return minimum;
            }
        }

        public double Maximum
        {
            get
            {
                return maximum;
            }
        }

        public TrainingStage Stage
        {
            get
            {
                return stage;
            }
        }


        public TrainingProgressEventArgs(TrainingStage stage, double value,
            double maximum, double minimum)
        {
            this.stage = stage;
            this.value = value;
            this.maximum = maximum;
            this.minimum = minimum;
        }
    }
}
