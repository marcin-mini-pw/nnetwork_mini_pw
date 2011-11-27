using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoiceAnalyseProto {
    [Serializable]
    public class LightWAVEFileReaderException : Exception {
        public LightWAVEFileReaderException() { }
        public LightWAVEFileReaderException(string message) : base(message) { }
        public LightWAVEFileReaderException(string message, Exception inner) : base(message, inner) { }
        protected LightWAVEFileReaderException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
