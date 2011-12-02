using System;

namespace NeuralNetworks2.Logic
{
    [Serializable]
    public class LightWaveFileReaderException : Exception
    {
        public LightWaveFileReaderException() { }
        public LightWaveFileReaderException(string message) : base(message) { }
        public LightWaveFileReaderException(string message, Exception inner) : base(message, inner) { }
        protected LightWaveFileReaderException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}