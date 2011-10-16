using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuralNetworks1 {
    [Serializable]
    public class InputDataSetException : Exception {
        public InputDataSetException() { }
        public InputDataSetException(string message) : base(message) { }
        public InputDataSetException(string message, Exception inner) : base(message, inner) { }
        protected InputDataSetException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
