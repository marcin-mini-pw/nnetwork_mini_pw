using System;
using System.IO;
using NAudio.Wave;

namespace NeuralNetworks2.Logic
{
    internal class AudioRecorder
    {
        private WaveIn waveInStream;
        private WaveFileWriter writer;
        private MemoryStream stream;


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
            stream = new MemoryStream();
            writer = new WaveFileWriter(stream, waveInStream.WaveFormat);

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

            var result = stream;
            stream = null;
            return result;
        }


        private void WaveInStream_DataAvailable(object sender, WaveInEventArgs e)
        {
            writer.WriteData(e.Buffer, 0, e.BytesRecorded);
        }
    }
}