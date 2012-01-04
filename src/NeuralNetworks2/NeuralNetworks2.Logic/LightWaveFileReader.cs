using System.IO;
using System.Linq;
using NAudio.Wave;
using System.Diagnostics;
using System;


namespace NeuralNetworks2.Logic {
    internal class LightWaveFileReader {
        private const int MONO_SOUND = 1;


        /// <summary>
        /// Czestotliwosc z jaka nagrano plik WAVE
        /// </summary>
        public int Frequency { get; private set; }

        /// <summary>
        /// Rozdzielczosc probkowania
        /// </summary>
        public int Bits { get; private set; }

        /// <summary>
        /// Ilosc problek w pliku
        /// </summary>
        public int SamplesCount { get; private set; }

        /// <summary>
        /// Skwantowane probki dzwieku.
        /// </summary>
        public float[] SoundSamples { get; private set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Nazwa pliku WAVE (*.wav)</param>
        public LightWaveFileReader(string path) {
            ReadWaveFile(path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream">Strumień z zapisanym dźwiękiem w formacie *.wav.</param>
        public LightWaveFileReader(Stream stream) {
            ReadWaveFile(stream);
        }


        /// <summary>
        /// Normalizuje sample dzwieku do zakresu -1..1
        /// </summary>
        public void NormalizeWaveSamples() {
            const float EPSILON = 0.0001f;
            const float NOISE_LEVEL_COEF = 0.3f;

            SamplesCount = SoundSamples.Length;

            float min = SoundSamples.Min();
            float max = SoundSamples.Max();

            // Nowe wzmocnienie dziala tak ze
            // 0 => 0, pomocne przy ustalaniu mocy sygnau
            float emph = 1.0f;
            if (Math.Abs(min) > EPSILON)
                emph = Math.Max(emph, 1.0f / Math.Abs(min));

            if (Math.Abs(max) > EPSILON)
                emph = Math.Min(emph, 1.0f / Math.Abs(max));

            if (emph > 1.0f) {
                SoundSamples = SoundSamples
                    .Select(x => {
                                    if (Math.Abs(x) > NOISE_LEVEL_COEF) {
                                        x *= emph;
                                    }
                                    return x;
                                })
                    .ToArray();
            }
        }


        private void ReadWaveFile(string path) {
            Debug.WriteLine("Load file: {0}", path);
            using (var waveReader = new WaveFileReader(path)) {
                ReadWaveFileHelper(waveReader);
            }
        }

        private void ReadWaveFile(Stream stream) {
            using (var waveReader = new WaveFileReader(stream)) {
                ReadWaveFileHelper(waveReader);
            }

            stream.Dispose(); // tak na wszelki wypadek
        }

        private void ReadWaveFileHelper(WaveFileReader waveReader) {
            Frequency = waveReader.WaveFormat.SampleRate;
            Bits = waveReader.WaveFormat.BitsPerSample;
            SamplesCount = (int)waveReader.SampleCount;

            SoundSamples = new float[SamplesCount];

            if (waveReader.WaveFormat.Channels != MONO_SOUND)
                throw new LightWaveFileReaderException("Sorry, we support only MONO wave files!");

            waveReader.Read(new float[][] { SoundSamples }, SamplesCount);
            NormalizeWaveSamples();
        }
    }
}