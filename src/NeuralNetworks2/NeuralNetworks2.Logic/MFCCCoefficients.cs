using System;
using System.Diagnostics;
using Complex = alglib.complex;

namespace NeuralNetworks2.Logic
{
    internal static class MFCCCoefficients
    {
        /// <summary>
        /// Oblicza wspolczynniki MFCC dla zadanej probki.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double[] GetMFCC(double sampleFrequency, double[] data, double[][] filtersBank, int coffCount, out double signalPower)
        {
            // 1. Hamming Window
            ApplyHammingWindow(data); 

            // 2. FFT
            Complex[] fft = GetFFT(data);
            
            // Uzyteczna jest tylko 1 polowa tablicy abs(fft)
            // reszta jest odbiciem symetrycznym (z wlasnosci fft)
            // Element realFft[i] - reprezentuje czestotliwosc i Hz, 
            // realFft[0] - reprezentuje moc sygnalu
            // Ale TYLKO dla probki (data) rozmiaru czestotliwosci probkowania
            // dla probki  rozmiaru k realFft[i] reprezentuje czestotliwosc
            // i * sampleFrequency / k
            double[] realFft = ExtractSignalPower(fft);
            signalPower = realFft[0];

            // 3. Filtracja trojkatami z banku filtrow
            double[] logEnergy = FilterData(sampleFrequency, realFft, filtersBank);

            // 4. DCT
            double[] mfcc = GetDCT(logEnergy, coffCount);

            return mfcc;
        }

        private static double[] ExtractSignalPower(Complex[] fft) {
            double[] realFft = new double[fft.Length / 2];
            // Tylko 1 polowa jest znaczaca
            for (int i = 0; i < realFft.Length / 2; i++)
                realFft[i] = alglib.math.abscomplex(fft[i]);
            return realFft;
        }

        /// <summary>
        /// Oblicza wspolczynniki MFCC dla zadanej probki.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double[] GetFurierSpectrum(double[] data, double[][] filtersBank, int coffCount)
        {
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
        /// Wykonaj szybka transformate furiera na zadanym ciagu problek.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static Complex[] GetFFT(double[] data)
        {
            Debug.Assert(MFCCUtils.IsPowerOf2(data.Length));

            var fftOutput = new Complex[data.Length];
            alglib.fft.fftr1d(data, data.Length, ref fftOutput);

            return fftOutput;
        }

        /// <summary>
        /// Dyskretna transformata kosinusowa sygnalu
        /// </summary>
        /// <param name="logEnergy"></param>
        /// <returns></returns>
        private static double[] GetDCT(double[] logEnergy, int mfccCoffsNumber)
        {
            double[] dct = new double[mfccCoffsNumber];

            for (int j = 0; j < dct.Length; j++)
            {
                double sum = 0.0f;
                for (int i = 0; i < logEnergy.Length; i++)
                {
                    sum += logEnergy[i] * Math.Cos((j + 1.0) * (i + 0.5) * Math.PI / logEnergy.Length);
                }
                dct[j] = sum;
            }

            return dct;
        }

        /// <summary>
        /// Filtruje sygnal z fft za pomoca zestawu filtrow z filtersBank
        /// </summary>
        /// <param name="realfft"></param>
        /// <param name="filtersBank"></param>
        /// <returns></returns>
        private static double[] FilterData(double sampleFrequency, double[] realfft, double[][] filtersBank)
        {
            // dla probki  rozmiaru k realFft[j] reprezentuje czestotliwosc
            // j * sampleFrequency / k

            double k = realfft.Length;
            double[] X = new double[filtersBank.Length];

            for (int i = 0; i < X.Length; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < realfft.Length; j++)
                {
                    sum += realfft[j] * filtersBank[i][j];
                }
                X[i] = Math.Log10(sum);
            }

            return X;
        }

        /// <summary>
        /// Przetwarza dane stosujac do nich okno Hamminga
        /// </summary>
        /// <param name="data"></param>
        private static void ApplyHammingWindow(double[] data)
        {
            int N = data.Length;

            for (int n = 0; n < data.Length; n++)
            {
                data[n] *= 0.54f - 0.46f * (float)Math.Cos((2.0 * Math.PI * n) / (N - 1));
            }
        }
    }
}