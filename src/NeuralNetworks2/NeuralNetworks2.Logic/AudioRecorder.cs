using System;
using System.IO;
using NAudio.Wave;

namespace NeuralNetworks2.Logic
{
    internal class AudioRecorder
    {
        private const string FileName = "recordedAudio.wav";

        private WaveIn waveInStream;
        private WaveFileWriter writer;


        public bool IsRecording
        {
            get
            {
                return waveInStream != null;
            }
        }

        public void StartRecording()
        {
            waveInStream = new WaveIn();
            waveInStream.WaveFormat = new WaveFormat(44100, 16, 1);

            writer = new WaveFileWriter(FileName, waveInStream.WaveFormat);

            waveInStream.DataAvailable += WaveInStream_DataAvailable;

            waveInStream.StartRecording();
        }

        public Stream StopRecording()
        {
            if (!IsRecording)
            {
                throw new InvalidOperationException("You have to start recording first!");
            }

            waveInStream.StopRecording();
            waveInStream.Dispose();
            waveInStream = null;

            writer.Close();
            writer = null;

            return new FileStream(FileName, FileMode.Open);
        }


        private void WaveInStream_DataAvailable(object sender, WaveInEventArgs e)
        {
            writer.WriteData(e.Buffer, 0, e.BytesRecorded);
        }
    }
}