using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NAudio;
using NAudio.Wave;


namespace VoiceAnalyseProto {
    class LightWAVEFileReader {

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
        public LightWAVEFileReader(string path) {
            ReadWAVEFile(path);
        }

        private void ReadWAVEFile(string path) {
            using (var waveReader = new WaveFileReader(path)) {
                Frequency = waveReader.WaveFormat.SampleRate;
                Bits = waveReader.WaveFormat.BitsPerSample;
                SamplesCount = (int) waveReader.SampleCount;

                SoundSamples = new float[SamplesCount];

                if (waveReader.WaveFormat.Channels != MONO_SOUND)
                    throw new LightWAVEFileReaderException("Sorry, we support only MONO wave files!");

                waveReader.Read(new float[][] { SoundSamples }, SamplesCount);
            }
        }

        /// <summary>
        /// Normalizuje sample dzwieku do zakresu -1..1
        /// </summary>
        public void NormalizeWaveSamples() {
            const float EPSILON = 0.0001f;

            float min = SoundSamples.Min();
            float max = SoundSamples.Max();

            float dist2 = (max - min) / 2.0f;

            if (dist2 > EPSILON) {
                SoundSamples = SoundSamples
                    .Select(x => ((x - min) / dist2) - 1.0f)
                    .ToArray();
            }
            else {
                float avg = (max + min) / 2.0f;
                SoundSamples = SoundSamples
                    .Select(x => (x - avg))
                    .ToArray();
            }
        }
    }
}
