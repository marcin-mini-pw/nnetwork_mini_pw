using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoiceAnalyseProto {
    static class MFCCUtils {
        /// <summary>
        /// Czy dana liczba jest potego liczby 2.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static bool IsPowerOf2(uint number) {
            uint onesCount = 0;
            while (number != 0) {
                onesCount += (number & 1);
                number >>= 1;
            }
            return (onesCount <= 1);
        }

        public static bool IsPowerOf2(int number) {
            return IsPowerOf2((uint)Math.Abs(number));
        }
    }
}
