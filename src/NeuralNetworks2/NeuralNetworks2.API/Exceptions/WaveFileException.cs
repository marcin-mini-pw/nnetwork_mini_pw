using System;
using System.Runtime.Serialization;

namespace NeuralNetworks2.API.Exceptions
{
    [Serializable]
    public class WaveFileException : Exception
    {
        private const string WaveFilePathSerName = "WaveFilePath";

        private readonly string waveFilePath;

        /// <summary>
        /// Ścieżka do pliku *.wav, który spowodował błąd.
        /// </summary>
        public string WaveFilePath
        {
            get
            {
                return waveFilePath;
            }
        }


        public WaveFileException() { }

        public WaveFileException(string message) : base(message) { }

        public WaveFileException(string waveFilePath, string message)
            : base(message)
        {
            this.waveFilePath = waveFilePath;
        }

        public WaveFileException(string message, Exception inner) : base(message, inner) { }

        public WaveFileException(string waveFilePath, string message, Exception inner)
            : base(message, inner)
        {
            this.waveFilePath = waveFilePath;
        }

        protected WaveFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info != null)
            {
                waveFilePath = info.GetString(WaveFilePathSerName);
            }
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (info != null)
            {
                info.AddValue(WaveFilePathSerName, WaveFilePath);
            }
        }
    }
}