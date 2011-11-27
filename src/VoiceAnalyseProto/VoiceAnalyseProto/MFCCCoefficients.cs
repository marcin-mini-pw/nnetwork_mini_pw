using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Complex = alglib.complex;

namespace VoiceAnalyseProto {
    static class MFCCCoefficients {

        /// <summary>
        /// Wykonaj szybka transformate furiera na zadanym ciagu problek.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static Complex[] GetFFT(double[] data) {
            Debug.Assert(MFCCUtils.IsPowerOf2(data.Length));

            var fftOutput = new Complex[data.Length];
            alglib.fft.fftr1d(data, data.Length, ref fftOutput);

            return fftOutput;
        }

        /// <summary>
        /// Oblicza wspolczynniki MFCC dla zadanej probki.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double[] GetMFCC(double[] data, double[][] filtersBank, int coffCount) {
            // 1. Hamming Window
            ApplyHammingWindow(data);

            // 2. FFT
            Complex[] fft = GetFFT(data);

            // 3. Filtracja trojkatami z banku filtrow
            double[] logEnergy = FilterData(fft, filtersBank);

            // 4. DCT
            double[] mfcc = GetDCT(logEnergy, coffCount);

            return mfcc;
        }

        /// <summary>
        /// Oblicza wspolczynniki MFCC dla zadanej probki.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double[] GetFurierSpectrum(double[] data, double[][] filtersBank, int coffCount) {
            // 1. Hamming Window
            ApplyHammingWindow(data);

            // 2. FFT
            Complex[] fft = GetFFT(data);
            double[] mfcc = new double[coffCount];

            for (int i = 0; i < coffCount; i++)
                mfcc[i] = alglib.math.abscomplex(fft[i]);

                return mfcc;
        }

        /// <summary>
        /// Dyskretna transformata kosinusowa sygnalu
        /// </summary>
        /// <param name="logEnergy"></param>
        /// <returns></returns>
        private static double[] GetDCT(double[] logEnergy, int mfccCoffsNumber) {
            double[] dct = new double[mfccCoffsNumber];

            for (int j = 0; j < dct.Length; j++) { 
                double sum = 0.0f;
                for (int i = 0; i < logEnergy.Length; i++) {
                    sum += logEnergy[i] * Math.Cos(j * (i - 0.5) * Math.PI / mfccCoffsNumber);
                }
                dct[j] = sum;
            }

            return dct;
        }

        /// <summary>
        /// Filtruje sygnal z fft za pomoca zestawu filtrow z filtersBank
        /// </summary>
        /// <param name="fft"></param>
        /// <param name="filtersBank"></param>
        /// <returns></returns>
        private static double[] FilterData(Complex[] fft, double[][] filtersBank) {
            double[] X = new double[filtersBank.Length];

            for (int i = 0; i < X.Length; i++) {
                double sum = 0.0;
                for (int j = 0; j < fft.Length; j++) {
                    sum += alglib.math.abscomplex(fft[j]) * filtersBank[i][j];
                }
                X[i] = Math.Log10(sum);
            }

            return X;
        }

        /// <summary>
        /// Przetwarza dane stosujac do nich okno Hamminga
        /// </summary>
        /// <param name="data"></param>
        private static void ApplyHammingWindow(double[] data) {
            int N = data.Length;

            for (int n = 0; n < data.Length; n++) {
                data[n] *= 0.54f - 0.46f * (float)Math.Cos((2.0 * Math.PI * n) / (N - 1));
            }
        }
    }
}
