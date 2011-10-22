using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuralNetworks1 {
    [Serializable]
    public class KohonenWrapperException : Exception {
        public KohonenWrapperException() { }
        public KohonenWrapperException(string message) : base(message) { }
        public KohonenWrapperException(string message, Exception inner) : base(message, inner) { }
        protected KohonenWrapperException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
