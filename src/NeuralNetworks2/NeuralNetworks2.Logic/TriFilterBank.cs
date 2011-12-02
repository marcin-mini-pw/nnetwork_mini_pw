using System;

namespace NeuralNetworks2.Logic
{
    /// <summary>
    /// Na podstawie: 
    /// Comparative Evaluation of Various MFCC Implementations
    /// on the Speaker Verification Task
    /// Todor Ganchev, Nikos Fakotakis, George Kokkinakis
    /// </summary>
    internal static class TriFilterBank
    {
        private static double FMEL3(double flin)
        {
            return 2595.0 * Math.Log10(1.0 + flin / 700.0);
        }

        private static double FMEL4(double flin)
        {
            return 1127.0 * Math.Log(1.0 + flin / 700.0);
        }

        /// <summary>
        /// Odwrotonosc FMEL4
        /// </summary>
        /// <param name="mel"></param>
        /// <returns></returns>
        private static double IFMEL4(double fmel)
        {
            return 700.0 * (Math.Exp(fmel / 1127.0) - 1);
        }

        /// <summary>
        /// N - rozmiar probki, b* - poczatek, srodek i koniec
        /// trojkatnego oka filtra
        /// </summary>
        /// <param name="N"></param>
        /// <param name="bPrev"></param>
        /// <param name="bCurr"></param>
        /// <param name="bNext"></param>
        /// <returns></returns>
        private static double[] CreateFilter(int N, double bPrev, double bCurr, double bNext)
        {
            double[] filter = new double[N];

            for (int i = 0; i < N; i++)
            {
                int k = i;

                if (k < bPrev)
                    filter[i] = 0.0;
                else if (k <= bCurr)
                    filter[i] = (k - bPrev) / (bCurr - bPrev);
                else if (k <= bNext)
                    filter[i] = (bNext - k) / (bNext - bCurr);
                else
                    filter[i] = 0.0;
            }

            return filter;
        }


        /// <summary>
        /// Oblicza wspolczynniki fbi
        /// </summary>
        /// <param name="index">1..M</param>
        /// <returns></returns>
        private static double GetBI(int index, int N, int M, double frequency, double freqLow, double freqHigh)
        {
            return ((float)N / (float)frequency) *
                IFMEL4(FMEL4(freqLow) + index * ((FMEL4(freqHigh) - FMEL4(freqLow)) / (M + 1)));
        }

        /// <summary>
        /// Tworzy bank filtrow bank[index_filtra][skladowe_filtra]
        /// </summary>
        /// <param name="filtersNumber">Ilosc filtrow (np. 20)</param>
        /// <param name="N">Wielkosc wyjscia z FFT</param>
        /// <param name="sampleFrequency">Czestotliwosc probkowania</param>
        /// <param name="lowFrequency">Dolny zakres czestotliwosci</param>
        /// <param name="hightFrequency">Gorny zakres czestotliwosci</param>
        /// <returns></returns>
        public static double[][] CreateFiltersBank(int filtersNumber, int N, double sampleFrequency, double lowFrequency, double hightFrequency)
        {
            double[][] filtersBank = new double[filtersNumber][];

            double[] bi = new double[filtersNumber + 2];
            for (int i = 0; i < bi.Length; i++)
            {
                bi[i] = GetBI(i, N, filtersNumber, sampleFrequency, lowFrequency, hightFrequency);
            }

            for (int i = 1; i <= filtersNumber; i++)
            {
                filtersBank[i - 1] = CreateFilter(N, bi[i - 1], bi[i], bi[i + 1]);
            }

            return filtersBank;
        }
    }
}