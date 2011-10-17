using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuralNetworks1 {
    [Serializable]
    public class PerceptronWrapperException : Exception {
        public PerceptronWrapperException() { }
        public PerceptronWrapperException(string message) : base(message) { }
        public PerceptronWrapperException(string message, Exception inner) : base(message, inner) { }
        protected PerceptronWrapperException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
