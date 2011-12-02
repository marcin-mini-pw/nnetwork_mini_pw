using System;

namespace NeuralNetworks2.Logic
{
    [Serializable]
    internal class PerceptronWrapperException : Exception
    {
        public PerceptronWrapperException() { }
        public PerceptronWrapperException(string message) : base(message) { }
        public PerceptronWrapperException(string message, Exception inner) : base(message, inner) { }
        protected PerceptronWrapperException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}