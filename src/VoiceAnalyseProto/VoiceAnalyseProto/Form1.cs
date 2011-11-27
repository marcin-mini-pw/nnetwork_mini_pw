using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio;
using System.Threading;

namespace VoiceAnalyseProto {
    public partial class Form1 : Form {

        private LightWAVEFileReader _wave;
        
        private double[][] _filters;

        public Form1() {
            InitializeComponent();

            _wave = new LightWAVEFileReader(@"D:\Dokumenty\Materiały na studia\Semestr IX\Sieci neuronowe\VoiceAnalyseProto\VoiceAnalyseProto\bin\Debug\tone.wav");
            _filters = TriFilterBank.CreateFiltersBank(20, 1024, 44100, 0, 40000);
        }

        private void panel1_Paint(object sender, PaintEventArgs e) {
            if (_wave != null)
                DrawSamples(panel1, e.Graphics, _wave.SoundSamples, _wave.SamplesCount, -1.0f, 1.0f, 16);
        }

        private void DrawSamples(Panel panel, Graphics g, float[] data, int dataCount, float minValue, float maxValue, int step = 1) {
            g.ResetClip();

            float dist = maxValue - minValue;
            float dx = (float) panel.ClientRectangle.Width / (float) dataCount;
            float x = 0.0f;

            Func<int, float> fGetData = (int index) => {
                return (1.0f - ((data[index] - minValue) / dist)) * panel.ClientRectangle.Height;
            };

            g.DrawRectangle(Pens.Black, panel.ClientRectangle);
            for (int i = step; i < dataCount; i += step) {
                g.DrawLine(Pens.Red, x, fGetData(i-step), x + dx, fGetData(i));
                x += dx * step;
            }
        }

        private void DrawSamples(Panel panel, Graphics g, double[] data, int dataCount, double minValue, double maxValue, int step = 1) {
            g.ResetClip();

            double dist = maxValue - minValue;
            float dx = (float)panel.ClientRectangle.Width / (float)dataCount;
            float x = 0.0f;

            Func<int, double> fGetData = (int index) => {
                return (1.0f - ((data[index] - minValue) / dist)) * panel.ClientRectangle.Height;
            };

           // g.DrawRectangle(Pens.Black, panel.ClientRectangle);
            for (int i = step; i < dataCount; i += step) {
                g.DrawLine(Pens.Red, x, (float)fGetData(i - step), x + dx, (float)fGetData(i));
                x += dx * step;
            }
        }

        private void panel1_Resize(object sender, EventArgs e) {
            (sender as Panel).Invalidate();
        }

        private void panel2_Paint(object sender, PaintEventArgs e) {
            if (_filters != null)
                for (int index = 1; index < 20; index++ )
                    DrawSamples(panel2, e.Graphics, _filters[index], _filters[index].Length, 0.0f, 1.0f, 1);
        }

        private void panel3_Paint(object sender, PaintEventArgs e) {
            var g = e.Graphics;

            if (_currentMfcc == null)
                return;

            float scale = 30.0f;
            float delta = 3;
            float dx = panel3.ClientRectangle.Width / (float)(MFCC_COUNT+1);

            lock (_mfccLock) {
                for (int i = 1; i < MFCC_COUNT; i++) {
                    g.FillRectangle(Brushes.Red, i * dx + delta, (float)panel3.ClientRectangle.Height - scale * (float)_currentMfcc[i],
                        dx - delta, scale * (float)Math.Abs(_currentMfcc[i]));
                }

                using (var g2 = panel1.CreateGraphics()) {
                    float dist = (float) _currentWindow / (float)_wave.SamplesCount;
                    g2.FillRectangle(Brushes.Black, 0, 0, panel1.ClientRectangle.Width, 30);
                    g2.FillRectangle(Brushes.Yellow, dist*panel1.ClientRectangle.Width, 0.0f, 30.0f, 30.0f);
                }
            }
        }

        private const int MFCC_COUNT = 16; 
        private double[] _currentMfcc;
        private double _currentWindow;
        private object _mfccLock = new object();
        private Thread _computeMfcc;

        private void ComputeMFCC() {
            int windowSize = 1024;
            int overlap = 512;
            int windowsDelta = windowSize - overlap;

            double[][] filters = TriFilterBank.CreateFiltersBank(20, 1024, _wave.Frequency, 0, 4800);
            
            double[] windowData = new double[windowSize];
            for (int i = 0; i < _wave.SamplesCount - windowSize; i += windowsDelta) {
                for (int j = 0; j < windowSize; j++)
                    windowData[j] = _wave.SoundSamples[i + j];
                // Change na MFCC i MFCC_COUNT
                // var mfcc = MFCCCoefficients.GetFurierSpectrum(windowData, filters, MFCC_COUNT);
                var mfcc = MFCCCoefficients.GetMFCC(windowData, filters, MFCC_COUNT);

                lock (_mfccLock) {
                    for (int k = 0; k < mfcc.Length; k++)
                        _currentMfcc[k] = mfcc[k];
                    _currentWindow = i;
                }

                Thread.Sleep(100);
            }
        }

        private void realTimeMFCCToolStripMenuItem_Click(object sender, EventArgs e) {
            if (_computeMfcc != null) {
                _computeMfcc.Abort();
            }

            if (_currentMfcc == null) {
                _currentMfcc = new double[MFCC_COUNT];
            }

            _computeMfcc = new Thread(ComputeMFCC);
            _computeMfcc.Start();

        }

        private void timer1_Tick(object sender, EventArgs e) {
            if (_currentMfcc != null) {
                panel3.Invalidate();
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
            if (_computeMfcc != null)
                _computeMfcc.Abort();
        }

    }
}
