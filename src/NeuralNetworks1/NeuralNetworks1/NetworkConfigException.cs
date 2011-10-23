using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuralNetworks1 {
    [Serializable]
    public class NetworkConfigException : Exception {
        public NetworkConfigException() { }
        public NetworkConfigException(string message) : base(message) { }
        public NetworkConfigException(string message, Exception inner) : base(message, inner) { }
        protected NetworkConfigException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
