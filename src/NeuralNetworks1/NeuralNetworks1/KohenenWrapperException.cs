using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuralNetworks1 {
    [Serializable]
    public class KohenenWrapperException : Exception {
        public KohenenWrapperException() { }
        public KohenenWrapperException(string message) : base(message) { }
        public KohenenWrapperException(string message, Exception inner) : base(message, inner) { }
        protected KohenenWrapperException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
