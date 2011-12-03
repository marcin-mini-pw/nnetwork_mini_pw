using System;


namespace NeuralNetworks2.API.Model
{
    [Serializable]
    public class AlgorithmParams : BaseModel
    {
        private double learningRate;
        private double momentum;
        private int mfccCount;
        private double tCoef;
        private int signalFramesCount;

        /// <summary>
        /// Współczynnik nauki sieci neuronowych.
        /// </summary>
        public double LearningRate
        {
            get
            {
                return learningRate;
            }
            set
            {
                if (value == learningRate)
                {
                    return;
                }

                learningRate = value;
                OnPropertyChanged<AlgorithmParams>(x => x.LearningRate);
            }
        }

        /// <summary>
        /// Moment algorytmu wstecznej propagacji błędów.
        /// </summary>
        public double Momentum
        {
            get
            {
                return momentum;
            }
            set
            {
                if (value == momentum)
                {
                    return;
                }

                momentum = value;
                OnPropertyChanged<AlgorithmParams>(x => x.Momentum);
            }
        }

        /// <summary>
        /// Ilość współczynników MFCC.
        /// </summary>
        public int MfccCount
        {
            get
            {
                return mfccCount;
            }
            set
            {
                if (value == mfccCount)
                {
                    return;
                }

                mfccCount = value;
                OnPropertyChanged<AlgorithmParams>(x => x.MfccCount);
            }
        }

        /// <summary>
        /// Współczynnik znajomości T.
        /// </summary>
        public double TCoef //TODO: ograniczyć wartość do przedziału [0.2; 0.6]
        {
            get
            {
                return tCoef;
            }
            set
            {
                if (value == tCoef)
                {
                    return;
                }

                tCoef = value;
                OnPropertyChanged<AlgorithmParams>(x => x.TCoef);
            }
        }

        /// <summary>
        /// Liczba ramek sygnału mowy.
        /// </summary>
        public int SignalFramesCount
        {
            get
            {
                return signalFramesCount;
            }
            set
            {
                if (value == signalFramesCount)
                {
                    return;
                }

                signalFramesCount = value;
                OnPropertyChanged<AlgorithmParams>(x => x.SignalFramesCount);
            }
        }


        public void CopyFrom(AlgorithmParams algorithmParams)
        {
            LearningRate = algorithmParams.LearningRate;
            Momentum = algorithmParams.Momentum;
            MfccCount = algorithmParams.MfccCount;
            TCoef = algorithmParams.TCoef;
            SignalFramesCount = algorithmParams.SignalFramesCount;
        }
    }
}